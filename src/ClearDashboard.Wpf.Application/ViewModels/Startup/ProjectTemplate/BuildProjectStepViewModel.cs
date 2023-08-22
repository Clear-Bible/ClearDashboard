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
using ClearDashboard.DAL.Alignment;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate
{
    public class BuildProjectStepViewModel : DashboardApplicationWorkflowStepViewModel<StartupDialogViewModel>, IHandle<BackgroundTaskChangedMessage>
    {
        private readonly ProjectTemplateProcessRunner _processRunner;
        private Task _runningTask = Task.CompletedTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private CancellationToken? _cancellationToken;
        private readonly ProjectDbContextFactory _projectNameDbContextFactory;

        private readonly string _createAction;
        private readonly string _backAction;
        private readonly string _cancelAction;

        private bool _showProjectOverviewMessage;
        public bool ShowProjectOverviewMessage
        {
            get => _showProjectOverviewMessage;
            set => Set(ref _showProjectOverviewMessage, value);
        }

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

        public BindableCollection<string> Messages => _messages;

        public ProjectBuilderStatusViewModel BackgroundTasksViewModel => _backgroundTasksViewModel;

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

        private StartupDialogViewModel? _startupDialogViewModel;
    
        private readonly BindableCollection<string> _messages = new BindableCollection<string>();
        private readonly ProjectBuilderStatusViewModel _backgroundTasksViewModel;

        public BuildProjectStepViewModel(DashboardProjectManager projectManager, 
                                         ProjectTemplateProcessRunner processRunner,
                                         ProjectBuilderStatusViewModel backgroundTasksViewModel,
                                         INavigationService navigationService, 
                                         ILogger<ProjectSetupViewModel> logger, 
                                         IEventAggregator eventAggregator, 
                                         IMediator mediator, 
                                         ILifetimeScope? lifetimeScope,  
                                         ILocalizationService localizationService, 
                                         ProjectDbContextFactory projectNameDbContextFactory)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _backgroundTasksViewModel = backgroundTasksViewModel;
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
            ShowProjectOverviewMessage = true;

            CanMoveForwards = ParentViewModel!.SelectedParatextProject != null && (ParentViewModel!.SelectedBookIds?.Any() ?? false);

            await ActivateProjectDesignSurface(cancellationToken);

            await Execute.OnUIThreadAsync(async () =>
            {
                ProjectDesignSurfaceViewModel!.ProjectName = ParentViewModel!.ProjectName;
                DisplayName = string.Format(LocalizationService!["ProjectPicker_ProjectTemplateWizardTemplate"], ProjectDesignSurfaceViewModel.ProjectName);
                await BuildProjectDesignSurface();
            });

            await BackgroundTasksViewModel.ActivateAsync();


            await base.OnActivateAsync(cancellationToken);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await BackgroundTasksViewModel.DeactivateAsync(true);
            await base.OnDeactivateAsync(close, cancellationToken);
        }

        private async Task ActivateProjectDesignSurface(CancellationToken cancellationToken)
        {
            await Initialize(cancellationToken);
        }

        public override async Task Reset(CancellationToken cancellationToken)
        {
            await Initialize(cancellationToken);
            await base.Reset(cancellationToken);
        }

        public override async Task Initialize(CancellationToken cancellationToken)
        {
            ProjectDesignSurfaceViewModel = LifetimeScope!.Resolve<ReadonlyProjectDesignSurfaceViewModel>();

            if (ProjectDesignSurfaceViewModel is null)
            {
                throw new NullReferenceException(
                    "Please register 'ReadonlyProjectDesignSurfaceViewModel' in ApplicationModule.");
            }

            await ProjectDesignSurfaceViewModel.Initialize(cancellationToken);


            ProjectDesignSurfaceViewModel.Parent = this;
            ProjectDesignSurfaceViewModel.ConductWith(this);
            var view = ViewLocator.LocateForModel(ProjectDesignSurfaceViewModel, null, null);
            ViewModelBinder.Bind(ProjectDesignSurfaceViewModel, view, null);
            await ScreenExtensions.TryActivateAsync(ProjectDesignSurfaceViewModel, cancellationToken);

            await base.Initialize(cancellationToken);
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

            CanMoveForwards = false;
            CanMoveBackwards = true;
            ShowProjectOverviewMessage = false;

            await RegisterProjectCreationTasks();
            await CreateProject(_cancellationToken);
        }

       

        private async Task CreateProject(CancellationToken? cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            // NB:  need to store a reference to the Parent view model so we can clean up in the finally block below;
            _startupDialogViewModel = ParentViewModel;

            Action<ILogger>? errorCleanupAction = null;
            ProjectManager!.PauseDenormalization = true;
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();

               
                _runningTask = Task.Run(async () => { await CreateNewProject(cancellationToken?? CancellationToken.None); });
            
                   
                await _runningTask;

                cancellationToken?.ThrowIfCancellationRequested();

                _runningTask = _processRunner.RunRegisteredTasks(stopwatch);
                await _runningTask;


                await UpdateDesignSurfaceData();

                _startupDialogViewModel!.Reset();

                PlaySound.PlaySoundFromResource();

                await _startupDialogViewModel!.TryCloseAsync(true);

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

                //ProgressIndicatorVisibility = Visibility.Hidden;
                CanMoveForwards = false;
                CanMoveBackwards = false;

                errorCleanupAction?.Invoke(Logger!);

                ProjectManager!.PauseDenormalization = false;
         
                stopwatch.Stop();
            }
        }

        private async Task UpdateDesignSurfaceData()
        {
           
            var projectDesignSurfaceData =
                ProjectDesignSurfaceViewModel!.LoadDesignSurfaceData(ProjectManager!.CurrentProject);

            if (projectDesignSurfaceData == null)
            {
                return;
            }

            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

            foreach (var topLevelProjectId in topLevelProjectIds.CorpusIds)
            {
                // find the correct node via the Corprus name
                var node = projectDesignSurfaceData.CorpusNodeLocations.FirstOrDefault(l => l.CorpusName == topLevelProjectId.Name);
                if (node != null)
                {
                    // Set the CorpusId to the Id of the Corpus saved in the database
                    // so we can properly draw the PDS when the project is loaded in the main app
                    node.CorpusId = topLevelProjectId.Id;
                }
            }

            ProjectManager.CurrentProject.DesignSurfaceLayout = ProjectDesignSurfaceViewModel.SerializeDesignSurface(projectDesignSurfaceData);
            await ProjectManager.UpdateCurrentProject();
        }

        private async Task CreateNewProject(CancellationToken cancellationToken)
        {
            var name = "Creating Project";
            await SendBackgroundStatus(name, LongRunningTaskStatus.Running, cancellationToken,
                $"Creating project '{ParentViewModel!.ProjectName}'");
            await ProjectManager!.CreateNewProject(ParentViewModel!.ProjectName);
            ProjectManager.CurrentProject.DesignSurfaceLayout =
                ProjectDesignSurfaceViewModel!.SerializeDesignSurface();
            await ProjectManager.UpdateCurrentProject();
            await Task.Delay(250, cancellationToken);
            await SendBackgroundStatus(name, LongRunningTaskStatus.Completed, cancellationToken,
                $"Created project '{ParentViewModel!.ProjectName}'");
        }

        private async Task RegisterProjectCreationTasks()
        {
            _processRunner.StartRegistration();

            var paratextTaskName = _processRunner.RegisterParatextProjectCorpusTask(
                ParentViewModel!.SelectedParatextProject!, Tokenizers.LatinWordTokenizer, ParentViewModel!.SelectedBookIds!);

            if (ParentViewModel!.IncludeBiblicalTexts)
            {
                if (ParentViewModel.SelectedBookManager.HasSelectedAndEnabledOldTestamentBooks)
                {
                    var manuscriptHebrewTaskName =
                        _processRunner.RegisterManuscriptCorpusTask(CorpusType.ManuscriptHebrew);

                    _ = _processRunner.RegisterParallelizationTasks(
                        paratextTaskName,
                        manuscriptHebrewTaskName,
                        false,
                        SmtModelType.FastAlign.ToString());
                }

                if (ParentViewModel.SelectedBookManager.HasSelectedAndEnabledNewTestamentBooks)
                {
                    var manuscriptGreekTaskName =
                        _processRunner.RegisterManuscriptCorpusTask(CorpusType.ManuscriptGreek);

                    _ = _processRunner.RegisterParallelizationTasks(
                        paratextTaskName,
                        manuscriptGreekTaskName,
                        false,
                        SmtModelType.FastAlign.ToString());
                }
            }

            if (ParentViewModel!.SelectedParatextBtProject is not null)
            {
                var paratextBtTaskName = _processRunner.RegisterParatextProjectCorpusTask(
                    ParentViewModel!.SelectedParatextBtProject, Tokenizers.LatinWordTokenizer,
                    ParentViewModel!.SelectedBookIds!);

                _ = _processRunner.RegisterParallelizationTasks(
                    paratextTaskName,
                    paratextBtTaskName,
                    true,
                    SmtModelType.FastAlign.ToString());
            }

            if (ParentViewModel!.SelectedParatextLwcProject is not null)
            {
                var paratextLwcTaskName = _processRunner.RegisterParatextProjectCorpusTask(
                    ParentViewModel!.SelectedParatextLwcProject, Tokenizers.LatinWordTokenizer,
                    ParentViewModel!.SelectedBookIds!);

                _ = _processRunner.RegisterParallelizationTasks(
                    paratextTaskName,
                    paratextLwcTaskName,
                    true,
                    SmtModelType.FastAlign.ToString());
            }

            await Task.CompletedTask;
        }

        private CorpusNodeViewModel BuildCorpusNode(Corpus corpus, Point designSurfaceLocation)
        {
           return ProjectDesignSurfaceViewModel!.DesignSurfaceViewModel!.CreateCorpusNode(corpus, designSurfaceLocation);
        }

        private Point GetNextPoint(ref int index)
        {
            Point point = new Point();
            switch (index)
            {
                case 0:
                {
                    index = index + 1;
                    point = new Point(25, 50);
                    break;
                }
                case 1:
                {
                    index = index + 1;
                    point = new Point(275, 50);
                    break;
                }
                case 2:
                {
                    index = index + 1;
                    point = new Point(275, 125);
                    break;
                }
                case 3:
                {
                    index = index + 1;
                    point = new Point(275, 200);
                    break;
                }
                case 4:
                {
                    index = index + 1;
                    point = new Point(275, 275);
                    break;
                }
            }

            return point;
        }

        private async Task BuildProjectDesignSurface()
        {
            var index = 0;

            // Build the selected Paratext Project node
            var paratextNode = BuildParatextProjectCorpusNode(GetNextPoint(ref index));

            // if selected, build the LWC node and associated connector
            if (ParentViewModel!.SelectedParatextLwcProject is not null)
            {
                var paratextLwcNode = BuildParatextLwcCorpusNode(GetNextPoint(ref index));
                BuildParatextProjectToLwcConnector(paratextNode, paratextLwcNode);
            }

            // if selected build the back translation node and associated connector
            if (ParentViewModel!.SelectedParatextBtProject is not null)
            {
                var paratextBackTranslationNode = BuildParatextBackTranslationCorpusNode(GetNextPoint(ref index));
                BuildParatextProjectToBackTranslationConnector(paratextNode, paratextBackTranslationNode);
            }

            // If selected, build Macula Greek and Macula Hebrew nodes and associated connectors
            if (ParentViewModel!.IncludeBiblicalTexts)
            {
              
                if (ParentViewModel.SelectedBookManager.HasSelectedAndEnabledOldTestamentBooks)
                {
                    var manuscriptHebrewNode = BuildMaculaHebrewCorpusNode(GetNextPoint(ref index));
                    BuildParatextProjectToMaculaHebrewConnector(paratextNode, manuscriptHebrewNode);
                }

                if (ParentViewModel.SelectedBookManager.HasSelectedAndEnabledNewTestamentBooks)
                {
                    var manuscriptGreekNode = BuildMaculaGreekCorpusNode(GetNextPoint(ref index));
                    BuildParatextProjectToMaculaGreekConnector(paratextNode, manuscriptGreekNode);
                }
            }
            await Task.CompletedTask;
        }

        private void BuildParatextProjectToMaculaGreekConnector(CorpusNodeViewModel paratextNode, CorpusNodeViewModel manuscriptGreekNode)
        {
            var connection = new ParallelCorpusConnectionViewModel
            {
                SourceConnector = paratextNode.OutputConnectors[0],
                DestinationConnector = manuscriptGreekNode.InputConnectors[0],
                ParallelCorpusDisplayName = $"{paratextNode.Name} - {manuscriptGreekNode.Name}",
                ParallelCorpusId = new ParallelCorpusId(Guid.NewGuid()),
                SourceFontFamily = ParentViewModel!.SelectedParatextProject!.FontFamily,
                TargetFontFamily = FontNames.GreekFontFamily,
            };

            ProjectDesignSurfaceViewModel!.DesignSurfaceViewModel!.ParallelCorpusConnections.Add(connection);
        }

        private void BuildParatextProjectToMaculaHebrewConnector(CorpusNodeViewModel paratextNode,
            CorpusNodeViewModel manuscriptHebrewNode)
        {
            var connection = new ParallelCorpusConnectionViewModel
            {
                SourceConnector = paratextNode.OutputConnectors[0],
                DestinationConnector = manuscriptHebrewNode.InputConnectors[0],
                ParallelCorpusDisplayName = $"{paratextNode.Name} - {manuscriptHebrewNode.Name}",
                ParallelCorpusId = new ParallelCorpusId(Guid.NewGuid()),
                SourceFontFamily = ParentViewModel!.SelectedParatextProject!.FontFamily,
                TargetFontFamily = FontNames.HebrewFontFamily,
            };
            ProjectDesignSurfaceViewModel!.DesignSurfaceViewModel!.ParallelCorpusConnections.Add(connection);
        }

        private void BuildParatextProjectToLwcConnector(CorpusNodeViewModel paratextNode, CorpusNodeViewModel paratextLwcNode)
        {
            var connection = new ParallelCorpusConnectionViewModel
            {
                SourceConnector = paratextNode.OutputConnectors[0],
                DestinationConnector = paratextLwcNode.InputConnectors[0],
                ParallelCorpusDisplayName = $"{paratextNode.Name} - {paratextLwcNode.Name}",
                ParallelCorpusId = new ParallelCorpusId(Guid.NewGuid()),
                SourceFontFamily = ParentViewModel!.SelectedParatextProject!.FontFamily,
                TargetFontFamily = ParentViewModel!.SelectedParatextLwcProject!.FontFamily,
            };
            ProjectDesignSurfaceViewModel!.DesignSurfaceViewModel!.ParallelCorpusConnections.Add(connection);
        }

        private void BuildParatextProjectToBackTranslationConnector(CorpusNodeViewModel paratextNode,
            CorpusNodeViewModel paratextBackTranslationNode)
        {
            var connection = new ParallelCorpusConnectionViewModel
            {
                SourceConnector = paratextNode.OutputConnectors[0],
                DestinationConnector = paratextBackTranslationNode.InputConnectors[0],
                ParallelCorpusDisplayName = $"{paratextNode.Name} - {paratextBackTranslationNode.Name}",
                ParallelCorpusId = new ParallelCorpusId(Guid.NewGuid()),
                SourceFontFamily = ParentViewModel!.SelectedParatextProject!.FontFamily,
                TargetFontFamily = ParentViewModel!.SelectedParatextBtProject!.FontFamily,
            };
            ProjectDesignSurfaceViewModel!.DesignSurfaceViewModel!.ParallelCorpusConnections.Add(connection);
        }

        private CorpusNodeViewModel BuildParatextLwcCorpusNode(Point point)
        {
           return BuildCorpusNode(new Corpus(new CorpusId(Guid.NewGuid())
            {
                Name = ParentViewModel!.SelectedParatextLwcProject!.Name,
                FontFamily = ParentViewModel!.SelectedParatextLwcProject.FontFamily,
                CorpusType = ParentViewModel!.SelectedParatextLwcProject.CorpusTypeDisplay
            }), point);

        }


        private CorpusNodeViewModel BuildParatextBackTranslationCorpusNode(Point point)
        {
           return BuildCorpusNode(new Corpus(new CorpusId(Guid.NewGuid())
            {
                Name = ParentViewModel!.SelectedParatextBtProject!.Name,
                FontFamily = ParentViewModel!.SelectedParatextBtProject.FontFamily,
                CorpusType = ParentViewModel!.SelectedParatextBtProject.CorpusTypeDisplay
            }), point);
           
        }

        private CorpusNodeViewModel BuildMaculaGreekCorpusNode(Point point)
        {
            return BuildCorpusNode(new Corpus(new CorpusId(ManuscriptIds.GreekManuscriptGuid)
            {
                Name = MaculaCorporaNames.GreekCorpusName, 
                FontFamily = FontNames.GreekFontFamily, 
                CorpusType = CorpusType.ManuscriptGreek.ToString()
            }), point);
        }

        private CorpusNodeViewModel BuildMaculaHebrewCorpusNode(Point point)
        {
            return BuildCorpusNode(new Corpus(new CorpusId(ManuscriptIds.HebrewManuscriptGuid)
            {
                Name = MaculaCorporaNames.HebrewCorpusName,
                FontFamily = FontNames.HebrewFontFamily,
                CorpusType = CorpusType.ManuscriptHebrew.ToString()
            }), point);
           
        }

        private CorpusNodeViewModel BuildParatextProjectCorpusNode(Point point)
        {
            return BuildCorpusNode(new Corpus(new CorpusId(Guid.NewGuid())
            {
                Name = ParentViewModel!.SelectedParatextProject!.Name,
                FontFamily = ParentViewModel!.SelectedParatextProject.FontFamily,
                CorpusType = ParentViewModel!.SelectedParatextProject.CorpusTypeDisplay
            }), point);
        }

        private async Task<Action<ILogger>?> GetErrorCleanupAction(string projectName)
        {
            await using var requestScope = _projectNameDbContextFactory.ServiceScope
                .BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            await using var projectContext = await _projectNameDbContextFactory.GetDatabaseContext(
                projectName,
                false,
                requestScope).ConfigureAwait(false);
            return InitializeDatabaseCommandHandler.GetErrorCleanupAction(projectContext, Logger!);
        }

        // ReSharper disable UnusedMember.Global
        public async Task BackOrCancel()
        {
            if (_cancellationToken is not null && !(_cancellationToken?.IsCancellationRequested ?? false))
            {
                CanMoveBackwards = false;

                _cancellationTokenSource.Cancel();
                _processRunner.Cancel();

                await Task.WhenAny(tasks: new [] { _runningTask, Task.Delay(30000) });
            }
            else
            {
                await ParentViewModel!.GoToStep(4);
            }
        }
        // ReSharper restore UnusedMember.Global

        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            var displayMessage = $"{message.Status.TaskLongRunningProcessStatus}: {message.Status.Name}: {message.Status.Description}";
            Logger!.LogInformation($"RunProcessViewModel: {displayMessage}");
            Execute.OnUIThread(()=> Messages.Insert(0, displayMessage) );
            
            await Task.CompletedTask;
        }
    }
}
