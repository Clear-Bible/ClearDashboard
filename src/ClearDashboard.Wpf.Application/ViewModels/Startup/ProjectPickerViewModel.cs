using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Features;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.Collaboration.Util;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Features.DashboardProjects;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Collaboration;
using ClearDashboard.Wpf.Application.ViewModels.DashboardSettings;
using ClearDashboard.Wpf.Application.ViewModels.Popups;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static ClearDashboard.DataAccessLayer.Features.DashboardProjects.GetProjectIdSlice;
using static ClearDashboard.DataAccessLayer.Features.DashboardProjects.GetProjectVersionSlice;
using static ClearDashboard.Wpf.Application.Helpers.Telemetry;
using Resources = ClearDashboard.Wpf.Application.Strings.Resources;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class ProjectPickerViewModel : DashboardApplicationWorkflowStepViewModel<StartupDialogViewModel>,
        IHandle<ParatextConnectedMessage>, IHandle<UserMessage>, IHandle<ReloadProjectPickerProjects>, IHandle<DeletedGitProject>
    {
        #region Member Variables
        private System.Timers.Timer _timer;
        private long _elapsedSeconds;
        private double _timerInterval = 15000;

        private readonly ParatextProxy _paratextProxy;
        private readonly IMediator _mediator;
        private readonly GitLabHttpClientServices _gitLabHttpClientServices;
        private readonly GitLabClient _gitLabClient;
        private readonly ILocalizationService _localizationService;
        private readonly TranslationSource? _translationSource;
        private readonly IWindowManager _windowManager;
        private readonly CollaborationManager _collaborationManager;
        private readonly CollaborationServerHttpClientServices _collaborationHttpClientServices;
        private readonly LongRunningTaskManager _longRunningTaskManager;
        private bool _initializationComplete = false;
        private readonly string _importTaskName = "Import Collab Project";

        private string _projectDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects");
        #endregion

        #region Observable Objects

        private CollaborationConfiguration _collaborationConfig = new();
        public CollaborationConfiguration CollaborationConfig
        {
            get => _collaborationConfig;
            set
            {
                _collaborationConfig = value;
                NotifyOfPropertyChange(() => CollaborationConfig);
            }
        }


        private List<DashboardProject> _dashboardProjects = new();
        public List<DashboardProject> DashboardProjects
        {
            get => _dashboardProjects;
            set
            {
                _dashboardProjects = value;
                NotifyOfPropertyChange(() => DashboardProjects);
            }
        }

        private BindableCollection<DashboardCollabProject> _dashboardCollabProjects = new();
        public BindableCollection<DashboardCollabProject> DashboardCollabProjects
        {
            get => _dashboardCollabProjects;
            set
            {
                _dashboardCollabProjects = value;
                NotifyOfPropertyChange(() => DashboardCollabProjects);
            }
        }

        private bool _createCollabUserVisibility;
        public bool CreateCollabUserVisibility
        {
            get => _createCollabUserVisibility;
            set
            {
                _createCollabUserVisibility = value;
                ManageCollabVisibility = !value;
                NotifyOfPropertyChange(() => CreateCollabUserVisibility);
            }
        }


        private bool _manageCollabVisibility;
        public bool ManageCollabVisibility
        {
            get => _manageCollabVisibility;
            set
            {
                _manageCollabVisibility = value;
                NotifyOfPropertyChange(() => ManageCollabVisibility);
            }
        }

        private bool _useProjectTemplate;
        public bool UseProjectTemplate
        {
            get => _useProjectTemplate;
            set => Set(ref _useProjectTemplate, value);
        }

        private Visibility _showCollabUserInfo = Visibility.Visible;
        public Visibility ShowCollabUserInfo
        {
            get => _showCollabUserInfo;
            set
            {
                _showCollabUserInfo = value;
                NotifyOfPropertyChange(() => ShowCollabUserInfo);
            }
        }



        private Visibility _initializeCollaborationVisibility;
        public Visibility InitializeCollaborationVisibility
        {
            get => _initializeCollaborationVisibility;
            set
            {
                _initializeCollaborationVisibility = value;
                NotifyOfPropertyChange(() => InitializeCollaborationVisibility);
            }
        }

        private string? _initializeCollaborationLabel;
        public string? InitializeCollaborationLabel
        {
            get => _initializeCollaborationLabel;

            set
            {
                _initializeCollaborationLabel = value;
                NotifyOfPropertyChange(() => InitializeCollaborationLabel);
            }
        }

        private Visibility _collabProjectVisibility;
        public Visibility CollabProjectVisibility
        {
            get => _collabProjectVisibility;
            set
            {
                _collabProjectVisibility = value;
                NotifyOfPropertyChange(() => CollabProjectVisibility);
            }
        }

        private bool _collabButtonsEnabled;
        public bool CollabButtonsEnabled
        {
            get => _collabButtonsEnabled;
            set
            {
                _collabButtonsEnabled = value;
                NotifyOfPropertyChange(() => CollabButtonsEnabled);
            }
        }

        private Visibility _searchBlankVisibility;
        public Visibility SearchBlankVisibility
        {
            get => _searchBlankVisibility;
            set
            {
                _searchBlankVisibility = value;
                NotifyOfPropertyChange(() => SearchBlankVisibility);
            }
        }

        private Visibility _noProjectVisibility;
        public Visibility NoProjectVisibility
        {
            get => _noProjectVisibility;
            set
            {
                _noProjectVisibility = value;
                NotifyOfPropertyChange(() => NoProjectVisibility);
            }
        }

        private Visibility _alertVisibility = Visibility.Visible;
        public Visibility AlertVisibility
        {
            get => _alertVisibility;
            set
            {
                _alertVisibility = value;
                NotifyOfPropertyChange(() => AlertVisibility);
            }
        }

        private Visibility _deleteErrorVisibility = Visibility.Collapsed;
        public Visibility DeleteErrorVisibility
        {
            get => _deleteErrorVisibility;
            set
            {
                _deleteErrorVisibility = value;
                NotifyOfPropertyChange(() => DeleteErrorVisibility);
            }
        }

        private Visibility _alreadyOpenMessageVisibility = Visibility.Collapsed;
        public Visibility AlreadyOpenMessageVisibility
        {
            get => _alreadyOpenMessageVisibility;
            set
            {
                _alreadyOpenMessageVisibility = value;
                NotifyOfPropertyChange(() => AlreadyOpenMessageVisibility);
            }
        }

        private string? _message = Resources.ResourceManager.GetString("language", Thread.CurrentThread.CurrentUICulture);
        public string? Message
        {
            get => _message;
            set
            {
                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }

        private LanguageTypeValue _selectedLanguage;
        public LanguageTypeValue SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;

                var language = EnumHelper.GetDescription(_selectedLanguage);
                SaveUserLanguage(_selectedLanguage.ToString());
                _translationSource.Language = language;

                Message = Resources.ResourceManager.GetString("language", Thread.CurrentThread.CurrentUICulture);

                NotifyOfPropertyChange(() => SelectedLanguage);

                SendUiLanguageChangeMessage(language);
            }
        }

        private bool _isParatextInstalled;
        public bool IsParatextInstalled
        {
            get => _isParatextInstalled;
            set => Set(ref _isParatextInstalled, value);
        }

        private bool _isParatextRunning;
        public bool IsParatextRunning
        {
            get => _isParatextRunning;
            set => Set(ref _isParatextRunning, value);
        }

        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set
            {
                value ??= string.Empty;

                _searchText = value;
                NotifyOfPropertyChange(() => SearchText);


                DashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, SearchText);
                if (SearchText == string.Empty || SearchText is null)
                {
                    SearchBlankVisibility = Visibility.Collapsed;
                    NoProjectVisibility = Visibility.Visible;
                }

                if (DashboardProjectsDisplay.Count <= 0 && DashboardProjects.Count > 0)
                {
                    NoProjectVisibility = Visibility.Hidden;
                    SearchBlankVisibility = Visibility.Visible;
                }
                else
                {
                    SearchBlankVisibility = Visibility.Collapsed;
                    NoProjectVisibility = Visibility.Visible;
                }

                if (SearchText != string.Empty && DashboardProjects.Count <= 0)
                {
                    NoProjectVisibility = Visibility.Hidden;
                    SearchBlankVisibility = Visibility.Visible;
                }
            }
        }

        private BindableCollection<DashboardProject> _dashboardProjectsDisplay = new();
        public BindableCollection<DashboardProject> DashboardProjectsDisplay
        {
            get => _dashboardProjectsDisplay;
            set
            {
                _dashboardProjectsDisplay = value;
                NotifyOfPropertyChange(() => DashboardProjectsDisplay);
            }
        }

        private BindableCollection<DashboardCollabProject> _dashboardCollabProjectsDisplay = new();

        public BindableCollection<DashboardCollabProject> DashboardCollabProjectsDisplay
        {
            get => _dashboardCollabProjectsDisplay;
            set
            {
                _dashboardCollabProjectsDisplay = value;
                NotifyOfPropertyChange(() => DashboardCollabProjectsDisplay);
            }
        }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                NotifyOfPropertyChange(() => Connected);
            }
        }

        private string? _paratextUserName;
        public string? ParatextUserName
        {
            get => _paratextUserName;

            set
            {
                _paratextUserName = value;
                NotifyOfPropertyChange(() => ParatextUserName);
            }
        }


        private Visibility _isParatextRegularInstalled = Visibility.Visible;
        public Visibility IsParatextRegularInstalled
        {
            get => _isParatextRegularInstalled;
            set 
            { 
                _isParatextRegularInstalled = value;
                NotifyOfPropertyChange(() => IsParatextRegularInstalled);
            }
        }


        private Visibility _isParatextBetaInstalled = Visibility.Collapsed;
        public Visibility IsParatextBetaInstalled
        {
            get => _isParatextBetaInstalled;
            set
            {
                _isParatextBetaInstalled = value;
                NotifyOfPropertyChange(() => IsParatextBetaInstalled);
            }
        }

        private Visibility _projectLoadingProgressBarVisibility = Visibility.Collapsed;
        public Visibility ProjectLoadingProgressBarVisibility
        {
            get => _projectLoadingProgressBarVisibility;
            set
            {
                _projectLoadingProgressBarVisibility = value;
                NotifyOfPropertyChange(() => ProjectLoadingProgressBarVisibility);
            }
        }

        #endregion

        #region Constructor

        public ProjectPickerViewModel()
        {
            // no-op
        }

        public ProjectPickerViewModel(TranslationSource translationSource,
            DashboardProjectManager projectManager,
            ParatextProxy paratextProxy,
            INavigationService navigationService,
            ILogger<ProjectPickerViewModel> logger,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope,
            ILocalizationService localizationService,
            IWindowManager windowManager,
            GitLabHttpClientServices gitLabHttpClientServices,
            GitLabClient gitLabClient,
            CollaborationManager collaborationManager,
            CollaborationServerHttpClientServices collaborationHttpClientServices,
        LongRunningTaskManager longRunningTaskManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            Logger?.LogInformation("Project Picker constructor called.");
            //_windowManager = windowManager;
            _paratextProxy = paratextProxy;
            _mediator = mediator;
            _gitLabHttpClientServices = gitLabHttpClientServices;
            _gitLabClient = gitLabClient;
            _localizationService = localizationService;
            AlertVisibility = Visibility.Collapsed;
            _translationSource = translationSource;
            NoProjectVisibility = Visibility.Visible;
            SearchBlankVisibility = Visibility.Collapsed;

            _windowManager = windowManager;
            _collaborationManager = collaborationManager;
            SetCollabVisibility();

            IsParatextRunning = _paratextProxy.IsParatextRunning();
            _collaborationHttpClientServices=collaborationHttpClientServices;
            _longRunningTaskManager = longRunningTaskManager;

            _paratextProxy.IsParatextInstalled();
            if (_paratextProxy.ParatextInstallPath != string.Empty)
            {
                IsParatextRegularInstalled = Visibility.Visible;
            }
            else
            {
                IsParatextRegularInstalled = Visibility.Collapsed;
            }

            if (_paratextProxy.ParatextBetaInstallPath != string.Empty)
            {
                IsParatextBetaInstalled = Visibility.Visible;
            }
            else
            {
                IsParatextBetaInstalled = Visibility.Collapsed;
            }

            //
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _longRunningTaskManager.CancellationTokenSource= new CancellationTokenSource();
            await base.OnActivateAsync(cancellationToken);

            if (_initializationComplete)
            {
                return;
            }

            await GetProjectsVersion();
        }

        protected override async void OnViewLoaded(object view)
        {
            EventAggregator.Subscribe(this);

            IsParatextInstalled = _paratextProxy.IsParatextInstalled();
            if (!IsParatextInstalled)
            {
                // await this.ShowMessageAsync("This is the title", "Some message");
            }

            IsParatextRunning = _paratextProxy.IsParatextRunning();
            if (IsParatextRunning)
            {
                if (Connected)
                {
                    ParatextUserName = ProjectManager.CurrentUser.ParatextUserName ?? ProjectManager.CurrentUser.FullName;
                }
            }
            else
            {
                ListenForParatextStart();
            }


            if (Pinger.PingHost())
            {
                await FinishAccountSetup();

                await CheckTokenExpiration();

                await GetCollabProjects();
            }

            await GetRemoteUser();

            await LicenseManager.DeleteOldLicense();

            base.OnViewLoaded(view);

            _initializationComplete = true;


            StartTimer();
        }



        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (_timer is not null)
            {
                try
                {
                    _timer.Stop();
                    _timer.Dispose();
                }
                catch (Exception e)
                {
                    Logger?.LogError(e, "Unable to stop timer");
                }
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion Constructor

        #region Methods

        private void StartTimer()
        {
            _timer = new System.Timers.Timer(15000);
            _timer.Elapsed += OnTimedEvent;
            _timer.Enabled = true;
            _timer.AutoReset = true;
        }

        private async void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await SetGitLabUpdateNeeded(DashboardProjects);

                _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, SearchText);
                NotifyOfPropertyChange(() => DashboardProjectsDisplay);
            });
        }

        public async Task OpenSettings()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 500;
            settings.MinHeight = 500;

            // Keep the window on top
            //settings.Topmost = true;
            settings.Owner = System.Windows.Application.Current.MainWindow;

            // get this applications position on the screen
            var window = App.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this.ParentViewModel);
            settings.Left = window.Left + 150;
            settings.Top = window.Top + 100;

            var viewmodel = IoC.Get<DashboardSettingsViewModel>();
            IWindowManager manager = new WindowManager();
            await manager.ShowWindowAsync(viewmodel, null, settings);
        }

        public void ShowAccountInfoWindow()
        {
            AccountWindow.ShowAccountInfoWindow(_localizationService, new NoExitWindowManager());
        }

        public async Task StartParatext()
        {
            if (!_paratextProxy.IsParatextRunning())
            {
                await _paratextProxy.StartParatextAsync();
            }

            IsParatextRunning = _paratextProxy.IsParatextRunning();
            IsParatextInstalled = _paratextProxy.IsParatextInstalled();
        }

        public async Task StartParatextBeta()
        {
            if (!_paratextProxy.IsParatextRunning())
            {
                await _paratextProxy.StartParatextBetaAsync();
            }

            IsParatextRunning = _paratextProxy.IsParatextRunning();
            IsParatextInstalled = _paratextProxy.IsParatextInstalled();
        }
        


        public async Task RefreshCollabProjectList()
        {
            await GetCollabProjects();
        }

        public async Task InitializeCollaborationUser()
        {
            //var localizedString = _localizationService!["MainView_About"];

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.CanResize;
            settings.MinWidth = 500;
            settings.MinHeight = 500;
            //settings.Title = $"{localizedString}";

            // Keep the window on top
            //settings.Topmost = true;
            settings.Owner = System.Windows.Application.Current.MainWindow;

            var viewModel = IoC.Get<NewCollabUserViewModel>();

            IWindowManager manager = new WindowManager();
            await manager.ShowDialogAsync(viewModel, null, settings);

            await GetRemoteUser();
            await GetProjectsVersion();
            await GetCollabProjects();
        }

        public async Task CollabProjectManager()
        {
            var localizedString = _localizationService!["CollabProjectManagementView_CollaborationManagement"];

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 500;
            settings.MinHeight = 500;
            settings.Title = $"{localizedString}";


            // Keep the window on top
            //settings.Topmost = true;
            settings.Owner = System.Windows.Application.Current.MainWindow;

            var viewModel = IoC.Get<CollabProjectManagementViewModel>();


            IWindowManager manager = new WindowManager();
            await manager.ShowDialogAsync(viewModel, null, settings);
        }

        public async Task InitializeCollaboration()
        {
            // Only respond to a Left button click otherwise,
            // the context menu will not be shown on a right click.
            if (CheckIfConnectedToParatext() == false)
            {
                return;
            }

            CollabButtonsEnabled = false;
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            if (!_collaborationManager.IsRepositoryInitialized())
            {
                _collaborationManager.InitializeRepository();
            }

            try
            {
                _collaborationManager.FetchMergeRemote();
            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, "Unable to fetch from server");
            }

            await GetCollabProjects();
            SetCollabVisibility();
        }

        private async Task FinishAccountSetup()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            Logger!.LogDebug("Entering FinishAccountSetup");

            var licenseUser = LicenseManager.GetUserFromLicense();

            var dashboardUser = await _collaborationHttpClientServices.GetDashboardUserExistsById(licenseUser.Id);

            if (dashboardUser is null)
            {
                Logger!.LogDebug("FinishAccountSetup: dashboard user is null");
            }
            else
            {
                Logger!.LogDebug($"FinishAccountSetup: dashboard user is: {dashboardUser.FullName} {dashboardUser.Id}");
                Logger!.LogDebug($"FinishAccountSetup: dashboard GitLabUserId: {dashboardUser.GitLabUserId} ParatextUserName: {dashboardUser.ParatextUserName}");
            }

            CollaborationUser collaborationUser = null;
            bool dashboardUserChanged = false;
            bool collaborationUserSet = false;
            if (_collaborationManager.HasRemoteConfigured())
            {

                CollaborationConfig = _collaborationManager.GetConfig();
                collaborationUser = await _collaborationHttpClientServices.GetCollabUserExistsById(CollaborationConfig.UserId);

                // make a new collab user if one doesn't exist
                if (collaborationUser.UserId < 1)
                {
                    Logger!.LogDebug("FinishAccountSetup:  create new GitLabUser");
                    var user = new GitLabUser
                    {
                        Id = CollaborationConfig.UserId,
                        UserName = CollaborationConfig.RemoteUserName!,
                        Email = CollaborationConfig.RemoteEmail!,
                        Password = CollaborationConfig.RemotePersonalPassword!,
                        Organization = CollaborationConfig.Group!,
                        NamespaceId = CollaborationConfig.NamespaceId
                    };

                    var result = await _collaborationHttpClientServices.CreateNewCollabUser(user, user.Password);
                    Logger!.LogDebug($"FinishAccountSetup: create new GitLabUser result: {result}");
                }

                //make a DashboardUser
                if (dashboardUser.Id == Guid.Empty)
                {
                    Logger!.LogDebug("FinishAccountSetup:  create new dashboard user");

                    var newDashboardUser = new DashboardUser(
                        licenseUser,
                        collaborationUser.RemoteEmail,
                        LicenseManager.GetLicenseFromFile(LicenseManager.LicenseFilePath),
                        collaborationUser.GroupName,
                        collaborationUser.UserId,
                        Assembly.GetEntryAssembly()?.GetName().Version?.ToString(),
                        DateTime.Today.Date,
                        ProjectManager.CurrentUser.ParatextUserName);

                    dashboardUserChanged = await _collaborationHttpClientServices.CreateNewDashboardUser(newDashboardUser);
                    Logger!.LogDebug($"FinishAccountSetup: create new dashboard user result: {dashboardUserChanged}");
                }
                else
                {
                    //update a DashboardUser with new paratext username
                    if (string.IsNullOrEmpty(dashboardUser.ParatextUserName) && ParatextUserName is not null)
                    {
                        dashboardUser.ParatextUserName = ParatextUserName;
                    }
                    dashboardUser.Organization = collaborationUser.GroupName;
                    dashboardUser.GitLabUserId = collaborationUser.UserId;
                    dashboardUser.AppVersionNumber = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
                    dashboardUser.AppLastDate = DateTime.Today.Date;

                    var response = await _collaborationHttpClientServices.UpdateDashboardUser(dashboardUser);
                    Logger!.LogDebug($"FinishAccountSetup: UpdateDashboardUser: {response}");
                }
            }

            // if a new user has been created, get the new user
            if (dashboardUserChanged)
            {
                Logger!.LogDebug("FinishAccountSetup: dashboardUserChanged");
                dashboardUser = await _collaborationHttpClientServices.GetDashboardUserExistsById(licenseUser.Id);
                Logger!.LogDebug($"FinishAccountSetup: dashboardUserChanged result: {dashboardUser.Id} {dashboardUser.FullName}");
            }

            if (collaborationUser is null)
            {
                Logger!.LogDebug("FinishAccountSetup: collaborationUser is null");
                collaborationUser = await _collaborationHttpClientServices.GetCollabUserExistsById(dashboardUser.GitLabUserId);
                Logger!.LogDebug($"FinishAccountSetup:  collaborationUser is null result: {collaborationUser.UserId} {collaborationUser.RemoteUserName}");                
            }


            if (collaborationUser.UserId < 1)
            {
                Logger!.LogDebug($"FinishAccountSetup: collaborationUser.UserId < 1 [{collaborationUser.UserId}]");
                await System.Windows.Application.Current.Dispatcher.Invoke(async delegate
                {
                    await InitializeCollaborationUser();
                });

            }
            else
            {
                Logger!.LogDebug($"FinishAccountSetup: saving collaborationUser [{collaborationUser.UserId}]");
                var gitLabUser = new CollaborationConfiguration
                {
                    UserId = collaborationUser.UserId,
                    RemoteUserName = collaborationUser.RemoteUserName,
                    RemoteEmail = collaborationUser.RemoteEmail,
                    RemotePersonalAccessToken = Encryption.Decrypt(collaborationUser.RemotePersonalAccessToken),
                    RemotePersonalPassword = Encryption.Decrypt(collaborationUser.RemotePersonalPassword),
                    Group = collaborationUser.GroupName,
                    NamespaceId = collaborationUser.NamespaceId
                };

                if (gitLabUser.RemotePersonalAccessToken != CollaborationConfig.RemotePersonalAccessToken)
                {
                    // update to using the remote personal access token from mysql
                    gitLabUser.RemotePersonalAccessToken = CollaborationConfig.RemotePersonalAccessToken;
                }

                _collaborationManager.SaveCollaborationLicense(gitLabUser);
            }

            stopwatch.Stop();
            Logger!.LogDebug($"FinishAccountSetup: elapsed time: {stopwatch.ElapsedMilliseconds} ms");
        }

        private async Task GetRemoteUser()
        {
            if (_collaborationManager.HasRemoteConfigured())
            {
                // collab user present
                CreateCollabUserVisibility = false;

                ShowCollabUserInfo = Visibility.Visible;
                CollaborationConfig = _collaborationManager.GetConfig();


                // get the user's projects
                var list = await _gitLabHttpClientServices.GetProjectsForUser(_collaborationManager.GetConfig());
            }
            else
            {
                // no user present
                CreateCollabUserVisibility = true;
                ShowCollabUserInfo = Visibility.Collapsed;
            }
        }

        public async Task GetProjectsVersion(bool afterMigration = false)
        {
            DashboardProjects.Clear();

            // check for Projects subfolder
            var directories = Directory.GetDirectories(FilePathTemplates.ProjectBaseDirectory);

            if (!IsDashboardRunningAlready() && !afterMigration)
            {
                OpenProjectManager.ClearOpenProjectList();

                OpenProjectManager.AddProjectToOpenProjectList(ProjectManager);
            }

            foreach (var directoryName in directories)
            {
                var directoryInfo = new DirectoryInfo(directoryName);

                // find the Alignment JSONs
                var files = Directory.GetFiles(Path.Combine(FilePathTemplates.ProjectBaseDirectory, directoryName),
                    "*.sqlite");
                foreach (var file in files)
                {
                    var dashboardProject = await FileToDashboardProject(file, directoryInfo);
                    DashboardProjects.Add(dashboardProject);
                }
            }

            _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, SearchText);

            _=Task.Run(async () =>
            {
                await SetDatabaseCompatibility(DashboardProjects);

                _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, SearchText);
                NotifyOfPropertyChange(() => DashboardProjectsDisplay);
            });

            _=Task.Run(async () =>
            {
                await SetGitLabUpdateNeeded(DashboardProjects);

                _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, SearchText);
                NotifyOfPropertyChange(() => DashboardProjectsDisplay);
            });
        }

        private async Task AddNewToDashboardProjectsList()
        {
            var projectsToAdd = new List<DashboardProject>();

            // check for Projects subfolder
            var directories = Directory.GetDirectories(FilePathTemplates.ProjectBaseDirectory);
            
            foreach (var directoryName in directories)
            {
                var directoryInfo = new DirectoryInfo(directoryName);
                if (DashboardProjects.All(x => x.ProjectName != directoryInfo.Name))
                {
                    // find the Alignment JSONs
                    var files = Directory.GetFiles(Path.Combine(FilePathTemplates.ProjectBaseDirectory, directoryName), "*.sqlite");
                    foreach (var file in files)
                    {
                        var dashboardProject = await FileToDashboardProject(file, directoryInfo);

                        projectsToAdd.Add(dashboardProject);
                    }
                }
            }

            DashboardProjects.AddRange(projectsToAdd);
            DashboardProjects.Sort((x, y) => string.Compare(x.ProjectName, y.ProjectName));

            _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, SearchText);

            _=Task.Run(async () =>
            {
                await SetDatabaseCompatibility(projectsToAdd);

                _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, SearchText);
                NotifyOfPropertyChange(() => DashboardProjectsDisplay);
            });
            

            _=Task.Run(async () =>
            {
                await SetGitLabUpdateNeeded(projectsToAdd);

                _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, SearchText);
                NotifyOfPropertyChange(() => DashboardProjectsDisplay);
            });
        }

        private async Task<DashboardProject> FileToDashboardProject(string file, DirectoryInfo directoryInfo)
        {
            var fileInfo = new FileInfo(file);

            string version = "unavailable";

            var results = await ExecuteRequest(new GetProjectVersionQuery(fileInfo.FullName), CancellationToken.None);
            if (results.Success && results.HasData)
            {
                version = results.Data ?? version;
            }

            Guid guid = Guid.Empty;
            results = await ExecuteRequest(new GetProjectIdQuery(fileInfo.FullName), CancellationToken.None);
            if (results.Success && results.HasData)
            {
                try
                {
                    guid = Guid.Parse(results.Data.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            string gitLabSha = string.Empty;
            string userId = string.Empty;
            bool shaPresent = false;
            results = await ExecuteRequest(new GetProjectGitLabShaQuery(fileInfo.FullName), CancellationToken.None);
            if (results.Success && results.HasData)
            {
                if (results.Data.ToString() != "")
                {
                    shaPresent = true;
                    gitLabSha = results.Data.ToString();
                    userId = results.Data.ToString();
                }
            }

            // add as ListItem
            var dashboardProject = new DashboardProject
            {
                Modified = fileInfo.LastWriteTime,
                ProjectName = directoryInfo.Name,
                ShortFilePath = fileInfo.Name,
                FullFilePath = fileInfo.FullName,
                Version = version,
                IsCollabProject = shaPresent,
                GitLabSha = gitLabSha,
                Id = guid,
                UserId=userId
            };
            return dashboardProject;
        }

        private async Task SetGitLabUpdateNeeded(List<DashboardProject> projects)
        {

            // get the collab most recent Sha
            foreach (var project in projects)
            {
                if (project.IsCollabProject)
                {
                    var results = await ExecuteRequest(new GetGitLabUpdatedNeededQuery(project), CancellationToken.None);
                    if (results.Success && results.HasData)
                    {
                        if (results.Data == true)
                        {
                            project.GitLabUpdateNeeded = true;
                        }
                        else
                        {
                            project.GitLabUpdateNeeded = false;
                        }
                    }
                }
            }
        }

        private static async Task SetDatabaseCompatibility(List<DashboardProject> projects)
        {
            // check for database compatibility
            foreach (var project in projects)
            {
                project.IsCompatibleVersion = await ReleaseNotesManager.CheckVersionCompatibility(project.Version);

                if (project.IsCompatibleVersion)
                {
                    MigrationChecker migrationChecker = new MigrationChecker(project.Version);
                    project.NeedsMigrationUpgrade = migrationChecker.CheckForResetVerseMappings();
                }
            }
        }

        public async void UpdateDatabase(DashboardProject project)
        {
            var localizedString = _localizationService!["Migrate_Header"];

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 500;
            settings.MinHeight = 500;
            settings.Title = $"{localizedString}";

            // Keep the window on top
            //settings.Topmost = true;
            settings.Owner = System.Windows.Application.Current.MainWindow;

            var viewModel = IoC.Get<MigrateDatabaseViewModel>();
            viewModel.Project = project;
            viewModel.ProjectPickerViewModel = this;

            IWindowManager manager = new WindowManager();
            await manager.ShowDialogAsync(viewModel, null, settings);
        }

        public async Task RefreshProjectList()
        {
            await GetProjectsVersion(afterMigration: true);
        }


        private async Task CheckTokenExpiration()
        {
            var userConfig = _collaborationManager.GetConfig();
            var token = await _gitLabHttpClientServices.GetTokenExpirationDate(userConfig);
            
            DateTime expireDate = DateTime.Now.AddDays(-60);
            if (token is null)
            {
                // generate a new token as the user has none
                var tempGitLabUser = new GitLabUser
                {
                    Id = userConfig.UserId,
                    UserName = userConfig.RemoteUserName,
                };
                var newToken = await _gitLabHttpClientServices.GeneratePersonalAccessToken(tempGitLabUser);
                token = await _gitLabHttpClientServices.GetTokenExpirationDate(userConfig);
                expireDate = DateTime.Parse(token.ExpiresAt);
            }
            else
            {
                try
                {
                    expireDate = DateTime.Parse(token.ExpiresAt);
                }
                catch (Exception)
                {
                    return;
                }
            }

            // check to see if the token has expires in more than 30 days
            // then bail out as nothing to do
            if (expireDate > DateTime.Now.AddDays(30))
            {
                return;
            }

            // check to see if there is a new token that has been generated up on the mysql server
            var gitLabUser = await _collaborationHttpClientServices.GetCollabUserExistsById(userConfig.UserId);
            var remotePersonalAccessToken = Encryption.Decrypt(gitLabUser.RemotePersonalAccessToken);

            if (remotePersonalAccessToken != userConfig.RemotePersonalAccessToken)
            {
                // the token has been updated so lets update the local config
                userConfig.RemotePersonalAccessToken = remotePersonalAccessToken;
                _collaborationManager.SaveCollaborationLicense(userConfig);
            }
            else
            {
                // less than 30 days left on the token so lets regenerate a new one
                var result = await _gitLabHttpClientServices.RefreshToken(_collaborationManager.GetConfig(), token);
            }
        }

        public async Task GetCollabProjects()
        {
            DashboardCollabProjects.Clear();

            // get the list of those GitLab projects that haven't been sync'd locally
            var projects = await _gitLabHttpClientServices.GetProjectsForUser(_collaborationManager.GetConfig());
            projects = projects.OrderByDescending(e => e.CreatedAt).ToList();
            var dashboardProjectsToRemove = new List<DashboardProject>();
            foreach (var dashboardProject in DashboardProjects)
            {
                var results =
                    await ExecuteRequest(new GetProjectIdSlice.GetProjectIdQuery(dashboardProject.FullFilePath), CancellationToken.None);

                if (results.Success)
                {
                    var projectGuid = results.Data;
                    var gitLabProject = projects.FirstOrDefault(x => projectGuid == x.Name.ToUpper().Replace("P_", ""));

                    if (gitLabProject is not null)
                    {
                        dashboardProject.IsCollabProject = true;
                        dashboardProject.CollabOwner = gitLabProject.RemoteOwner.Name;
                        dashboardProject.PermissionLevel = gitLabProject.RemotePermissionLevel;

                        if (dashboardProject.PermissionLevel == PermissionLevel.None &&
                            Guid.Parse(dashboardProject.UserId) != ProjectManager.CurrentUser.Id)
                        {
                            dashboardProjectsToRemove.Add(dashboardProject);
                        }

                        // remove from the available GitLab projects
                        projects.Remove(gitLabProject);
                    }
                    else
                    {
                        if (dashboardProject.IsCollabProject)
                        {
                            // project has been removed from GitLab so remove the SHA from the database
                            var resultDelete = await ExecuteRequest(new ResetProjectGitLabShaQuery(dashboardProject.FullFilePath), CancellationToken.None);
                            if (resultDelete.Success)
                            {
                                Logger!.LogDebug($"ResetProjectGitLabShaQuery: {dashboardProject.ProjectName} {dashboardProject.Id} SHA Removed from Project table");
                            }

                            dashboardProject.IsCollabProject = false;
                        }
                    }
                }
            }

            // create the GitLab unsync'd projects
            foreach (var gitLabProject in projects.OrderByDescending(e => e.CreatedAt))
            {
                // check to see if this is a Dashboard project with expected naming
                if (gitLabProject.Name.StartsWith("P_") && gitLabProject.Name.Length == 38)
                {
                    var dashboardCollabProject = new DashboardCollabProject
                    {
                        ProjectId = Guid.Parse(gitLabProject.Name.ToUpper().Replace("P_", "")),
                        ProjectName = gitLabProject.Description.ToString()!,
                        AppVersion = "unknown",
                        Created = gitLabProject.CreatedAt,
                        RemoteOwner = gitLabProject.RemoteOwner.Name,
                        PermissionLevel = gitLabProject.RemotePermissionLevel,
                    };
                    DashboardCollabProjects.Add(dashboardCollabProject);

                }

            }

            if (_dashboardCollabProjectsDisplay is null)
            {
                _dashboardCollabProjectsDisplay = new();
            }

            DashboardCollabProjectsDisplay = DashboardCollabProjects;

            if (DashboardCollabProjectsDisplay.Count() > 0)
            {
                CollabButtonsEnabled = true;
            }

            _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, SearchText);

            NotifyOfPropertyChange(nameof(DashboardProjectsDisplay));
        }

        private void SetCollabVisibility()
        {
            if (!_collaborationManager.HasRemoteConfigured())
            {
                CollabProjectVisibility = Visibility.Collapsed;
                InitializeCollaborationVisibility = Visibility.Collapsed;
            }
            else
            {
                if (!_collaborationManager.IsRepositoryInitialized())
                {
                    CollabProjectVisibility = Visibility.Collapsed;
                    CollabButtonsEnabled = InternetAvailability.IsInternetAvailable();
                    InitializeCollaborationVisibility = Visibility.Visible;
                    InitializeCollaborationLabel = "Initialize Collaboration";
                }
                else
                {
                    CollabProjectVisibility = Visibility.Visible;
                    CollabButtonsEnabled = InternetAvailability.IsInternetAvailable();
                    InitializeCollaborationVisibility = Visibility.Visible;
                    InitializeCollaborationLabel = "Refresh Server Projects";
                }
            }
        }

        private bool IsDashboardRunningAlready()
        {
            var process = Process.GetProcessesByName("ClearDashboard.Wpf.Application");
            return process.Length > 1;
        }

        private async void ListenForParatextStart()
        {
            while (!IsParatextRunning || !Connected)
            {
                IsParatextRunning = await Task.Run(() => _paratextProxy.IsParatextRunning());
            }
        }

        public BindableCollection<DashboardProject> CopyDashboardProjectsToAnother(List<DashboardProject> original, string searchTerm = "")
        {
            BindableCollection<DashboardProject> copy = new();

            foreach (var project in original)
            {
                copy.Add(project);
            }

            copy.RemoveAll(project=>!project.ProjectName.ToLower().Contains((searchTerm??string.Empty).ToLower().Replace(' ', '_')));

            return copy;
        }
        public BindableCollection<DashboardCollabProject> CopyDashboardCollabProjectsToAnother(BindableCollection<DashboardCollabProject> original)
        {
            BindableCollection<DashboardCollabProject> copy = new();

            foreach (var project in original)
            {
                copy.Add(project);
            }

            return copy;
        }
        public void AlertClose()
        {
            AlertVisibility = Visibility.Collapsed;
        }

        public async void NavigateToMainViewModel(DashboardProject? project, MouseButtonEventArgs args)
        {
            if (project == null)
            {
                return;
            }
            // Only respond to a Left button click otherwise,
            // the context menu will not be shown on a right click.
            if (args.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            if (CheckIfConnectedToParatext() == false)
            {
                var confirmationViewPopupViewModel = LifetimeScope!.Resolve<ConfirmationPopupViewModel>();

                if (confirmationViewPopupViewModel == null)
                {
                    throw new ArgumentNullException(nameof(confirmationViewPopupViewModel), "ConfirmationPopupViewModel needs to be registered with the DI container.");
                }

                confirmationViewPopupViewModel.SimpleMessagePopupMode = SimpleMessagePopupMode.StartParatextFirst;

                await _windowManager.ShowDialogAsync(confirmationViewPopupViewModel, null,
                    SimpleMessagePopupViewModel.CreateDialogSettings(confirmationViewPopupViewModel.Title));

                return;
            }

            if (ProjectLoadingProgressBarVisibility == Visibility.Visible)
            {
                return;
            }
            
            var currentlyOpenProjectsList = OpenProjectManager.DeserializeOpenProjectList();
            if (currentlyOpenProjectsList.Contains(project.ProjectName))
            {
                AlreadyOpenMessageVisibility = Visibility.Visible;
                return;
            }
            AlreadyOpenMessageVisibility = Visibility.Collapsed;

            try
            {
                ProjectLoadingProgressBarVisibility = Visibility.Visible;

                ProjectManager!.CurrentDashboardProject = project;

                // fix any hyphenated label groups in the database
                await Mediator.Send(new UpdateDatabaseHyphenQuery(ProjectManager.CurrentDashboardProject));
                
                await EventAggregator.PublishOnUIThreadAsync(
                    new DashboardProjectNameMessage(ProjectManager!.CurrentDashboardProject.ProjectName));
                await EventAggregator.PublishOnUIThreadAsync(
                    new DashboardProjectPermissionLevelMessage(project.PermissionLevel));

               
                //Turn on ProgressBar

                ParentViewModel!.ExtraData = project;


                var projectGuid =
                    await ExecuteRequest(new GetProjectIdSlice.GetProjectIdQuery(project.FullFilePath),
                        CancellationToken.None);


                // get the user's projects
                var projectList = await _gitLabHttpClientServices.GetProjectsForUser(_collaborationManager.GetConfig());

                foreach (var gitLabProject in projectList)
                {
                    var guid = gitLabProject.Name.Substring(2).ToUpper();
                    if (guid == projectGuid.Data)
                    {
                        _collaborationManager.SetRemoteUrl(gitLabProject.HttpUrlToRepo, gitLabProject.Name);
                        break;
                    }
                }


                ParentViewModel.Ok();
            }
            finally
            {
                OpenProjectManager.AddProjectToOpenProjectList(ProjectManager);
                ProjectLoadingProgressBarVisibility = Visibility.Collapsed;
            }
        }

        public async Task ImportServerProject(DashboardCollabProject project, MouseButtonEventArgs args)
        {
            // Only respond to a Left button click otherwise,
            // the context menu will not be shown on a right click.
            if (args.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            if (CheckIfConnectedToParatext() == false)
            {
                var confirmationViewPopupViewModel = LifetimeScope!.Resolve<ConfirmationPopupViewModel>();

                if (confirmationViewPopupViewModel == null)
                {
                    throw new ArgumentNullException(nameof(confirmationViewPopupViewModel), "ConfirmationPopupViewModel needs to be registered with the DI container.");
                }

                confirmationViewPopupViewModel.SimpleMessagePopupMode = SimpleMessagePopupMode.StartParatextFirst;

                await _windowManager.ShowDialogAsync(confirmationViewPopupViewModel, null,
                    SimpleMessagePopupViewModel.CreateDialogSettings(confirmationViewPopupViewModel.Title));

                return;
            }

            if (project.ProjectId == Guid.Empty)
            {
                return;
            }

            if (_longRunningTaskManager.HasTask(_importTaskName))
            {
                return;
            }

            // check to see if there is an existing project with the same name
            var p = DashboardProjects.FirstOrDefault(x => x.ProjectName == project.ProjectName);
            if (p is not null)
            {
                var confirmationViewPopupViewModel = LifetimeScope!.Resolve<ConfirmationPopupViewModel>();

                if (confirmationViewPopupViewModel == null)
                {
                    throw new ArgumentNullException(nameof(confirmationViewPopupViewModel), "ConfirmationPopupViewModel needs to be registered with the DI container.");
                }

                confirmationViewPopupViewModel.SimpleMessagePopupMode = SimpleMessagePopupMode.ExistingProjectNameTheSame;

                var result = await _windowManager!.ShowDialogAsync(confirmationViewPopupViewModel, null,
                    SimpleMessagePopupViewModel.CreateDialogSettings(confirmationViewPopupViewModel.Title));
                return;
            }



            _longRunningTaskManager.Create(_importTaskName, LongRunningTaskStatus.Running);

            // get the user's projects
            var projectList = await _gitLabHttpClientServices.GetProjectsForUser(_collaborationManager.GetConfig());

            foreach (var gitLabProject in projectList)
            {
                var guid = gitLabProject.Name.Substring(2);
                if (Guid.Parse(guid) == project.ProjectId)
                {
                    _collaborationManager.SetRemoteUrl(gitLabProject.HttpUrlToRepo, gitLabProject.Name);
                    break;
                }
            }



            var importServerProjectViewModel = LifetimeScope?.Resolve<MergeServerProjectDialogViewModel>();
            if (importServerProjectViewModel != null)
            {
                importServerProjectViewModel.ProjectId = project.ProjectId;
                importServerProjectViewModel.ProjectName = project.ProjectName;
                importServerProjectViewModel.CollaborationDialogAction = CollaborationDialogAction.Import;
                var result = await _windowManager.ShowDialogAsync(importServerProjectViewModel, null, importServerProjectViewModel.DialogSettings());

                if(result)
                {
                    await AddNewToDashboardProjectsList();
                    await GetCollabProjects();
                }
            }

            _longRunningTaskManager.TaskComplete(_importTaskName);
        }

        private async Task SendUiLanguageChangeMessage(string language)
        {
            await EventAggregator.PublishOnUIThreadAsync(new UiLanguageChangedMessage(language));
        }

        private bool CheckIfConnectedToParatext()
        {

            //if (IsParatextRunning)
            //{
            //    return true;
            //}
            if (ProjectManager?.HasCurrentParatextProject == false || !IsParatextRunning)
            {
                AlertVisibility = Visibility.Visible;
                return false;
            }
            return true;
        }

        public void DeleteProject(DashboardProject project)
        {
            if (!project.HasFullFilePath)
            {
                return;
            }

            // confirm the user wants to delete the project
            var confirmationViewPopupViewModel = LifetimeScope!.Resolve<ConfirmationPopupViewModel>();

            if (confirmationViewPopupViewModel == null)
            {
                throw new ArgumentNullException(nameof(confirmationViewPopupViewModel), "ConfirmationPopupViewModel needs to be registered with the DI container.");
            }

            confirmationViewPopupViewModel.SimpleMessagePopupMode = SimpleMessagePopupMode.DeleteProjectConfirmation;

            bool result = false;
            OnUIThread(async () =>
            {
                result = await _windowManager!.ShowDialogAsync(confirmationViewPopupViewModel, null,
                    SimpleMessagePopupViewModel.CreateDialogSettings(confirmationViewPopupViewModel.Title));
            });

            if (!result)
            {
                return;
            }


            var fileInfo = new FileInfo(project.FullFilePath ?? throw new InvalidOperationException("Project full file path is null."));

            try
            {
                // remove the dashboard project from the list
                var directoryInfo = new DirectoryInfo(fileInfo.DirectoryName ?? throw new InvalidOperationException("File directory name is null."));

                foreach (var file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }
                foreach (var directory in directoryInfo.GetDirectories())
                {
                    directory.Delete(true);
                }

                directoryInfo.Delete();

                DashboardProjects.Remove(project);
                DashboardProjectsDisplay.Remove(project);

                var originalDatabaseCopyName = $"{project.ProjectName}_original.sqlite";
                File.Delete(Path.Combine(directoryInfo.Parent.ToString(), originalDatabaseCopyName));
                DeleteErrorVisibility = Visibility.Collapsed;

                // remove the collaboration project directory
                var path = Path.Combine(_collaborationManager.GetRespositoryBasePath(), "P_" + project.Id);
                if (Directory.Exists(path))
                {
                    FileAttributesHelper.SetNormalFileAttributes(path);

                    directoryInfo = new DirectoryInfo(path);
                    foreach (var file in directoryInfo.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (var directory in directoryInfo.GetDirectories())
                    {
                        directory.Delete(true);
                    }

                    directoryInfo.Delete();
                }

            }
            catch (Exception e)
            {
                DeleteErrorVisibility = Visibility.Visible;
                Logger?.LogError(e, "An unexpected error occurred while deleting a project.");
            }
        }





        public void SetLanguage()
        {
            var culture = Settings.Default.language_code;
            // strip out any "-" characters so the string can be properly parsed into the target enum
            SelectedLanguage = (LanguageTypeValue)Enum.Parse(typeof(LanguageTypeValue), culture.Replace("-", string.Empty));

            var languageFlowDirection = SelectedLanguage.GetAttribute<RTLAttribute>();
            if (languageFlowDirection.isRTL)
            {
                //TODO ProjectManager.CurrentLanguageFlowDirection = FlowDirection.RightToLeft;
            }
            else
            {
                //ProjectManager.CurrentLanguageFlowDirection = FlowDirection.LeftToRight;
            }

            //WindowFlowDirection = ProjectManager.CurrentLanguageFlowDirection;
        }

        protected override void OnViewAttached(object view, object context)
        {
            SetLanguage();
            base.OnViewAttached(view, context);
            DeleteErrorVisibility = Visibility.Collapsed;
        }

        private static void SaveUserLanguage(string language)
        {
            Settings.Default.language_code = language;
            Settings.Default.Save();
        }

        public async void Create()
        {
            await MoveForwards();
            await EventAggregator.PublishOnUIThreadAsync(new CreateProjectMessage(SearchText));
        }

       
        public async Task CreateProjectWithProjectTemplate()
        {
            ParentViewModel.Reset();    

            var projectTemplateItems = ParentViewModel.Steps.Skip(3);
            //foreach (var item in projectTemplateItems)
            //{
            //    await item.Reset(CancellationToken.None);
            //}

            await ParentViewModel!.GoToStep(3);
        }

        public async void ShowCollaborationGetLatest(DashboardProject project)
        {
            await ProjectManager.LoadProject(project.ProjectName);
            if (_collaborationManager.IsCurrentProjectInRepository() && InternetAvailability.IsInternetAvailable())
            {
                var viewModel = LifetimeScope?.Resolve<MergeServerProjectDialogViewModel>();

                if (viewModel is not null)
                {
                    
                    viewModel.ProjectId = ProjectManager.CurrentProject.Id;
                    viewModel.ProjectName = ProjectManager.CurrentProject.ProjectName;
                    viewModel.CollaborationDialogAction = CollaborationDialogAction.Merge;

                    this._windowManager.ShowDialogAsync(viewModel, null, viewModel.DialogSettings());

                    if (viewModel.CommitSha is not null)
                    {
                        
                        ProjectManager.CurrentProject.LastMergedCommitSha = viewModel.CommitSha;
                        await ProjectManager.UpdateProject(ProjectManager.CurrentProject);
                        project.GitLabUpdateNeeded = false;
                    }
                }
            }

            await GetProjectsVersion();
        }

        #endregion  Methods

        public async Task HandleAsync(ParatextConnectedMessage message, CancellationToken cancellationToken)
        {
            if (!message.Connected && Connected) //only run when going from connected to not connected
            {
                IsParatextRunning=false;
                ListenForParatextStart();
            }

            Connected = message.Connected;

            await Task.CompletedTask;
        }

        public async Task HandleAsync(UserMessage message, CancellationToken cancellationToken)
        {
            ParatextUserName = message.User.ParatextUserName;
            await Task.CompletedTask;
        }


        /// <summary>
        /// Reload the project picker projects/collab projecst
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(ReloadProjectPickerProjects message, CancellationToken cancellationToken)
        {
            await GetProjectsVersion();

            if (Pinger.PingHost())
            {
                await GetCollabProjects();
            }
        }

        /// <summary>
        /// Remove the project's SHA from the project table
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(DeletedGitProject message, CancellationToken cancellationToken)
        {
            var project = _dashboardProjectsDisplay.FirstOrDefault(x => x.Id == message.Guid);

            if (project is not null)
            {
                var results = await ExecuteRequest(new ResetProjectGitLabShaQuery(project.FullFilePath), CancellationToken.None);
                if (results.Success)
                {
                    Logger!.LogDebug($"ResetProjectGitLabShaQuery: {project.ProjectName} {project.Id} SHA Removed from Project table");
                }
            }
        }
    }
}
