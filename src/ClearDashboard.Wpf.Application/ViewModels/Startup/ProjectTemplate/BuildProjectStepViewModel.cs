using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Features;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Application.ViewStartup.ProjectTemplate;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DataAccessLayer;
using Corpus = ClearDashboard.DAL.Alignment.Corpora.Corpus;
using CorpusType = ClearDashboard.DataAccessLayer.Models.CorpusType;
using Point = System.Windows.Point;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate
{
    public class BuildProjectStepViewModel : DashboardApplicationWorkflowStepViewModel<StartupDialogViewModel>, IHandle<BackgroundTaskChangedMessage>
    {
        private readonly ProjectTemplateProcessRunner _processRunner;
        private Task _runningTask = Task.CompletedTask;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private CancellationToken? _cancellationToken = null;
        private ProjectDbContextFactory _projectNameDbContextFactory;

        private string _createAction;
        private string _backAction;
        private string _cancelAction;

        private string _backOrCancelAction;
        public string BackOrCancelAction
        {
            get => _backOrCancelAction;
            set => Set(ref _backOrCancelAction, value);
        }

        private string _createOrCloseAction;
        public string CreateOrCloseAction
        {
            get => _createOrCloseAction;
            set => Set(ref _createOrCloseAction, value);
        }

        private Visibility _progressIndicatorVisibility = Visibility.Hidden;
        public Visibility ProgressIndicatorVisibility
        {
            get => _progressIndicatorVisibility;
            set => Set(ref _progressIndicatorVisibility, value);
        }

        public BindableCollection<string> Messages = new BindableCollection<string>();

        /// <summary>
        /// This is the design surface that is displayed in the window.
        /// It is the main part of the view-model.
        /// </summary>
        private ReadonlyProjectDesignSurfaceViewModel? _designSurfaceViewModel;
        public ReadonlyProjectDesignSurfaceViewModel? ProjectDesignSurfaceViewModel
        {
            get => _designSurfaceViewModel;
            private set => Set(ref _designSurfaceViewModel, value);
        }

        public BuildProjectStepViewModel(DashboardProjectManager projectManager, ProjectTemplateProcessRunner processRunner,
            INavigationService navigationService, ILogger<ProjectSetupViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, ILocalizationService localizationService,
            ProjectDbContextFactory projectNameDbContextFactory)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _processRunner = processRunner;
            _projectNameDbContextFactory = projectNameDbContextFactory;

            _createAction = LocalizationService!.Get("Create");
            _backAction = LocalizationService!.Get("Back");
            _cancelAction = LocalizationService!.Get("Cancel");

            _backOrCancelAction = _backAction;
            _createOrCloseAction = _createAction;

            
        }
        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _runningTask = Task.CompletedTask;
            _cancellationToken = null;

            BackOrCancelAction = _backAction;
            CreateOrCloseAction = _createAction;

            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;

            CanMoveForwards = ParentViewModel!.SelectedParatextProject != null && (ParentViewModel!.SelectedBookIds?.Any() ?? false);

           
            await ActivateProjectDesignSurface(cancellationToken);

            await Execute.OnUIThreadAsync(async () =>
            {
                ProjectDesignSurfaceViewModel.ProjectName = ParentViewModel!.ProjectName;
                DisplayName = string.Format(LocalizationService!["ProjectPicker_ProjectTemplateWizardTemplate"], ProjectDesignSurfaceViewModel.ProjectName);
                await BuildProjectDesignSurface();
            });
          

            await base.OnActivateAsync(cancellationToken);
        }

        private async Task ActivateProjectDesignSurface(CancellationToken cancellationToken)
        {
            ProjectDesignSurfaceViewModel = LifetimeScope!.Resolve<ReadonlyProjectDesignSurfaceViewModel>();

            await ProjectDesignSurfaceViewModel.Initialize(cancellationToken);


            ProjectDesignSurfaceViewModel.Parent = this;
            ProjectDesignSurfaceViewModel.ConductWith(this);
            var view = ViewLocator.LocateForModel(ProjectDesignSurfaceViewModel, null, null);
            ViewModelBinder.Bind(ProjectDesignSurfaceViewModel, view, null);
            await ScreenExtensions.TryActivateAsync(ProjectDesignSurfaceViewModel, cancellationToken);
        }

        public async Task CreateOrClose()
        {
            if (!(ParentViewModel!.SelectedBookIds?.Any() ?? false) || ParentViewModel!.SelectedParatextProject is null)
            {
                // FIXME:  
                await ParentViewModel!.GoToStep(1);
                return;
            }

           

            _cancellationToken = _cancellationTokenSource.Token;

            BackOrCancelAction = _cancelAction;
            CreateOrCloseAction = _createAction;
            ProgressIndicatorVisibility = Visibility.Visible;

            CanMoveForwards = false;
            CanMoveBackwards = true;

            await RegisterProjectCreationTasks();
            await CreateProject(_cancellationToken);
        }

        private StartupDialogViewModel _startupDialogViewModel;

        private async Task CreateProject(CancellationToken? cancellationToken)
        {
            Stopwatch sw = new();
            sw.Start();

            // NB:  need to store a reference to the Parent view model so we can clean up in the finally block below;
            _startupDialogViewModel = ParentViewModel;

            Action<ILogger>? errorCleanupAction = null;
            ProjectManager!.PauseDenormalization = true;
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();

                _runningTask = Task.Run(async () =>
                    await ProjectManager!.CreateNewProject(ParentViewModel!.ProjectName));
                await _runningTask;

                cancellationToken?.ThrowIfCancellationRequested();

                _runningTask = _processRunner.RunRegisteredTasks(sw);
                await _runningTask;

                PlaySound.PlaySoundFromResource();

                await ProjectDesignSurfaceViewModel!.SaveDesignSurfaceData();

                await TryCloseAsync(true);
            }
            catch (OperationCanceledException)
            {
                errorCleanupAction = await GetErrorCleanupAction(ParentViewModel!.ProjectName);
                ProjectManager!.CurrentDashboardProject.ProjectName = null;
                ProjectManager!.CurrentProject = null;

                PlaySound.PlaySoundFromResource(SoundType.Error);

                await _startupDialogViewModel!.GoToStep(1);
            }
            catch (Exception)
            {
                errorCleanupAction = await GetErrorCleanupAction(ParentViewModel!.ProjectName);
                ProjectManager!.CurrentDashboardProject.ProjectName = null;
                ProjectManager!.CurrentProject = null;

                PlaySound.PlaySoundFromResource(SoundType.Error);

                await _startupDialogViewModel!.GoToStep(1);
            }
            finally
            {
                _runningTask.Dispose();
                _runningTask = Task.CompletedTask;

                _cancellationToken = null;

                ProgressIndicatorVisibility = Visibility.Hidden;
                CanMoveForwards = false;
                CanMoveBackwards = false;

                errorCleanupAction?.Invoke(Logger!);

                ProjectManager!.PauseDenormalization = false;
              

                // Reset everything in case the wizard is activated again:
                _startupDialogViewModel!.SelectedParatextProject = null;
                _startupDialogViewModel!.SelectedParatextBtProject = null;
                _startupDialogViewModel!.SelectedParatextLwcProject = null;
                _startupDialogViewModel!.IncludeBiblicalTexts = true;
                _startupDialogViewModel!.SelectedBookIds = null;

                await _startupDialogViewModel!.TryCloseAsync(true);

                sw.Stop();
            }
        }

        private async Task RegisterProjectCreationTasks()
        {
            _processRunner.StartRegistration();

            string? manuscriptHebrewTaskName = null;
            string? manuscriptGreekTaskName = null;
            if (ParentViewModel!.IncludeBiblicalTexts)
            {
                manuscriptHebrewTaskName = _processRunner.RegisterManuscriptCorpusTask(CorpusType.ManuscriptHebrew);
                manuscriptGreekTaskName = _processRunner.RegisterManuscriptCorpusTask(CorpusType.ManuscriptGreek);
            }

            var paratextTaskName = _processRunner.RegisterParatextProjectCorpusTask(
                ParentViewModel!.SelectedParatextProject, Tokenizers.LatinWordTokenizer, ParentViewModel!.SelectedBookIds);
            string? paratextBtTaskName = null;
            string? paratextLwcTaskName = null;

            if (ParentViewModel!.SelectedParatextBtProject is not null)
            {
                paratextBtTaskName = _processRunner.RegisterParatextProjectCorpusTask(
                    ParentViewModel!.SelectedParatextBtProject, Tokenizers.LatinWordTokenizer,
                    ParentViewModel!.SelectedBookIds);
            }

            if (ParentViewModel!.SelectedParatextLwcProject is not null)
            {
                paratextLwcTaskName = _processRunner.RegisterParatextProjectCorpusTask(
                    ParentViewModel!.SelectedParatextLwcProject, Tokenizers.LatinWordTokenizer,
                    ParentViewModel!.SelectedBookIds);

                var taskNameSetParatextLwc = _processRunner.RegisterParallelizationTasks(
                    paratextTaskName,
                    paratextLwcTaskName,
                    true,
                    SmtModelType.FastAlign.ToString());
            }

            if (paratextLwcTaskName is not null && paratextBtTaskName is not null)
            {
                var taskNameSetLwcBt = _processRunner.RegisterParallelizationTasks(
                    paratextLwcTaskName,
                    paratextBtTaskName,
                    true,
                    SmtModelType.FastAlign.ToString());
            }

            if (manuscriptHebrewTaskName is not null && paratextLwcTaskName is not null)
            {
                var taskNameLwcHebrewBt = _processRunner.RegisterParallelizationTasks(
                    paratextLwcTaskName,
                    manuscriptHebrewTaskName,
                    false,
                    SmtModelType.FastAlign.ToString());
            }

            if (manuscriptGreekTaskName is not null)
            {
                var taskNameParatextGreekBt = _processRunner.RegisterParallelizationTasks(
                    paratextTaskName,
                    manuscriptGreekTaskName,
                    false,
                    SmtModelType.FastAlign.ToString());
            }

            await Task.CompletedTask;
        }

        private CorpusNodeViewModel BuildCorpusNode(Corpus corpus, Point designSurfaceLocation)
        {

           return ProjectDesignSurfaceViewModel!.DesignSurfaceViewModel!.CreateCorpusNode(corpus, designSurfaceLocation);

            

            //var corpus2 = new Corpus(new CorpusId(Guid.NewGuid()) { Name = "Node2" });
            //var node2 = ProjectDesignSurfaceViewModel.DesignSurfaceViewModel.CreateCorpusNode(corpus2, new Point(300, 50));

            //if (node1 is not null && node2 is not null)
            //{
            //    var connection = new ParallelCorpusConnectionViewModel
            //    {
            //        SourceConnector = node1.OutputConnectors[0],
            //        DestinationConnector = node2.InputConnectors[0],
            //        ParallelCorpusDisplayName = "Test Connection",
            //        ParallelCorpusId = new ParallelCorpusId(Guid.NewGuid()),
            //        SourceFontFamily = FontFamily.Families[0].Name,
            //        TargetFontFamily = FontFamily.Families[0].Name,
            //    };
            //    ProjectDesignSurfaceViewModel.DesignSurfaceViewModel.ParallelCorpusConnections.Add(connection);
            //    // add in the context menu
            //    //ProjectDesignSurfaceViewModel.DesignSurfaceViewModel!.CreateParallelCorpusConnectionMenu(connection, topLevelProjectIds);
            //}
        }



        private async Task BuildProjectDesignSurface()
        {
  
            CorpusNodeViewModel? paratextBackTranslationNode = null;
            CorpusNodeViewModel? manuscriptHebrewNode = null;
            CorpusNodeViewModel? manuscriptGreekNode = null;
            CorpusNodeViewModel? paratextLwcNode = null;
            CorpusNodeViewModel? paratextNode = null;

            paratextNode = BuildCorpusNode(new Corpus(new CorpusId(Guid.NewGuid())
            {
                Name = ParentViewModel!.SelectedParatextProject.Name,
                FontFamily = ParentViewModel!.SelectedParatextProject.FontFamily,
                CorpusType = ParentViewModel!.SelectedParatextProject.CorpusTypeDisplay
            }), new Point(50, 50));

            if (ParentViewModel!.IncludeBiblicalTexts)
            {
                manuscriptHebrewNode = BuildCorpusNode(new Corpus(new CorpusId(ManuscriptIds.HebrewManuscriptGuid)
                {
                    Name = MaculaCorporaNames.HebrewCorpusName, 
                    FontFamily = FontNames.HebrewFontFamily, 
                    CorpusType = CorpusType.ManuscriptHebrew.ToString()
                }), new Point(300, 200)); ;
                manuscriptGreekNode = BuildCorpusNode(new Corpus(new CorpusId(ManuscriptIds.GreekManuscriptGuid) { Name = MaculaCorporaNames.GreekCorpusName, FontFamily = FontNames.GreekFontFamily, CorpusType = CorpusType.ManuscriptGreek.ToString() }), new Point(300, 275));
            }

            if (ParentViewModel!.SelectedParatextBtProject is not null)
            {
                paratextBackTranslationNode = BuildCorpusNode(new Corpus(new CorpusId(Guid.NewGuid())
                {
                    Name = ParentViewModel!.SelectedParatextBtProject.Name, 
                    FontFamily = ParentViewModel!.SelectedParatextBtProject.FontFamily, 
                    CorpusType = ParentViewModel!.SelectedParatextBtProject.CorpusTypeDisplay
                }), new Point(300, 50));
            }

            if (ParentViewModel!.SelectedParatextLwcProject is not null)
            {
                paratextLwcNode = BuildCorpusNode(new Corpus(new CorpusId(Guid.NewGuid())
                {
                    Name = ParentViewModel!.SelectedParatextLwcProject.Name,
                    FontFamily = ParentViewModel!.SelectedParatextLwcProject.FontFamily,
                    CorpusType = ParentViewModel!.SelectedParatextLwcProject.CorpusTypeDisplay
                }), new Point(300, 125));
            }

            if (paratextNode is not null && paratextBackTranslationNode is not null)
            {
                var connection = new ParallelCorpusConnectionViewModel
                {
                    SourceConnector = paratextNode.OutputConnectors[0],
                    DestinationConnector = paratextBackTranslationNode.InputConnectors[0],
                    ParallelCorpusDisplayName = $"{paratextNode.Name} - {paratextBackTranslationNode.Name}",
                    ParallelCorpusId = new ParallelCorpusId(Guid.NewGuid()),
                    SourceFontFamily = ParentViewModel!.SelectedParatextProject.FontFamily,
                    TargetFontFamily = ParentViewModel!.SelectedParatextBtProject.FontFamily,
                }; 
                ProjectDesignSurfaceViewModel.DesignSurfaceViewModel.ParallelCorpusConnections.Add(connection);
            }

            if (paratextNode != null && paratextLwcNode != null)
            {
                var connection = new ParallelCorpusConnectionViewModel
                {
                    SourceConnector = paratextNode.OutputConnectors[0],
                    DestinationConnector = paratextLwcNode.InputConnectors[0],
                    ParallelCorpusDisplayName = $"{paratextNode.Name} - {paratextLwcNode.Name}",
                    ParallelCorpusId = new ParallelCorpusId(Guid.NewGuid()),
                    SourceFontFamily = ParentViewModel!.SelectedParatextProject.FontFamily,
                    TargetFontFamily = ParentViewModel!.SelectedParatextLwcProject.FontFamily,
                };
                ProjectDesignSurfaceViewModel.DesignSurfaceViewModel.ParallelCorpusConnections.Add(connection);
            }

            if (paratextNode != null && manuscriptHebrewNode != null)
            {
                var connection = new ParallelCorpusConnectionViewModel
                {
                    SourceConnector = paratextNode.OutputConnectors[0],
                    DestinationConnector = manuscriptHebrewNode.InputConnectors[0],
                    ParallelCorpusDisplayName = $"{paratextNode.Name} - {manuscriptHebrewNode.Name}",
                    ParallelCorpusId = new ParallelCorpusId(Guid.NewGuid()),
                    SourceFontFamily = ParentViewModel!.SelectedParatextProject.FontFamily,
                    TargetFontFamily = FontNames.HebrewFontFamily,
                };
                ProjectDesignSurfaceViewModel.DesignSurfaceViewModel.ParallelCorpusConnections.Add(connection);
            }

            if (paratextNode != null && manuscriptGreekNode != null)
            {
                var connection = new ParallelCorpusConnectionViewModel
                {
                    SourceConnector = paratextNode.OutputConnectors[0],
                    DestinationConnector = manuscriptGreekNode.InputConnectors[0],
                    ParallelCorpusDisplayName = $"{paratextNode.Name} - {manuscriptGreekNode.Name}",
                    ParallelCorpusId = new ParallelCorpusId(Guid.NewGuid()),
                    SourceFontFamily = ParentViewModel!.SelectedParatextProject.FontFamily,
                    TargetFontFamily = FontNames.GreekFontFamily,
                };

                ProjectDesignSurfaceViewModel.DesignSurfaceViewModel.ParallelCorpusConnections.Add(connection);
            }

            await Task.CompletedTask;
        }

        private async Task<Action<ILogger>?> GetErrorCleanupAction(string projectName)
        {
            await using (var requestScope = _projectNameDbContextFactory!.ServiceScope
                .BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
            {
                await using (var projectContext = await _projectNameDbContextFactory!.GetDatabaseContext(
                    projectName,
                    false,
                    requestScope).ConfigureAwait(false))
                {
                    return InitializeDatabaseCommandHandler.GetErrorCleanupAction(projectContext, Logger!);
                }
            }
        }

        public async Task BackOrCancel()
        {
            if (_cancellationToken is not null && !(_cancellationToken?.IsCancellationRequested ?? false))
            {
                CanMoveBackwards = false;

                _cancellationTokenSource?.Cancel();
                _processRunner.Cancel();

                await Task.WhenAny(tasks: new Task[] { _runningTask, Task.Delay(30000) });
            }
            else
            {
                await ParentViewModel!.GoToStep(4);
            }
        }

        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            Logger!.LogInformation($"RunProcessViewModel:  {message.Status.TaskLongRunningProcessStatus}: {message.Status.Name}: {message.Status.Description}");
            await Task.CompletedTask;
        }
    }
}
