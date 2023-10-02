using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Features;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Application.ViewStartup.ProjectTemplate;
using Dahomey.Json;
using Dahomey.Json.Serialization.Conventions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Corpus = ClearDashboard.DAL.Alignment.Corpora.Corpus;
using CorpusType = ClearDashboard.DataAccessLayer.Models.CorpusType;
using Point = System.Windows.Point;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate
{
    public class BuildProjectStepViewModel : DashboardApplicationWorkflowStepViewModel<StartupDialogViewModel>, IHandle<BackgroundTaskChangedMessage>
    {
        #region Calls to keep screen on and not allow sleep mode

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);


        #endregion



        #region Member Variables   

        private readonly ProjectTemplateProcessRunner _processRunner;
        private Task _runningTask = Task.CompletedTask;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private CancellationToken? _cancellationToken;
        private readonly ProjectDbContextFactory _projectNameDbContextFactory;

        private readonly string _createAction;
        private readonly string _backAction;
        private readonly string _cancelAction;

        private DataAccessLayer.Models.Project _oldProject;

        /// <summary>
        /// This is the design surface that is displayed in the window.
        /// It is the main part of the view-model.
        /// </summary>
        private ReadonlyProjectDesignSurfaceViewModel? _designSurfaceViewModel;

        private StartupDialogViewModel? _startupDialogViewModel;

        private readonly BindableCollection<string> _messages = new BindableCollection<string>();
        private readonly ProjectBuilderStatusViewModel _backgroundTasksViewModel;

        #endregion //Member Variables


        #region Public Properties

        public BindableCollection<string> Messages => _messages;
        public ProjectBuilderStatusViewModel BackgroundTasksViewModel => _backgroundTasksViewModel;

        public ReadonlyProjectDesignSurfaceViewModel? ProjectDesignSurfaceViewModel
        {
            get => _designSurfaceViewModel;
            private set => Set(ref _designSurfaceViewModel, value);
        }

        #endregion //Public Properties


        #region Observable Properties

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


        private Visibility _cancelTextVisibility = Visibility.Collapsed;
        public Visibility CancelTextVisibility
        {
            get => _cancelTextVisibility;
            set => Set(ref _cancelTextVisibility, value);
        }


        #endregion //Observable Properties


        #region Constructor

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

            _oldProject = ProjectManager!.CurrentProject;

        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            CancelTextVisibility = Visibility.Collapsed;

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

        #endregion //Constructor


        #region Methods

        private void Prevent_ScreenSaver(bool enabled)
        {
            if (enabled)
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            }
            else
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            }
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
            _cancellationTokenSource = new CancellationTokenSource();

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
            ProjectManager!.IsNewlySetFromTemplate = true;
            // NB:  need to store a reference to the Parent view model so we can clean up in the finally block below;
            _startupDialogViewModel = ParentViewModel;

            Action<ILogger>? errorCleanupAction = null;
            ProjectManager!.PauseDenormalization = true;
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();

                _runningTask = Task.Run((Func<Task?>)(async () => { await CreateNewProject(cancellationToken ?? CancellationToken.None); }));

                // turn off ability for computer to go into screen saver mode
                Prevent_ScreenSaver(true);

                await _runningTask;

                cancellationToken?.ThrowIfCancellationRequested();

                _runningTask = _processRunner.RunRegisteredTasks(stopwatch);
                await _runningTask;


                await UpdateDesignSurfaceData();
                await AddInterlinearEnhancedViewItems();

                _startupDialogViewModel!.Reset();

                PlaySound.PlaySoundFromResource();

                await _startupDialogViewModel!.TryCloseAsync(true);

            }
            catch (OperationCanceledException)
            {
                errorCleanupAction = await GetErrorCleanupAction(ParentViewModel!.ProjectName);
                ProjectManager!.CurrentDashboardProject.ProjectName = (_oldProject == null) ? null : _oldProject.ProjectName;
                ProjectManager!.CurrentProject = _oldProject;

                PlaySound.PlaySoundFromResource(SoundType.Error);

                await _startupDialogViewModel!.GoToStep(1);
            }
            catch (Exception)
            {
                errorCleanupAction = await GetErrorCleanupAction(ParentViewModel!.ProjectName);
                ProjectManager!.CurrentDashboardProject.ProjectName = (_oldProject == null) ? null : _oldProject.ProjectName;
                ProjectManager!.CurrentProject = _oldProject;

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

                await EventAggregator.PublishOnUIThreadAsync(new DashboardProjectNameMessage(ProjectManager!.CurrentDashboardProject.ProjectName));

                stopwatch.Stop();

                // turn back on the ability to go into screen saver mode
                Prevent_ScreenSaver(false);
            }
        }

        private async Task AddInterlinearEnhancedViewItems()
        {
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
            var enhancedViewLayouts = new List<EnhancedViewLayout>();
            var enhancedViewLayout = new EnhancedViewLayout
            {
                BBBCCCVVV = $"{ParentViewModel!.SelectedBookManager.SelectedAndEnabledBooks.First().BookNum:000}{1:000}{1:000}",
                ParatextSync = false,
                Title = "⳼ View",
                VerseOffset = 0
            };
            
            var parallelCorpusIds = topLevelProjectIds.ParallelCorpusIds.Where(x =>
                x.SourceTokenizedCorpusId!.CorpusId!.ParatextGuid == ParentViewModel!.SelectedParatextProject!.Id);

            foreach (var parallelCorpusId in parallelCorpusIds)
            {
                var translationSets = topLevelProjectIds.TranslationSetIds.Where(translationSet =>
                    translationSet.ParallelCorpusId!.IdEquals(parallelCorpusId));
                foreach (var translationSet in translationSets)
                {

                    var alignmentSet = topLevelProjectIds.AlignmentSetIds.FirstOrDefault(alignmentSet =>
                        alignmentSet.Id == translationSet.AlignmentSetGuid);
                    var metadatum = new InterlinearEnhancedViewItemMetadatum
                    {

                        TranslationSetId = translationSet.Id.ToString(),
                        DisplayName = $"{translationSet.DisplayName} Interlinear",
                        ParallelCorpusDisplayName = translationSet.ParallelCorpusId!.DisplayName,
                        ParallelCorpusId = translationSet.ParallelCorpusId.Id.ToString(),
                        IsRtl = parallelCorpusId.SourceTokenizedCorpusId!.CorpusId!.IsRtl,
                        IsNewWindow = false,
                        IsTargetRtl = parallelCorpusId.TargetTokenizedCorpusId!.CorpusId!.IsRtl,
                        SourceParatextId = parallelCorpusId.SourceTokenizedCorpusId!.CorpusId!.ParatextGuid,
                        TargetParatextId =  parallelCorpusId.TargetTokenizedCorpusId.CorpusId.ParatextGuid
                    };
                    enhancedViewLayout.EnhancedViewItems.Add(metadatum);
                }
            }

            enhancedViewLayouts.Add(enhancedViewLayout);


            var options = CreateDiscriminatedJsonSerializerOptions();
            ProjectManager!.CurrentProject.WindowTabLayout = JsonSerializer.Serialize(enhancedViewLayouts, options);

            await ProjectManager.UpdateCurrentProject();
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

        private JsonSerializerOptions CreateDiscriminatedJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = false,
            };
            options.SetupExtensions();
            var registry = options.GetDiscriminatorConventionRegistry();
            registry.ClearConventions();
            registry.RegisterConvention(new DefaultDiscriminatorConvention<string>(options, "_t"));

            var registrars = LifetimeScope.Resolve<IEnumerable<IJsonDiscriminatorRegistrar>>();
            foreach (var registrar in registrars)
            {
                registrar.Register(registry);
            }
            return options;
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

            if (ParentViewModel!.IncludeOtBiblicalTexts)
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
            }

            if (ParentViewModel!.IncludeNtBiblicalTexts)
            {
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

            if (ParentViewModel.SelectedBookManager.HasSelectedAndEnabledOldTestamentBooks && ParentViewModel!.IncludeOtBiblicalTexts)
            {
                var manuscriptHebrewNode = BuildMaculaHebrewCorpusNode(GetNextPoint(ref index));
                BuildParatextProjectToMaculaHebrewConnector(paratextNode, manuscriptHebrewNode);
            }

            if (ParentViewModel.SelectedBookManager.HasSelectedAndEnabledNewTestamentBooks && ParentViewModel!.IncludeNtBiblicalTexts)
            {
                var manuscriptGreekNode = BuildMaculaGreekCorpusNode(GetNextPoint(ref index));
                BuildParatextProjectToMaculaGreekConnector(paratextNode, manuscriptGreekNode);
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
                CancelTextVisibility = Visibility.Visible;
                
                CanMoveBackwards = false;

                _cancellationTokenSource.Cancel();
                _processRunner.Cancel();

                await Task.WhenAny(tasks: new[] { _runningTask, Task.Delay(30000) });

                ParentViewModel!.ProjectName = string.Empty;
            }
            else
            {
                await ParentViewModel!.GoToStep(5);
            }
        }
        // ReSharper restore UnusedMember.Global

        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            var displayMessage = $"{message.Status.TaskLongRunningProcessStatus}: {message.Status.Name}: {message.Status.Description}";
            Logger!.LogInformation($"RunProcessViewModel: {displayMessage}");
            Execute.OnUIThread(() => Messages.Insert(0, displayMessage));

            await Task.CompletedTask;
        }

        #endregion // Methods
    }
}
