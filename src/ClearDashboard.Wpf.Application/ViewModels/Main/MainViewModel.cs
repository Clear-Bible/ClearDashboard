using Autofac;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;
using Caliburn.Micro;
using ClearApplicationFoundation.Framework.Input;
using ClearApplicationFoundation.LogHelpers;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.Exceptions;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.DashboardSettings;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.Marble;
using ClearDashboard.Wpf.Application.ViewModels.Menus;
using ClearDashboard.Wpf.Application.ViewModels.Notes;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;
using ClearDashboard.Wpf.Application.ViewModels.Popups;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using ClearDashboard.Wpf.Application.Views.Main;
using Dahomey.Json;
using Dahomey.Json.Serialization.Conventions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.Wpf.Application.Helpers;
using DockingManager = AvalonDock.DockingManager;
using Point = System.Drawing.Point;
using ClearDashboard.Wpf.Application.ViewModels.Collaboration;

namespace ClearDashboard.Wpf.Application.ViewModels.Main
{

    public class MainViewModel : Conductor<IScreen>.Collection.AllActive,
                IEnhancedViewManager,
                IHandle<ProgressBarVisibilityMessage>,
                IHandle<ProgressBarMessage>,
                IHandle<UiLanguageChangedMessage>,
                IHandle<ActiveDocumentMessage>,
                IHandle<CloseDockingPane>,
                IHandle<ApplicationWindowSettings>,
                IHandle<FilterPinsMessage>,
                IHandle<BackgroundTaskChangedMessage>,
                IHandle<ReloadProjectMessage>
    {
        #region Member Variables

#nullable disable
        private readonly LongRunningTaskManager _longRunningTaskManager;
        private readonly ILocalizationService _localizationService;
        private readonly CollaborationManager _collaborationManager;
        private ILifetimeScope LifetimeScope { get; }
        private IWindowManager WindowManager { get; }
        private INavigationService NavigationService { get; }
        private NoteManager NoteManager { get; }


        private IEventAggregator EventAggregator { get; }
        private DashboardProjectManager ProjectManager { get; }
        private ILogger<MainViewModel> Logger { get; }

        private DockingManager _dockingManager = new();


        private string _lastLayout = "";

        private WindowSettings _windowSettings;
        #endregion //Member Variables

        #region Public Properties

        private ProjectDesignSurfaceViewModel _projectDesignSurfaceViewModel;
        public ProjectDesignSurfaceViewModel ProjectDesignSurfaceViewModel
        {
            get => _projectDesignSurfaceViewModel;
            set => Set(ref _projectDesignSurfaceViewModel, value);
        }

        private PinsViewModel _pinsViewModel;
        public PinsViewModel PinsViewModel
        {
            get => _pinsViewModel;
            set => Set(ref _pinsViewModel, value);
        }

        private bool _paratextSync = true;
        // ReSharper disable once UnusedMember.Global
        public bool ParatextSync
        {
            get => _paratextSync;
            set => Set(ref _paratextSync, value);
        }
        private bool _isBusy;
        // ReSharper disable once UnusedMember.Global
        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DashboardProject Parameter { get; set; }

        #endregion //Public Properties

        #region Commands


        #endregion  //Commands

        #region Observable Properties

        private Visibility _gridIsVisible = Visibility.Collapsed;
        public Visibility GridIsVisible
        {
            get => _gridIsVisible;
            set => Set(ref _gridIsVisible, value);
        }

        private Visibility _deleteGridIsVisible = Visibility.Collapsed;
        public Visibility DeleteGridIsVisible
        {
            get => _deleteGridIsVisible;
            set => Set(ref _deleteGridIsVisible, value);
        }


        private string _selectedLayoutText;
        public string SelectedLayoutText
        {
            get => _selectedLayoutText;
            set => Set(ref _selectedLayoutText, value);
        }

        private string _windowIdToLoad;
        public string WindowIdToLoad
        {
            get => _windowIdToLoad;
            set => Set(ref _windowIdToLoad, value);
        }

        public async Task CollabProjectManager()
        {
            var localizedString = _localizationService!["MainView_About"];

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 500;
            settings.MinHeight = 500;
            settings.Title = $"{localizedString}";

            var viewModel = IoC.Get<CollabProjectManagementViewModel>();


            IWindowManager manager = new WindowManager();
            await manager.ShowDialogAsync(viewModel, null, settings);
        }

        private async Task ShowCollaborationInitialize()
        {
            if (_collaborationManager.HasRemoteConfigured() && !_collaborationManager.IsCurrentProjectInRepository() && InternetAvailability.IsInternetAvailable())
            {
                var viewModel = LifetimeScope?.Resolve<MergeServerProjectDialogViewModel>();

                if (viewModel is not null)
                {
                    viewModel.ProjectId = ProjectManager.CurrentProject.Id;
                    viewModel.ProjectName = ProjectManager.CurrentProject.ProjectName;
                    viewModel.CommitMessage = "Initial Commit";
                    viewModel.CollaborationDialogAction = CollaborationDialogAction.Initialize;

                    this.WindowManager.ShowDialogAsync(viewModel, null, viewModel.DialogSettings());

                    if (viewModel.CommitSha is not null)
                    {
                        ProjectManager.CurrentProject.LastMergedCommitSha = viewModel.CommitSha;
                        // LastMergedCommitSha value should already be in database
                    }

                    await RebuildMainMenu();
                }
            }
        }

        private async Task ShowCollaborationGetLatest()
        {
            if (_collaborationManager.IsCurrentProjectInRepository() && InternetAvailability.IsInternetAvailable())
            {
                var viewModel = LifetimeScope?.Resolve<MergeServerProjectDialogViewModel>();

                if (viewModel is not null)
                {
                    viewModel.ProjectId = ProjectManager.CurrentProject.Id;
                    viewModel.ProjectName = ProjectManager.CurrentProject.ProjectName;
                    viewModel.CollaborationDialogAction = CollaborationDialogAction.Merge;

                    this.WindowManager.ShowDialogAsync(viewModel, null, viewModel.DialogSettings());

                    if (viewModel.CommitSha is not null)
                    {
                        ProjectManager.CurrentProject.LastMergedCommitSha = viewModel.CommitSha;
                        // LastMergedCommitSha value should already be in database
                    }
                }

                await RebuildMainMenu();
            }
        }

        private async Task ShowCollaborationCommit()
        {
            if (_collaborationManager.IsCurrentProjectInRepository() && !_collaborationManager.AreUnmergedChanges() && InternetAvailability.IsInternetAvailable())
            {
                var viewModel = LifetimeScope?.Resolve<MergeServerProjectDialogViewModel>();

                if (viewModel is not null)
                {
                    viewModel.ProjectId = ProjectManager.CurrentProject.Id;
                    viewModel.ProjectName = ProjectManager.CurrentProject.ProjectName;
                    // FIXME:  need to prompt user:
                    viewModel.CommitMessage = $"Commit issued: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}";
                    viewModel.CollaborationDialogAction = CollaborationDialogAction.Commit;

                    this.WindowManager.ShowDialogAsync(viewModel, null, viewModel.DialogSettings());

                    if (viewModel.CommitSha is not null)
                    {
                        ProjectManager.CurrentProject.LastMergedCommitSha = viewModel.CommitSha;
                        await ProjectManager.UpdateProject(ProjectManager.CurrentProject);
                    }

                    await RebuildMainMenu();
                }
            }
        }


        private BindableCollection<MenuItemViewModel> _menuItems = new();
        public BindableCollection<MenuItemViewModel> MenuItems
        {
            get => _menuItems;
            private set => Set(ref _menuItems, value);
        }

        private PaneViewModel _paneViewModel;
        public PaneViewModel ActiveDocument
        {
            get => _paneViewModel;
            set => Set(ref _paneViewModel, value);
        }

        private BindableCollection<ToolViewModel> _tools = new();
        public BindableCollection<ToolViewModel> Tools
        {
            get => _tools;
            set => Set(ref _tools, value);
        }


        private BindableCollection<PaneViewModel> _documents = new();
        public BindableCollection<PaneViewModel> Documents
        {
            get => _documents;
            set => Set(ref _documents, value);
        }

        private List<Tuple<string, Theme>> Themes { get; }

        //private readonly Tuple<string, Theme> _selectedTheme;
        private Tuple<string, Theme> _selectedTheme;
        public Tuple<string, Theme> SelectedTheme
        {
            get => _selectedTheme;
            //private init
            set
            {
                _selectedTheme = value;
                NotifyOfPropertyChange(() => SelectedTheme);
            }
        }


        private LayoutFile _selectedLayout;
        public LayoutFile SelectedLayout
        {
            get => _selectedLayout;
            set
            {
                _selectedLayout = value;
                NotifyOfPropertyChange(() => SelectedLayout);
            }
        }

        private BindableCollection<LayoutFile> _fileLayouts = new();
        public BindableCollection<LayoutFile> FileLayouts
        {
            get => _fileLayouts;
            private set
            {
                _fileLayouts = value;
                NotifyOfPropertyChange(() => FileLayouts);
            }
        }

        private readonly FlowDirection _windowFlowDirection = FlowDirection.LeftToRight;
        public FlowDirection WindowFlowDirection
        {
            get => _windowFlowDirection;
            init => Set(ref _windowFlowDirection, value);
        }

        private bool _showProgressBar;
        public bool ShowProgressBar
        {
            get => _showProgressBar;
            set => Set(ref _showProgressBar, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        private string _projectName;
        public string ProjectName
        {
            get => _projectName;
            set => Set(ref _projectName, value);
        }

        #endregion //Observable Properties

        #region Constructor

        /// <summary>
        /// Required for design-time support
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public MainViewModel()
        {

            // no-op for design time support
        }


        // ReSharper disable once UnusedMember.Global
        public MainViewModel(INavigationService navigationService,
                             ILogger<MainViewModel> logger,
                             DashboardProjectManager projectManager,
                             IEventAggregator eventAggregator,
                             IWindowManager windowManager,
                             ILifetimeScope lifetimeScope,
                             NoteManager noteManager,
                             LongRunningTaskManager longRunningTaskManager,
                             ILocalizationService localizationService,
                             CollaborationManager collaborationManager)
        {
            _longRunningTaskManager = longRunningTaskManager;
            _localizationService = localizationService;
            _collaborationManager = collaborationManager;


            LifetimeScope = lifetimeScope;
            WindowManager = windowManager;
            NoteManager = noteManager;
            EventAggregator = eventAggregator;
            ProjectManager = projectManager;
            Logger = logger;
            WindowFlowDirection = ProjectManager.CurrentLanguageFlowDirection;
            NavigationService = navigationService;
#pragma warning disable CA1416 // Validate platform compatibility
            Themes = new List<Tuple<string, Theme>>
            {
                new(nameof(Vs2013DarkTheme),new Vs2013DarkTheme()),
                new(nameof(Vs2013LightTheme),new Vs2013LightTheme()),
                new(nameof(AeroTheme),new AeroTheme()),
                new(nameof(Vs2013BlueTheme),new Vs2013BlueTheme()),
                new(nameof(GenericTheme), new GenericTheme()),
                new(nameof(ExpressionDarkTheme),new ExpressionDarkTheme()),
                new(nameof(ExpressionLightTheme),new ExpressionLightTheme()),
                new(nameof(MetroTheme),new MetroTheme()),
                new(nameof(VS2010Theme),new VS2010Theme()),
            };
#pragma warning restore CA1416 // Validate platform compatibility

            this.SelectedTheme = Settings.Default.Theme == MaterialDesignThemes.Wpf.BaseTheme.Dark ? Themes[0] : Themes[1];

        }


        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ProjectManager.CurrentUser.ParatextUserName))
            {
                await ProjectManager.UpdateCurrentUserWithParatextUserInformation();
            }

            await Initialize(cancellationToken);
            await base.OnInitializeAsync(cancellationToken);
        }

        private async Task Initialize(CancellationToken cancellationToken)
        {
            await LoadParatextProjectMetadata(cancellationToken);
            await LoadProject();
            ProjectManager.CheckForCurrentUser();
            await NoteManager!.InitializeAsync();
            await RebuildMainMenu();
            await ActivateDockedWindowViewModels(cancellationToken);
            await LoadAvalonDockLayout();
            await LoadEnhancedViewTabs(cancellationToken);
        }


        private async Task LoadParatextProjectMetadata(CancellationToken cancellationToken)
        {
            var result = await ProjectManager?.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken)!;
            if (result.Success && result.HasData)
            {
                //ProjectMetadata = result.Data;
                ProjectManager!.ProjectMetadata = result.Data;
            }
        }


        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            EventAggregator.SubscribeOnUIThread(this);
            Logger.LogInformation($"Subscribing {nameof(MainViewModel)} to the EventAggregator");
            _dockingManager.ActiveContentChanged += OnActiveContentChanged;
            _dockingManager.DocumentClosed += OnEnhancedViewClosed;
            await base.OnActivateAsync(cancellationToken);
        }

        private void OnEnhancedViewClosed(object sender, DocumentClosedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.Document.Content is EnhancedViewModel enhancedViewModel)
            {
                var removed = Items.Remove(enhancedViewModel);
                if (removed)
                {
                    Logger.LogInformation($"Removed EnhancedView '{e.Document.Title}");
                }
            }

        }

        protected override async Task<Task> OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"{nameof(MainViewModel)} is deactivating.");
            _dockingManager.ActiveContentChanged -= OnActiveContentChanged;
            _dockingManager.DocumentClosed -= OnEnhancedViewClosed;



            if (_lastLayout == "")
            {
                SelectedLayoutText = "Last Saved";
                await SaveAvalonDockLayout();
            }

            await SaveProjectData();
            UnsubscribeFromEventAggregator();

            await PinsViewModel.DeactivateAsync(close);
            // await ProjectDesignSurfaceViewModel.DeactivateAsync(close);

            foreach (var screen in Items)
            {
                await screen.DeactivateAsync(true, cancellationToken);
            }

            // Clear the items in the event the user is switching projects.
            Items.Clear();

            OpenProjectManager.RemoveProjectToOpenProjectList(ProjectManager);

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        private void UnsubscribeFromEventAggregator()
        {
            // unsubscribe to the event aggregator
            Logger.LogInformation($"Unsubscribing {nameof(MainViewModel)} to the EventAggregator");
            EventAggregator?.Unsubscribe(this);
        }

        public async Task SaveProjectData()
        {
            try
            {
                await SaveProjectEnhancedViewTabs();
                await SaveProjectDesignSurface();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }

        private async Task SaveProjectEnhancedViewTabs()
        {
            var enhancedViewLayouts = new List<EnhancedViewLayout>();
            Logger!.LogInformation("Saving ProjectDesignSurface layout.");
            foreach (var item in Items)
            {
                if (item is EnhancedViewModel enhancedViewModel)
                {
                    var id = enhancedViewModel.PaneId;
                    var title = enhancedViewModel.Title;
                    var windowsDockable = _dockingManager.Layout.Descendents()
                        .OfType<LayoutDocument>()
                        .FirstOrDefault(a =>
                        {
                            if (a.Content is not null)
                            {
                                if (a.Content is EnhancedViewModel)
                                {
                                    var vm = (EnhancedViewModel)a.Content;
                                    if (vm.PaneId == id)
                                    {
                                        return true;
                                    }
                                }
                            }
                            return false;
                        });

                    if (windowsDockable is not null && enhancedViewModel.EnhancedViewLayout is not null)
                    {
                        title = windowsDockable.Title;
                        enhancedViewModel.EnhancedViewLayout.Title = title;
                    }

                    enhancedViewLayouts.Add(enhancedViewModel.EnhancedViewLayout);
                }
            }

            var options = CreateDiscriminatedJsonSerializerOptions();
            ProjectManager.CurrentProject.WindowTabLayout = JsonSerializer.Serialize(enhancedViewLayouts, options);
            await ProjectManager.UpdateProject(ProjectManager.CurrentProject);

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

        private async Task SaveProjectDesignSurface()
        {
            // save the design surface
            // NB:  Call the following results "System.ObjectDisposedException: Instances cannot be resolved and nested lifetimes cannot be created from this LifetimeScope as it (or one of its parent scopes) has already been disposed."
            //await _projectDesignSurfaceViewModel.SaveCanvas();
            // So we're using the following to save the PDS data to the project database
            Logger!.LogInformation("Saving ProjectDesignSurface layout.");
            ProjectManager.CurrentProject.DesignSurfaceLayout = _projectDesignSurfaceViewModel.SerializeDesignSurface();
            await ProjectManager.UpdateProject(ProjectManager.CurrentProject);
        }

        protected override async void OnViewLoaded(object view)
        {
            // send out a notice that the project is loaded up
            await EventAggregator.PublishOnUIThreadAsync(new ProjectLoadCompleteMessage(true));

            base.OnViewLoaded(view);
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);

            // hook up a reference to the windows dock manager
            if (view is MainView currentView)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                _dockingManager = (DockingManager)currentView.FindName("DockManager");

            }
        }


        /// <summary>
        /// Binds the viewmodel to it's view prior to activating so that the OnViewAttached method of the
        /// child viewmodel are called.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <returns></returns>
        private async Task<TViewModel> ActivateItemAsync<TViewModel>(CancellationToken cancellationToken = default)
            where TViewModel : Screen
        {

            // NOTE:  This is the hack to get OnViewAttached and OnViewReady methods to be called on conducted ViewModels.  Also note
            //   OnViewLoaded is not called.

            var viewModel = IoC.Get<TViewModel>();
            viewModel.Parent = this;
            viewModel.ConductWith(this);
            var view = ViewLocator.LocateForModel(viewModel, null, null);
            ViewModelBinder.Bind(viewModel, view, null);
            await ActivateItemAsync(viewModel, cancellationToken);
            return viewModel;
        }


        private async void OnActiveContentChanged(object sender, EventArgs e)
        {
            await DeactivateDockedWindows();
        }

        private async Task LoadProject()
        {
            if (Parameter != null)
            {
                Logger.LogInformation($"Loading project '{Parameter.ProjectName}'.");
                if (Parameter.IsNew)
                {
                    await ProjectManager.CreateNewProject(Parameter.ProjectName);
                    Parameter.IsNew = false;
                }
                else
                {
                    await ProjectManager.LoadProject(Parameter.ProjectName);
                }
            }
        }



        private async Task LoadEnhancedViewTabs(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                var enhancedViews = LoadEnhancedViewTabLayout();

                if (enhancedViews == null)
                {
                    return;
                }

                await Execute.OnUIThreadAsync(async () =>
                {
                    await DrawEnhancedViewTabs(enhancedViews, cancellationToken);
                    await LoadEnhancedViewData(enhancedViews);
                });


                sw.Stop();
                Logger.LogInformation($"LoadEnhancedViewTabs - Total Load Time {enhancedViews.Count} documents in {sw.ElapsedMilliseconds} ms");
            }, cancellationToken);

            await Task.CompletedTask;
        }

        private IEnumerable<EnhancedViewModel> EnhancedViewModels => Items.Where(item => item is EnhancedViewModel).Cast<EnhancedViewModel>();


        private async Task LoadEnhancedViewData(List<EnhancedViewLayout> enhancedViews)
        {
            var orderedViews = enhancedViews.AsParallel().AsOrdered();

            await Parallel.ForEachAsync(orderedViews, new ParallelOptions(), async (enhancedView, cancellationToken) =>
            {

                var enhancedViewModel = EnhancedViewModels.FirstOrDefault(item => item.Title == enhancedView.Title);

                if (enhancedViewModel == null)
                {
                    throw new MissingEnhancedViewModelException(
                        $"Cannot locate an EnhancedViewModel for '{enhancedView.Title}'");
                }

                enhancedViewModel.EnableBcvControl = false;
                try
                {
                    await enhancedViewModel.LoadData(CancellationToken.None);
                }
                finally
                {
                    enhancedViewModel.EnableBcvControl = true;
                }
            });
        }


        private async Task DrawEnhancedViewTabs(List<EnhancedViewLayout> enhancedViewLayouts, CancellationToken cancellationToken)
        {
            try
            {
                var index = 0;
                foreach (var enhancedViewLayout in enhancedViewLayouts)
                {
                    EnhancedViewModel enhancedViewModel = null;
                    if (index == 0)
                    {
                        ++index;
                        enhancedViewModel = (EnhancedViewModel)Items.FirstOrDefault(vm => vm is EnhancedViewModel);
                    }

                    var isNewEnhancedView = enhancedViewModel is null;
                    if (isNewEnhancedView)
                    {

                        // create a new one
                        enhancedViewModel = await ActivateItemAsync<EnhancedViewModel>(cancellationToken);

                        var enhancedViewLayoutDocument = new LayoutDocument
                        {
                            Content = enhancedViewModel,
                            Title = enhancedViewLayout.Title,
                        };


                        AddNewEnhancedViewTab(enhancedViewLayoutDocument);


                    }

                    await enhancedViewModel.Initialize(enhancedViewLayout, null, cancellationToken);
                    await Task.Delay(100, cancellationToken);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "an unexpected error occurred while activating an EnhancedViewModel.");
                throw;
            }
        }

        private void AddNewEnhancedViewTab(LayoutDocument windowDockable)
        {
            var documentPane = _dockingManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
            documentPane?.Children.Add(windowDockable);
        }


        private List<EnhancedViewLayout> LoadEnhancedViewTabLayout()
        {
            if (ProjectManager.CurrentProject?.WindowTabLayout is null || ProjectManager.CurrentProject?.WindowTabLayout == "[null]")
            {
                var newLayouts = new List<EnhancedViewLayout>
                {
                    new EnhancedViewLayout
                    {
                        ParatextSync = true,
                        Title = "⳼ ENHANCED VIEW",
                        VerseOffset = 0
                    }
                };

                return newLayouts;
            }
            var json = ProjectManager.CurrentProject.WindowTabLayout;
            var options = CreateDiscriminatedJsonSerializerOptions();
            var layouts = JsonSerializer.Deserialize<List<EnhancedViewLayout>>(json, options);

            return layouts;
        }

        private async Task ActivateDockedWindowViewModels(CancellationToken cancellationToken)
        {
            Items.Clear();

            // documents
            await ActivateItemAsync<EnhancedViewModel>(cancellationToken);
            // tools
            await ActivateItemAsync<BiblicalTermsViewModel>(cancellationToken);
            PinsViewModel = await ActivateItemAsync<PinsViewModel>(cancellationToken);
            await ActivateItemAsync<TextCollectionsViewModel>(cancellationToken);
            //await ActivateItemAsync<WordMeaningsViewModel>();
            await ActivateItemAsync<MarbleViewModel>(cancellationToken);

            await ActivateItemAsync<NotesViewModel>(cancellationToken);

            _ = await Task.Factory.StartNew(async () =>
            {
                await Execute.OnUIThreadAsync(async () =>
                {
                    // Activate the ProjectDesignSurfaceViewModel - this will call the appropriate
                    // Caliburn.Micro Screen lifecycle methods.  Also note that this will add ProjectDesignSurfaceViewModel 
                    // as the last Screen managed by this conductor implementation.
                    ProjectDesignSurfaceViewModel =
                        await ActivateItemAsync<ProjectDesignSurfaceViewModel>(cancellationToken);
                    await ProjectDesignSurfaceViewModel.Initialize(cancellationToken);

                });
            }, cancellationToken);



            await Task.Delay(1000, cancellationToken);
        }

        private async Task LoadAvalonDockLayout()
        {
            var layoutSerializer = new XmlLayoutSerializer(_dockingManager);

            if (Settings.Default.LastLayout == "")
            {
                // bring up the default layout
                LoadLayout(layoutSerializer, FileLayouts[0].LayoutPath);
            }
            else
            {
                // check to see if the layout exists
                var layoutPath = Settings.Default.LastLayout;
                LoadLayout(layoutSerializer, File.Exists(layoutPath) ? layoutPath : FileLayouts[0].LayoutPath);
            }

            await Task.CompletedTask;
        }

        #endregion //Constructor

        #region Methods

        //public enum Direction
        //{
        //    Up,
        //    Down
        //}

        public ICommand NavigateToNextDocumentForwards => new RelayCommand(param => CycleNextDocuments(Direction.Forwards));
        public ICommand NavigateToNextDocumentBackwards => new RelayCommand(param => CycleNextDocuments(Direction.Backwards));
        private void CycleNextDocuments(Direction direction)
        {
            var documentPane = _dockingManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
            var documents = documentPane?.Children;

            // Only one or no document -> nothing to cycle through
            if (documents.Count < 2)
            {
                return;
            }

            var currentlySelectedDocument = documents.FirstOrDefault(document => document.IsSelected);
            int currentDocumentIndex = currentlySelectedDocument == null ? 0 : documents.IndexOf(currentlySelectedDocument);

            if (currentlySelectedDocument != null)
            {
                currentlySelectedDocument.IsSelected = false;
            }

            switch (direction)
            {
                case Direction.Forwards:
                    {
                        if (currentDocumentIndex == documents.Count - 1)
                        {
                            documents.First().IsSelected = true;
                            return;
                        }

                        var nextDocument = documents
                            .Skip(currentDocumentIndex + 1)
                            .Take(1)
                            .FirstOrDefault();

                        if (nextDocument != null)
                        {
                            nextDocument.IsSelected = true;
                        }
                        break;
                    }

                case Direction.Backwards:
                    {
                        // If first document reached, show last again
                        if (currentDocumentIndex == 0)
                        {
                            documents.Last().IsSelected = true;
                            return;
                        }

                        var nextDocument = documents
                            .Skip(currentDocumentIndex - 1)
                            .Take(1)
                            .FirstOrDefault();

                        if (nextDocument != null)
                        {
                            nextDocument.IsSelected = true;
                        }
                        break;
                    }
            }
        }

        public void LaunchGettingStartedGuide()
        {
            var programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            var path = Path.Combine(programFiles, "Clear Dashboard", "Dashboard_Instructions.pdf");
            if (File.Exists(path))
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo(path)
                {
                    UseShellExecute = true
                };
                p.Start();
            }
            else
            {
                var message = "Dashboard_Instructions.pdf missing.";
                MessageBox.Show(message);
                Logger?.LogInformation(message);
            }
        }

        public void LaunchGettingStartedVideos()
        {
            var videosUrl = new Uri("https://cleardashboard.org/");

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = videosUrl.AbsoluteUri,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        private void ShowLogs()
        {
            var tailBlazerProxy = LifetimeScope.Resolve<TailBlazerProxy>();
            tailBlazerProxy.StartTailBlazer();
        }

        private async void GatherLogs()
        {
            await EventAggregator.PublishOnUIThreadAsync(new GetApplicationWindowSettings());

            var destinationParatextLogPath = Path.Combine(Path.GetTempPath(), "paratext.log");
            var destinationDashboardLogPath = Path.Combine(Path.GetTempPath(), "dashboard.log");

            var destinationScreenShotPath = Path.Combine(Path.GetTempPath(), "screenshot.jpg");
            if (File.Exists(destinationScreenShotPath))
            {
                File.Delete(destinationScreenShotPath);
            }

            // get the paratext log file path
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Paratext93", "ParatextLog.log");

            if (File.Exists(path))
            {
                // the file is probably locked by Paratext so we can't read it so make a copy of it
                try
                {
                    var sourceFile = new FileInfo(path);
                    sourceFile.CopyTo(destinationParatextLogPath, true);
                }
                catch (Exception e)
                {
                    Logger.LogError(e.Message);
                    destinationParatextLogPath = "";
                }
            }

            // get the Dashboard log file
            var dashboardLogPath = IoC.Get<CaptureFilePathHook>();

            if (File.Exists(dashboardLogPath.Path))
            {
                try
                {
                    var sourceLogFile = new FileInfo(dashboardLogPath.Path);
                    sourceLogFile.CopyTo(destinationDashboardLogPath, true);
                }
                catch (Exception e)
                {
                    Logger.LogError(e.Message);
                    destinationDashboardLogPath = "";
                }
            }

            // get a screenshot of the application
            if (_windowSettings is not null)
            {
                try
                {
                    Rectangle bounds = new Rectangle(0, 0, 0, 0);

                    System.Windows.Point screenPoint = new System.Windows.Point((int)_windowSettings.Left, (int)_windowSettings.Top);

                    var monitors = Helpers.Monitor.AllMonitors;
                    Rect workingArea;
                    foreach (var monitor in monitors)
                    {
                        Debug.WriteLine(screenPoint);
                        Debug.WriteLine(monitor.Bounds);

                        if (monitor.Bounds.Contains(screenPoint))
                        {
                            workingArea = monitor.Bounds;
                        }
                    }

                    if (_windowSettings.IsMaximized)
                    {
                        bounds = new Rectangle((int)workingArea.Left, (int)workingArea.Top, (int)workingArea.Width,
                            (int)workingArea.Height);
                    }
                    else
                    {
                        bounds = new Rectangle((int)_windowSettings.Left,
                             (int)_windowSettings.Top,
                             (int)_windowSettings.Width,
                            (int)_windowSettings.Height);

                    }
                    using var bitmap = new Bitmap(bounds.Width, bounds.Height);
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                    }
                    bitmap.Save(destinationScreenShotPath, ImageFormat.Jpeg);
                }
                catch (Exception e)
                {
                    Logger.LogError(e.Message);
                    destinationScreenShotPath = "";
                }
            }

            List<string> files = new();
            if (destinationParatextLogPath != "")
            {
                files.Add(destinationParatextLogPath);
            }

            if (destinationDashboardLogPath != "")
            {
                files.Add(destinationDashboardLogPath);
            }

            if (destinationScreenShotPath != "")
            {
                files.Add(destinationScreenShotPath);
            }

            // open the message window
            ShowSlackMessageWindow(files);
        }


        private void ShowSlackMessageWindow(List<string> files)
        {
            var localizedString = _localizationService!.Get("SlackMessageView_Title");

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 500;
            settings.MinHeight = 500;
            settings.Title = $"{localizedString}";

            var viewModel = IoC.Get<SlackMessageViewModel>();
            viewModel.Files = files;
            viewModel.ParatextUser = ProjectManager.CurrentUser.ParatextUserName!;
            viewModel.DashboardUser = ProjectManager.CurrentUser;
            viewModel.GitLabUser = _collaborationManager.GetConfig();

            IWindowManager manager = new WindowManager();
            manager.ShowDialogAsync(viewModel, null, settings);
        }

        private void ShowAboutWindow()
        {
            var localizedString = _localizationService!["MainView_About"];

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 500;
            settings.MinHeight = 500;
            settings.Title = $"{localizedString}";

            var viewModel = IoC.Get<AboutViewModel>();

            IWindowManager manager = new WindowManager();
            manager.ShowDialogAsync(viewModel, null, settings);
        }

        private async Task AddNewEnhancedView()
        {
            var viewModel = IoC.Get<EnhancedViewModel>();
            viewModel.BcvDictionary = ProjectManager.CurrentParatextProject.BcvDictionary;
            viewModel.CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);
            viewModel.VerseChange = ProjectManager.CurrentVerse;
            viewModel.EnhancedViewLayout = new EnhancedViewLayout();



            // add vm to conductor
            Items.Add(viewModel);

            // figure out how many enhanced views there are and set the title number for the window
            var enhancedViews = Items.Where(w => w is EnhancedViewModel).ToList();

            // make a new document for the windows
            var windowDockable = new LayoutDocument
            {
                Title = $"{viewModel.Title}  ({enhancedViews.Count})",
                Content = viewModel,
                IsActive = true
            };

            AddNewEnhancedViewTab(windowDockable);

            await SaveProjectData();
        }

        private BindableCollection<LayoutFile> GetFileLayouts()
        {
            var id = 0;
            BindableCollection<LayoutFile> fileLayouts = new();
            // add in the default layouts
            var path = Path.Combine(Environment.CurrentDirectory, @"Resources\Layouts");
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.Layout.config");

                foreach (var file in files.Where(f => !f.StartsWith("Project")))
                {
                    var fileInfo = new FileInfo(file);
                    var name = fileInfo.Name.Substring(0, fileInfo.Name.Length - ".Layout.config".Length);

                    fileLayouts.Add(new LayoutFile
                    {
                        LayoutName = name,
                        LayoutID = "StandardLayout:" + id.ToString(),
                        LayoutPath = file,
                        LayoutType = LayoutFile.eLayoutType.Standard
                    });
                    id++;
                }
            }

            // get the project layouts
            if (ProjectManager.CurrentParatextProject.DirectoryPath != null)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                path = Path.Combine(ProjectManager.CurrentParatextProject.DirectoryPath, "shared");
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*.Layout.config");

                    foreach (var file in files)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        string name = fileInfo.Name.Substring(0, fileInfo.Name.Length - ".Layout.config".Length);

                        fileLayouts.Add(new LayoutFile
                        {
                            LayoutName = name,
                            LayoutID = "ProjectLayout:" + id.ToString(),
                            LayoutPath = file,
                            LayoutType = LayoutFile.eLayoutType.Project
                        });
                        id++;
                    }
                }

            }

            return fileLayouts;
        }

        private async Task RebuildMainMenu()
        {
            FileLayouts = GetFileLayouts();
            BindableCollection<MenuItemViewModel> collaborationItems = new()
            {
                // add in the standard menu items
                new MenuItemViewModel
                {
                    Header = "Manage Collaboration Projects", Id = MenuIds.CollaborationManageProjects,
                    ViewModel = this,
                    IsEnabled = _collaborationManager.HasRemoteConfigured() && _collaborationManager.IsCurrentProjectInRepository() && InternetAvailability.IsInternetAvailable()
                },
                // Save Current Layout
                new MenuItemViewModel
                {
                    Header = "Initialize Server with Project", Id = MenuIds.CollaborationInitialize,
                    ViewModel = this,
                    IsEnabled = _collaborationManager.HasRemoteConfigured() && !_collaborationManager.IsCurrentProjectInRepository() && InternetAvailability.IsInternetAvailable()
                },
                new MenuItemViewModel
                {
                    Header = "Get Latest from Server", Id = MenuIds.CollaborationGetLatest,
                    ViewModel = this,
                    IsEnabled = _collaborationManager.IsCurrentProjectInRepository() && InternetAvailability.IsInternetAvailable()
                },
                new MenuItemViewModel
                {
                    Header = "Commit Changes to Server", Id = MenuIds.CollaborationCommit,
                    ViewModel = this,
                    IsEnabled = _collaborationManager.IsCurrentProjectInRepository() && !_collaborationManager.AreUnmergedChanges() && InternetAvailability.IsInternetAvailable()
                },
                //// separator
                //new() { Header = "---------------------------------", Id = MenuIds.Separator, ViewModel = this, },
                //new MenuItemViewModel
                //{
                //    Header = "Git Fetch + Merge", Id = MenuIds.CollaborationFetchMerge,
                //    ViewModel = this,
                //    IsEnabled = _collaborationManager.IsRepositoryInitialized() && InternetAvailability.IsInternetAvailable()
                //},
                //new MenuItemViewModel
                //{
                //    Header = "Git Hard Reset", Id = MenuIds.CollaborationHardReset,
                //    ViewModel = this,
                //    IsEnabled = _collaborationManager.IsRepositoryInitialized() && _collaborationManager.IsCurrentProjectInRepository()
                //},
                //new MenuItemViewModel
                //{
                //    Header = "Create Project Snapshot Backup", Id = MenuIds.CollaborationCreateBackup,
                //    ViewModel = this,
                //    IsEnabled = _collaborationManager.IsRepositoryInitialized() && _collaborationManager.IsCurrentProjectInRepository()
                //},
                //new MenuItemViewModel
                //{
                //    Header = "Dump Differences between Last Merged and Head", Id = MenuIds.CollaborationDumpDifferencesLastMergedHead,
                //    ViewModel = this,
                //    IsEnabled = _collaborationManager.IsRepositoryInitialized() && _collaborationManager.IsCurrentProjectInRepository()
                //},
                //new MenuItemViewModel
                //{
                //    Header = "Dump Differences between Head and Current Database", Id = MenuIds.CollaborationDumpDifferencesHeadCurrentDb,
                //    ViewModel = this,
                //    IsEnabled = _collaborationManager.IsRepositoryInitialized() && _collaborationManager.IsCurrentProjectInRepository()
                //},
            };
            BindableCollection<MenuItemViewModel> layouts = new()
            {
                // add in the standard menu items

                // Save Current Layout
                new MenuItemViewModel
                {
                    Header = "🖫 " + _localizationService!.Get("MainView_LayoutsSave"), Id = MenuIds.Save,
                    ViewModel = this
                },
                
                // Delete Saved Layout
                new MenuItemViewModel
                {
                    Header = "🗑 " +_localizationService!.Get("MainView_LayoutsDelete"), Id = MenuIds.Delete,
                    ViewModel = this,
                },

                // STANDARD LAYOUTS
                new MenuItemViewModel
                {
                    Header = "---- " + _localizationService!.Get("MainView_LayoutsStandardLayouts") + " ----",
                    Id = MenuIds.Separator, ViewModel = this,
                }
            };

            var bFound = false;
            foreach (var fileLayout in FileLayouts)
            {
                // PROJECT LAYOUTS
                if (fileLayout.LayoutID.StartsWith("ProjectLayout:") && bFound == false)
                {
                    layouts.Add(new MenuItemViewModel
                    {
                        Header = "---- " + _localizationService!.Get("MainView_LayoutsProjectLayouts") + " ----",
                        Id = MenuIds.Separator,
                        ViewModel = this,
                    });
                    bFound = true;
                }


                layouts.Add(new MenuItemViewModel
                {
                    Header = "🝆 " + fileLayout.LayoutName,
                    Id = fileLayout.LayoutID,
                    ViewModel = this,
                });
            }

            // initiate the menu system
            var tasksRunning = !_longRunningTaskManager.HasTasks();
            MenuItems.Clear();
            MenuItems = new BindableCollection<MenuItemViewModel>
            {
                // File
                new()
                {
                    Header =_localizationService!.Get("MainView_File"), Id = MenuIds.File, ViewModel = this, IsEnabled = tasksRunning,
                    MenuItems = new BindableCollection<MenuItemViewModel>
                    {
                        // New
                        new() { Header =_localizationService!.Get("MainView_FileNew"), Id = MenuIds.FileNew, ViewModel = this, IsEnabled = true },
                        new() { Header = _localizationService!.Get("MainView_FileOpen"), Id = MenuIds.FileOpen, ViewModel = this, IsEnabled = true }
                    }
                },
                new()
                {
                    // Layouts
                    Header = _localizationService!.Get("MainView_Layouts"), Id = MenuIds.Layout, ViewModel = this,
                    MenuItems = layouts,
                },
                new()
                {
                    // Windows
                    Header = _localizationService!.Get("MainView_Windows"), Id = MenuIds.Window, ViewModel = this,
                    MenuItems = new BindableCollection<MenuItemViewModel>
                    {
                        // Enhanced Corpus
                        new() { Header = "⳼ " + _localizationService!.Get("MainView_WindowsNewEnhancedView"), Id = MenuIds.NewEnhancedCorpus, ViewModel = this, },

                        // separator
                        new() { Header = "---------------------------------", Id = "SeparatorID", ViewModel = this, },

                        // Biblical Terms
                        new() { Header = "🕮 " + _localizationService!.Get("MainView_WindowsBiblicalTerms"), Id = MenuIds.BiblicalTerms, ViewModel = this, },
                        
                        // Enhanced Corpus
                        new() { Header = "⳼ " +_localizationService!.Get("MainView_WindowsEnhancedView"), Id = MenuIds.EnhancedCorpus, ViewModel = this, },
                        
                        // PINS
                        new() { Header = "⍒ " + _localizationService!.Get("MainView_WindowsPINS"), Id = MenuIds.Pins, ViewModel = this, },
                        
                        // Text Collection
                        new() { Header = "🗐 " +_localizationService!.Get("MainView_WindowsTextCollections"), Id = MenuIds.TextCollection, ViewModel = this, },
                        
                        // Word Meanings
                        //new() { Header = "⌺ " + LocalizationStrings.Get("MainView_WindowsWordMeanings", Logger), Id = "WordMeaningsID", ViewModel = this, },
                        
                        // MARBLE
                        new() { Header = "◕ " +_localizationService!.Get("MainView_WindowsMarble"), Id = MenuIds.Marble, ViewModel = this, },

                        // Notes
                        new() { Header = "⌺ " +_localizationService!.Get("MainView_WindowsNotes"), Id = MenuIds.Notes, ViewModel = this, },

                    }
                },
                
                // SETTINGS
                new()
                {
                    Header = _localizationService!.Get("MainView_Settings"), Id =  MenuIds.Settings, ViewModel = this,
                    //MenuItems = new BindableCollection<MenuItemViewModel>
                    //{
                    //    // Gather Logs
                    //    new() { Header = _localizationService!.Get("MainView_ProgramPreferences"), Id = "PreferencesID", ViewModel = this, },
                    //}
                },

                // COLLABORATION
                new()
                {
                    Header = "Collaboration", Id = "CollaborationID", ViewModel = this,
                    MenuItems = collaborationItems,
                },

                // HELP
                new()
                {
                    Header = _localizationService!.Get("MainView_Help"), Id =  MenuIds.Help, ViewModel = this,
                    MenuItems = new BindableCollection<MenuItemViewModel>
                    {
                        // launch Getting Started Guide
                        new() { Header = _localizationService!.Get("MainView_GettingStartedGuide"), Id = MenuIds.GettingStartedGuide, ViewModel = this, },
                        
                        // launch Getting Started Guide
                        new() { Header = "Getting Started Videos", Id = MenuIds.GettingStartedVideos, ViewModel = this, },

                        // Gather Logs
                        new() { Header = _localizationService!.Get("MainView_ShowLog"), Id = MenuIds.ShowLog, ViewModel = this, },

                        // Gather Logs
                        new() { Header = _localizationService!.Get("MainView_GatherLogs"), Id = MenuIds.GatherLogs, ViewModel = this, },
                        // About
                        new() { Header = _localizationService!.Get("MainView_About"), Id = MenuIds.About, ViewModel = this, },

                        // Test reloading a project.
                        //ViewModel merge
                        //new() { Header = "Test reload project ", Id = MenuIds.ReloadProject, ViewModel = this, },
                    }
                },

            };

            await Task.CompletedTask;
        }

        /// <summary>
        /// Save the layout
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public async void OkSave()
        {
            await SaveAvalonDockLayout();
        }

        private async Task SaveAvalonDockLayout()
        {

            var filePath = string.Empty;
            if (SelectedLayout == null)
            {
                // create a new layout
                if (!string.IsNullOrEmpty(SelectedLayoutText))
                {
                    filePath = Path.Combine(ProjectManager.CurrentParatextProject.DirectoryPath!, "shared");
                    if (Directory.Exists(filePath) == false)
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    filePath = Path.Combine(filePath, Helpers.Helpers.SanitizeFileName(SelectedLayoutText) + ".Layout.config");
                }
            }
            else
            {
                // overwrite a layout
                filePath = SelectedLayout.LayoutPath;
            }


            try
            {
                // save the layout
                var layoutSerializer = new XmlLayoutSerializer(this._dockingManager);
                layoutSerializer.Serialize(filePath);
            }
            catch (Exception e)
            {
                this.Logger.LogError(e.Message);
            }
            finally
            {
                GridIsVisible = Visibility.Collapsed;

                await RebuildMainMenu();
            }
        }

        public async void DeleteLayout(LayoutFile layoutFile)
        {
            if (layoutFile.LayoutType == LayoutFile.eLayoutType.Standard)
            {
                return;
            }

            try
            {
                File.Delete(layoutFile.LayoutPath);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                return;
            }

            FileLayouts.Remove(layoutFile);
            await RebuildMainMenu();
            await SaveProjectData();
        }

        public void CancelSave()
        {
            GridIsVisible = Visibility.Collapsed;
        }

        public void CancelDelete()
        {
            DeleteGridIsVisible = Visibility.Collapsed;
        }

        private void LoadLayoutById(string layoutId)
        {
            var layoutFile = FileLayouts.SingleOrDefault(f => f.LayoutID == layoutId);

            if (layoutFile is null)
            {
                return;
            }

            // remove all existing windows
            _dockingManager.AnchorablesSource = null;
            _dockingManager.DocumentsSource = null;
            var layoutSerializer = new XmlLayoutSerializer(_dockingManager);
            LoadLayout(layoutSerializer, layoutFile.LayoutPath);

        }


        private void LoadLayout(XmlLayoutSerializer layoutSerializer, string filePath)
        {
            // Here I've implemented the LayoutSerializationCallback just to show
            //  a way to feed layout deserialization with content loaded at runtime
            // Actually I could in this case let AvalonDock to attach the contents
            // from current layout using the content ids
            // LayoutSerializationCallback should anyway be handled to attach contents
            // not currently loaded
#pragma warning disable CA1416 // Validate platform compatibility
            layoutSerializer.LayoutSerializationCallback += (_, e) =>
            {
                if (e.Model.ContentId is not null)
                {
                    e.Content = e.Model.ContentId.ToUpper() switch
                    {
                        // Documents
                        WorkspaceLayoutNames.EnhancedView => GetPaneViewModelFromItems("EnhancedViewModel"),
                        //WorkspaceLayoutNames.EnhancedView => GetPaneViewModelFromItems<EnhancedViewModel>(),
                        // Tools
                        WorkspaceLayoutNames.BiblicalTerms => GetToolViewModelFromItems("BiblicalTermsViewModel"),
                        //case WorkspaceLayoutNames.WordMeanings:
                        //    e.Content = GetToolViewModelFromItems("WordMeaningsViewModel");
                        //    break;
                        WorkspaceLayoutNames.Marble => GetToolViewModelFromItems("MarbleViewModel"),
                        WorkspaceLayoutNames.Pins => GetToolViewModelFromItems("PinsViewModel"),
                        WorkspaceLayoutNames.TextCollection => GetToolViewModelFromItems("TextCollectionsViewModel"),
                        WorkspaceLayoutNames.Notes => GetToolViewModelFromItems("NotesViewModel"),
                        _ => e.Content
                    };
                }
            };
#pragma warning restore CA1416 // Validate platform compatibility
            try
            {
                layoutSerializer.Deserialize(filePath);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                filePath = Path.Combine(Environment.CurrentDirectory, @"Resources\Layouts\Dashboard.Layout.config");
                if (File.Exists(filePath))
                {
                    // load in the default layout
                    layoutSerializer.Deserialize(filePath);
                }
                else
                {
                    // build a layout
                    _tools.Clear();
                    _documents.Clear();

                    foreach (var t in Items)
                    {
                        var type = t;
                        switch (type)
                        {

                            case EnhancedViewModel:
                                _documents.Add((PaneViewModel)t);
                                break;


                            //case WordMeaningsViewModel:
                            case MarbleViewModel:
                            case NotesViewModel:
                            case BiblicalTermsViewModel:
                            case ParatextViews.PinsViewModel:
                            case TextCollectionsViewModel:
                                _tools.Add((ToolViewModel)t);
                                break;

                            case Project.ProjectDesignSurfaceViewModel:
                                break;
                        }
                    }

                    NotifyOfPropertyChange(() => Documents);
                }
            }

            // save to settings
            Settings.Default.LastLayout = filePath;
            _lastLayout = filePath;

            // update the existing window to set the title
            var enhancedView = _dockingManager.Layout.Descendents().OfType<LayoutDocument>()
                .FirstOrDefault(a =>
                {
                    if (a.Content is not null)
                    {
                        if (a.Content is EnhancedViewModel)
                        {
                            return true;
                        }
                    }
                    return false;
                });

            if (enhancedView is not null)
            {
                var vm = (EnhancedViewModel)enhancedView.Content;
                enhancedView.Title = vm.Title;
            }
        }

        /// <summary>
        /// return the correct existing vm from Items list - DOCUMENTS
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        private IPaneViewModel GetPaneViewModelFromItems(string vm)
        {
            foreach (var item in Items)
            {
                var type = item;
                if (type.GetType().Name == vm)
                {
                    switch (type)
                    {
                        case EnhancedViewModel:
                            return (IPaneViewModel)item;
                    }
                }
            }

            return (IPaneViewModel)Items[0];
        }

        /// <summary>
        /// return the correct existing vm from Items list - TOOLS
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        private ToolViewModel GetToolViewModelFromItems(string vm)
        {
            foreach (var t in Items)
            {
                var type = t;
                if (type.GetType().Name == vm)
                {
                    switch (type)
                    {
                        //case WordMeaningsViewModel:
                        case BiblicalTermsViewModel:
                        case ParatextViews.PinsViewModel:
                        case TextCollectionsViewModel:
                        case MarbleViewModel:
                        case NotesViewModel:
                            return (ToolViewModel)t;
                    }
                }
            }

            return (ToolViewModel)Items[0];
        }

        private (object vm, string title, DockSide dockSide) LoadWindowViewModel(string windowTag)
        {
            // window has been closed so we need to reopen it
            switch (windowTag)
            {
                // DOCUMENTS
                case WorkspaceLayoutNames.EnhancedView:
                    var enhancedViewModel = GetPaneViewModelFromItems("EnhancedViewModel");
                    //var enhancedViewModel = GetPaneViewModelFromItems<EnhancedViewModel>();
                    return (enhancedViewModel, enhancedViewModel.Title, enhancedViewModel.DockSide);
                // TOOLS
                case WorkspaceLayoutNames.BiblicalTerms:
                    var biblicalTermsViewModel = GetToolViewModelFromItems("BiblicalTermsViewModel");
                    return (biblicalTermsViewModel, biblicalTermsViewModel.Title, biblicalTermsViewModel.DockSide);
                case WorkspaceLayoutNames.Pins:
                    var pinsViewModel = GetToolViewModelFromItems("PinsViewModel");
                    return (pinsViewModel, pinsViewModel.Title, pinsViewModel.DockSide);
                case WorkspaceLayoutNames.TextCollection:
                    var textCollectionsViewModel = GetToolViewModelFromItems("TextCollectionsViewModel");
                    return (textCollectionsViewModel, textCollectionsViewModel.Title, textCollectionsViewModel.DockSide);
                //case WorkspaceLayoutNames.WordMeanings:
                //    var wordMeaningsViewModel = GetToolViewModelFromItems("WordMeaningsViewModel");
                //    return (wordMeaningsViewModel, wordMeaningsViewModel.Title, wordMeaningsViewModel.DockSide);
                case WorkspaceLayoutNames.Marble:
                    var marbleViewModel = GetToolViewModelFromItems("MarbleViewModel");
                    return (marbleViewModel, marbleViewModel.Title, marbleViewModel.DockSide);
                case WorkspaceLayoutNames.Notes:
                    var notesViewModel = GetToolViewModelFromItems("NotesViewModel");
                    return (notesViewModel, notesViewModel.Title, notesViewModel.DockSide);
            }
            return (null, null, DockSide.Bottom);
        }

        /// <summary>
        /// Un hide window
        /// </summary>
        /// <param name="windowTag"></param>
        private void UnhideWindow(string windowTag)
        {
            WindowIdToLoad = windowTag;
            // test for tool window
            var windowPane = _dockingManager.Layout.Descendents()
                .OfType<LayoutAnchorable>()
                .SingleOrDefault(a =>
                {
                    if (a.ContentId is not null)
                    {
                        //Debug.WriteLine(a.ContentId);
                        return a.ContentId.ToUpper() == windowTag.ToUpper();
                    }
                    return false;
                });

            if (windowPane != null)
            {
                if (windowPane.IsAutoHidden)
                {
                    windowPane.ToggleAutoHide();
                }
                else if (windowPane.IsHidden)
                {
                    windowPane.Show();
                }
                else if (windowPane.IsVisible)
                {
                    windowPane.IsActive = true;
                }
            }
            else
            {
                // test for document window
                var windowDockable = _dockingManager.Layout.Descendents()
                    .OfType<LayoutDocument>()
                    .SingleOrDefault(a =>
                    {
                        if (a.ContentId is not null)
                        {
                            //Debug.WriteLine(a.ContentId);
                            return a.ContentId.ToUpper() == windowTag.ToUpper();
                        }
                        return false;
                    });

                if (windowDockable == null)
                {
                    switch (windowTag.ToUpper())
                    {
                        // Documents
                        case WorkspaceLayoutNames.EnhancedView:

                            // setup the right ViewModel for the pane
                            var tuple = LoadWindowViewModel(windowTag);
                            windowDockable = new LayoutDocument
                            {
                                ContentId = windowTag,
                                Content = tuple.vm,
                                Title = tuple.title,
                                IsActive = true
                            };

                            AddNewEnhancedViewTab(windowDockable);
                            break;

                        // Tools
                        case WorkspaceLayoutNames.BiblicalTerms:
                        //case WorkspaceLayoutNames.WordMeanings:
                        case WorkspaceLayoutNames.Marble:
                        case WorkspaceLayoutNames.Pins:
                        case WorkspaceLayoutNames.TextCollection:
                        case WorkspaceLayoutNames.Notes:
                            {

                                // setup the right ViewModel for the pane
                                tuple = LoadWindowViewModel(windowTag);

                                // window has been closed so reload it
                                windowPane = new LayoutAnchorable
                                {
                                    ContentId = windowTag,
                                    Content = tuple.vm,
                                    Title = tuple.title,
                                    IsActive = true
                                };


                                // set where it will doc on layout
                                if (tuple.dockSide == DockSide.Bottom)
                                {
                                    windowPane.AddToLayout(_dockingManager, AnchorableShowStrategy.Bottom);
                                }
                                else if (tuple.dockSide == DockSide.Left)
                                {
                                    windowPane.AddToLayout(_dockingManager, AnchorableShowStrategy.Left);
                                }

                                break;
                            }
                    }
                }
                else
                {
                    windowDockable.IsActive = true;
                }

            }
#pragma warning restore CA1416 // Validate platform compatibility
        }

        private async Task<bool> TryUpdateExistingEnhancedView(EnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken)
        {

            var dockableWindows = _dockingManager.Layout.Descendents()
                .OfType<LayoutDocument>().ToList();

            // There are no EnhancedViews currently being displayed, so
            // return "false".
            if (dockableWindows.Count == 0)
            {
                return false;
            }


            // the user wants to add to the currently active window
            if (metadatum.IsNewWindow == false)
            {

                if (dockableWindows.Count == 1)
                {
                    await EnhancedViewModels.First().AddItem(metadatum, cancellationToken);
                    return true;
                }

                // more than one enhanced corpus window is open and active
                foreach (var document in dockableWindows)
                {
                    if (document.IsActive && document.Content is EnhancedViewModel enhancedViewModel)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        var guid = enhancedViewModel.PaneId;
                        if (EnhancedViewModels.Any(item => item.PaneId == guid))
                        {
                            await enhancedViewModel.AddItem(metadatum, cancellationToken);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private async Task DeactivateDockedWindows()
        {
            // deactivate any other doc windows before we add in the new one
            var docWindows = _dockingManager.Layout.Descendents().OfType<LayoutDocument>();
            foreach (var document in docWindows)
            {
                document.IsActive = false;
            }

            await Task.CompletedTask;
        }

        public async Task AddMetadatumEnhancedView(EnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken = default)
        {
            if (!await TryUpdateExistingEnhancedView(metadatum, cancellationToken))
            {
                await DeactivateDockedWindows();
                var viewModel = await ActivateItemAsync<EnhancedViewModel>(cancellationToken);
                await viewModel.Initialize(new EnhancedViewLayout
                {
                    ParatextSync = false,
                    Title = $"{metadatum.DisplayName}",
                    VerseOffset = 0
                }, metadatum, cancellationToken);

                AddNewEnhancedViewTab(metadatum.CreateLayoutDocument(viewModel));
            }
            await SaveAvalonDockLayout();
        }

        public async Task ExecuteMenuCommand(MenuItemViewModel menuItem)
        {
            switch (menuItem.Id)
            {
                case MenuIds.Settings:
                    {
                        var viewmodel = IoC.Get<DashboardSettingsViewModel>();
                        await this.WindowManager.ShowWindowAsync(viewmodel, null, null);
                        break;
                    }

                case MenuIds.CollaborationManageProjects:
                    {
                        await CollabProjectManager();
                        break;
                    }

                case MenuIds.CollaborationInitialize:
                    {
                        await ShowCollaborationInitialize();
                        break;
                    }

                case MenuIds.CollaborationGetLatest:
                    {
                        await ShowCollaborationGetLatest();
                        break;
                    }

                case MenuIds.CollaborationCommit:
                    {
                        await ShowCollaborationCommit();
                        break;
                    }

                case MenuIds.CollaborationFetchMerge:
                    {
                        if (_collaborationManager.IsRepositoryInitialized() && InternetAvailability.IsInternetAvailable())
                        {
                            _collaborationManager.FetchMergeRemote();
                            await RebuildMainMenu();
                        }
                        break;
                    }

                case MenuIds.CollaborationHardReset:
                    {
                        if (_collaborationManager.IsRepositoryInitialized() && _collaborationManager.IsCurrentProjectInRepository())
                        {
                            _collaborationManager.HardResetChanges();
                            await RebuildMainMenu();
                        }
                        break;
                    }

                case MenuIds.CollaborationCreateBackup:
                    {
                        if (_collaborationManager.IsRepositoryInitialized() && _collaborationManager.IsCurrentProjectInRepository())
                        {
                            await _collaborationManager.CreateProjectBackupAsync(CancellationToken.None);
                            await RebuildMainMenu();
                        }
                        break;
                    }

                case MenuIds.CollaborationDumpDifferencesLastMergedHead:
                    {
                        if (_collaborationManager.IsRepositoryInitialized() && _collaborationManager.IsCurrentProjectInRepository())
                        {
                            _collaborationManager.DumpDifferencesBetweenLastMergedCommitAndHead();
                            await RebuildMainMenu();
                        }
                        break;
                    }

                case MenuIds.CollaborationDumpDifferencesHeadCurrentDb:
                    {
                        if (_collaborationManager.IsRepositoryInitialized() && _collaborationManager.IsCurrentProjectInRepository())
                        {
                            await _collaborationManager.DumpDifferencesBetweenHeadAndCurrentDatabaseAsync();
                            await RebuildMainMenu();
                        }
                        break;
                    }

                case MenuIds.About:
                    {
                        ShowAboutWindow();
                        break;
                    }

                case MenuIds.GettingStartedGuide:
                    {
                        LaunchGettingStartedGuide();
                        break;
                    }

                case MenuIds.GettingStartedVideos:
                    {
                        LaunchGettingStartedVideos();
                        break;
                    }

                case MenuIds.ShowLog:
                    {
                        ShowLogs();
                        break;
                    }

                case MenuIds.GatherLogs:
                    {
                        GatherLogs();
                        break;
                    }

                default:
                    {
                        UnhideWindow(menuItem.Id);
                        break;
                    }
            }

            if (!_longRunningTaskManager!.HasTasks())
            {
                switch (menuItem.Id)
                {
                    case { } a when a.StartsWith(MenuIds.ProjectLayout):
                        {
                            LoadLayoutById(menuItem.Id);
                            break;
                        }

                    case { } b when b.StartsWith(MenuIds.StandardLayout):
                        {
                            LoadLayoutById(menuItem.Id);
                            break;
                        }

                    case MenuIds.Separator:
                        {
                            // no-op
                            break;
                        }

                    case MenuIds.Save:
                        {
                            GridIsVisible = Visibility.Visible;
                            DeleteGridIsVisible = Visibility.Collapsed;
                            break;
                        }

                    case MenuIds.Delete:
                        {
                            DeleteGridIsVisible = Visibility.Visible;
                            GridIsVisible = Visibility.Collapsed;
                            break;
                        }

                    case MenuIds.NewEnhancedCorpus:
                        {
                            await AddNewEnhancedView();
                            break;
                        }
                    case MenuIds.BiblicalTerms:
                        {
                            UnhideWindow(WindowIds.BiblicalTerms);
                            break;
                        }

                    case MenuIds.EnhancedCorpus:
                        {
                            UnhideWindow(WindowIds.EnhancedView);
                            break;
                        }

                    case MenuIds.Pins:
                        {
                            UnhideWindow(WindowIds.Pins);
                            break;
                        }

                    //case MenuIds.WordMeanings":
                    //{
                    //     UnhideWindow(WindowIds.WordMeanings);
                    //    break;
                    //}

                    case MenuIds.Marble:
                        {
                            UnhideWindow(WindowIds.Marble);
                            break;
                        }

                    case MenuIds.TextCollection:
                        {
                            UnhideWindow(WindowIds.TextCollection);
                            break;
                        }

                    case MenuIds.Notes:
                        {
                            UnhideWindow(WindowIds.Notes);
                            break;
                        }

                    case MenuIds.FileNew:
                        {
                            await ShowStartupDialog(menuItem);
                            break;
                        }
                    case MenuIds.FileOpen:
                        {
                            await ShowStartupDialog(menuItem);
                            break;
                        }

                    case MenuIds.ReloadProject:
                        {
                            await EventAggregator.PublishOnUIThreadAsync(new ReloadProjectMessage());
                            break;

                        }
                    case MenuIds.Layout:
                        {
                            //no-op
                            break;
                        }
                    default:
                        {
                            UnhideWindow(menuItem.Id);
                            break;
                        }
                }
            }
        }

        private async Task ShowStartupDialog(MenuItemViewModel menuItem)
        {
            if (menuItem.Id == MenuIds.FileNew)
            {
                StartupDialogViewModel.GoToSetup = true;
            }

            var startupDialogViewModel = LifetimeScope!.Resolve<StartupDialogViewModel>();
            startupDialogViewModel.MimicParatextConnection = true;

            var result = await WindowManager!.ShowDialogAsync(startupDialogViewModel);

            if (result == true)
            {
                await OnDeactivateAsync(false, CancellationToken.None);
                NavigationService?.NavigateToViewModel<MainViewModel>(startupDialogViewModel.ExtraData);
                await OnInitializeAsync(CancellationToken.None);
                await OnActivateAsync(CancellationToken.None);
                await EventAggregator.PublishOnUIThreadAsync(new ProjectLoadCompleteMessage(true));
            }
        }

        #endregion // Methods

        #region IHandle implementations

        public async Task HandleAsync(ProgressBarVisibilityMessage message, CancellationToken cancellationToken)
        {
            OnUIThread(() => ShowProgressBar = message.Show);
            await Task.CompletedTask;
        }

        public async Task HandleAsync(ProgressBarMessage message, CancellationToken cancellationToken)
        {
            OnUIThread(() => Message = message.Message);
            await Task.CompletedTask;
        }

        public async Task HandleAsync(UiLanguageChangedMessage message, CancellationToken cancellationToken)
        {
            // rebuild the menu system with the new language
            await RebuildMainMenu();
        }


        public Task HandleAsync(ActiveDocumentMessage message, CancellationToken cancellationToken)
        {
            var guid = message.Guid;

            var dockableWindows = _dockingManager.Layout.Descendents()
                .OfType<LayoutDocument>();
            foreach (var pane in dockableWindows)
            {
                var content = pane.Content as EnhancedViewModel;
                // ReSharper disable once PossibleNullReferenceException
                if (content.PaneId != guid)
                {
                    pane.IsActive = false;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Ensure that there is at least one document tab open at all times
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(CloseDockingPane message, CancellationToken cancellationToken)
        {
            var windowGuid = message.Guid;

            var dockableWindows = _dockingManager.Layout.Descendents()
                .OfType<LayoutDocument>().ToArray();

            if (dockableWindows.Length > 1)
            {
                foreach (var pane in dockableWindows)
                {
                    var content = pane.Content as EnhancedViewModel;
                    // ReSharper disable once PossibleNullReferenceException
                    if (content.PaneId == windowGuid)
                    {
                        var closingEnhancedViewPopupViewModel = LifetimeScope!.Resolve<ClosingEnhancedViewPopupViewModel>();

                        var result = await WindowManager!.ShowDialogAsync(closingEnhancedViewPopupViewModel, null,
                            SimpleMessagePopupViewModel.CreateDialogSettings(closingEnhancedViewPopupViewModel.Title));

                        if (result == true)
                        {
                            pane.Close();
                        }
                        break;
                    }
                }
            }
        }

        // capture the window settings for if we do a screenshot
        public Task HandleAsync(ApplicationWindowSettings message, CancellationToken cancellationToken)
        {
            _windowSettings = message.WindowSettings;
            return Task.CompletedTask;
        }

        public Task HandleAsync(FilterPinsMessage message, CancellationToken cancellationToken)
        {
            UnhideWindow("PINS");
            return Task.CompletedTask;
        }

        public Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            bool enable;
            if (_longRunningTaskManager.Tasks.Count <= 1 && message.Status.TaskLongRunningProcessStatus != LongRunningTaskStatus.Running)
            {
                enable = true;
            }
            else
            {
                enable = false;
            }
            foreach (var item in MenuItems)
            {
                if (item.Id == MenuIds.File)
                {
                    item.IsEnabled = enable;
                }
                if (item.Id == MenuIds.Layout)
                {
                    item.IsEnabled = enable;
                }
                if (item.Id == MenuIds.Window)
                {
                    item.IsEnabled = enable;
                }
            }
            return Task.CompletedTask;
        }

        public async Task HandleAsync(ReloadProjectMessage message, CancellationToken cancellationToken)
        {
            //await Initialize(cancellationToken);

            await OnDeactivateAsync(false, CancellationToken.None);
            //NavigationService?.NavigateToViewModel<MainViewModel>(startupDialogViewModel.ExtraData);
            await OnInitializeAsync(CancellationToken.None);
            await OnActivateAsync(CancellationToken.None);
            //await EventAggregator.PublishOnUIThreadAsync(new ProjectLoadCompleteMessage(true));
        }


        #endregion
    }
}