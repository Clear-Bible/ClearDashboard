﻿using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Collaboration;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static ClearDashboard.DataAccessLayer.Features.DashboardProjects.GetProjectVersionSlice;
using Resources = ClearDashboard.Wpf.Application.Strings.Resources;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class ProjectPickerViewModel : DashboardApplicationWorkflowStepViewModel<StartupDialogViewModel>, IHandle<ParatextConnectedMessage>, IHandle<UserMessage>
    {
        #region Member Variables
        private readonly ParatextProxy _paratextProxy;
        private readonly IMediator _mediator;
        private readonly HttpClientServices _httpClientServices;
        private readonly ILocalizationService _localizationService;
        private readonly TranslationSource? _translationSource;
        private readonly IWindowManager _windowManager;
        private readonly CollaborationManager _collaborationManager;

        private string _projectDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"ClearDashboard_Projects");
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


        private ObservableCollection<DashboardProject> _dashboardProjects = new();
        public ObservableCollection<DashboardProject> DashboardProjects
        {
            get => _dashboardProjects;
            set
            {
                _dashboardProjects = value; 
                NotifyOfPropertyChange(() => DashboardProjects);   
            }
        }

        private ObservableCollection<DashboardCollabProject> _dashboardCollabProjects = new();
        public ObservableCollection<DashboardCollabProject> DashboardCollabProjects
        {
            get => _dashboardCollabProjects;
            set
            {
                _dashboardCollabProjects = value;
                NotifyOfPropertyChange(() => DashboardCollabProjects);
            }
        }

        private Visibility _createCollabUserVisibility = Visibility.Collapsed;
        public Visibility CreateCollabUserVisibilityVisibility
        {
            get => _createCollabUserVisibility;
            set
            {
                _createCollabUserVisibility = value;
                NotifyOfPropertyChange(() => CreateCollabUserVisibilityVisibility);
            }
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

                if (SearchText == string.Empty || SearchText is null)
                {
                    _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, _dashboardProjectsDisplay);
                    SearchBlankVisibility = Visibility.Collapsed;
                    NoProjectVisibility = Visibility.Visible;
                }
                else
                {
                    _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, _dashboardProjectsDisplay);
                    _dashboardProjectsDisplay.RemoveAll(project => !project.ProjectName.ToLower().Contains(SearchText.ToLower().Replace(' ','_')));
                }

                if (_dashboardProjectsDisplay.Count <= 0 && DashboardProjects.Count>0)
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

        private ObservableCollection<DashboardProject>? _dashboardProjectsDisplay = new();
        public ObservableCollection<DashboardProject>? DashboardProjectsDisplay
        {
            get => _dashboardProjectsDisplay;
            set
            {
                _dashboardProjectsDisplay = value;
                NotifyOfPropertyChange(() => DashboardProjectsDisplay);
            }
        }

        private ObservableCollection<DashboardCollabProject>? _dashboardCollabProjectsDisplay;

        public ObservableCollection<DashboardCollabProject>? DashboardCollabProjectsDisplay
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

        #endregion

        #region Constructor
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
            HttpClientServices httpClientServices,
            CollaborationManager collaborationManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            Logger?.LogInformation("Project Picker constructor called.");
            //_windowManager = windowManager;
            _paratextProxy = paratextProxy;
            _mediator = mediator;
            _httpClientServices = httpClientServices;
            _localizationService = localizationService;
            AlertVisibility = Visibility.Collapsed;
            _translationSource = translationSource;
            NoProjectVisibility = Visibility.Visible;
            SearchBlankVisibility = Visibility.Collapsed;

            _windowManager = windowManager;
            _collaborationManager = collaborationManager;
            SetCollabVisibility();

            IsParatextRunning = _paratextProxy.IsParatextRunning();
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

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await GetRemoteUser();
            await GetProjectsVersion().ConfigureAwait(false);
            await GetCollabProjects().ConfigureAwait(false);



            IsParatextRunning = _paratextProxy.IsParatextRunning();
            IsParatextInstalled = _paratextProxy.IsParatextInstalled();
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
            if (!IsParatextInstalled)
            {
                // await this.ShowMessageAsync("This is the title", "Some message");
            }

            await base.OnActivateAsync(cancellationToken);
        }

        #endregion Constructor

        #region Methods
        public void InitializeCollaborationUser()
        {
            var localizedString = _localizationService!["MainView_About"];

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 500;
            settings.MinHeight = 500;
            settings.Title = $"{localizedString}";

            var viewModel = IoC.Get<NewCollabUserViewModel>();

            IWindowManager manager = new WindowManager();
            manager.ShowDialogAsync(viewModel, null, settings);
        }

        private async Task GetRemoteUser()
        {
            if (_collaborationManager.HasRemoteConfigured())
            {
                // collab user present
                CreateCollabUserVisibilityVisibility = Visibility.Collapsed;

                ShowCollabUserInfo = Visibility.Visible;
                CollaborationConfig = _collaborationManager.GetConfig();
            }
            else
            {
                // no user present
                CreateCollabUserVisibilityVisibility = Visibility.Visible;
                ShowCollabUserInfo = Visibility.Collapsed;
            }
        }

        private async Task GetProjectsVersion(bool afterMigration=false)
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
                    var fileInfo = new FileInfo(file);

                    string version = "unavailable";


                    var results =
                        await ExecuteRequest(new GetProjectVersionQuery(fileInfo.FullName), CancellationToken.None);
                    if (results.Success && results.HasData)
                    {
                        version = results.Data;
                    }

                    // add as ListItem
                    var dashboardProject = new DashboardProject
                    {
                        Modified = fileInfo.LastWriteTime,
                        ProjectName = directoryInfo.Name,
                        ShortFilePath = fileInfo.Name,
                        FullFilePath = fileInfo.FullName,
                        Version = version,
                    };

                    DashboardProjects.Add(dashboardProject);
                }
            }

            foreach (var project in DashboardProjects)
            {
                project.IsCompatibleVersion = await ReleaseNotesManager.CheckVersionCompatibility(project.Version).ConfigureAwait(true);

                if (project.IsCompatibleVersion)
                {
                    MigrationChecker migrationChecker = new MigrationChecker(project.Version);
                    project.NeedsMigrationUpgrade = migrationChecker.CheckForResetVerseMappings();
                }
            }

            DashboardProjectsDisplay.Clear();
            _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, _dashboardProjectsDisplay);

            NotifyOfPropertyChange(() => DashboardProjectsDisplay);
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

            var viewModel = IoC.Get<MigrateDatabaseViewModel>();
            viewModel.Project = project;
            viewModel.ProjectPickerViewModel = this;

            IWindowManager manager = new WindowManager();
            manager.ShowDialogAsync(viewModel, null, settings);
        }

        public async Task RefreshProjectList()
        {
            await GetProjectsVersion(true);
        }

        private async Task GetCollabProjects()
        {
            DashboardCollabProjects.Clear();

            if (_collaborationManager.IsRepositoryInitialized())
            {
                var dashboardProjectNames = DashboardProjects?
                    .Select(e => ProjectDbContextFactory.ConvertProjectNameToSanitizedName(e.ProjectName!))
                    .ToList();

                var projects = _collaborationManager.GetAllProjects();
                foreach (var project in projects
                    .OrderByDescending(e => e.created))
                {
                    if (dashboardProjectNames is null || !dashboardProjectNames
                        .Contains(ProjectDbContextFactory.ConvertProjectNameToSanitizedName(project.projectName)))
                    {
                        var dashboardCollabProject = new DashboardCollabProject
                        {
                            ProjectId = project.projectId,
                            ProjectName = project.projectName,
                            AppVersion = project.appVersion,
                            Created = project.created
                        };
                        DashboardCollabProjects.Add(dashboardCollabProject);
                    }
                }
            }

            foreach (var project in DashboardCollabProjects)
            {
                project.IsCompatibleVersion = await ReleaseNotesManager.CheckVersionCompatibility(project.AppVersion!).ConfigureAwait(true);
            }

            if (_dashboardCollabProjectsDisplay is null)
            {
                _dashboardCollabProjectsDisplay = new();
            }

            CopyDashboardCollabProjectsToAnother(DashboardCollabProjects, _dashboardCollabProjectsDisplay);
        }

        private void SetCollabVisibility()
        {
#if COLLAB_RELEASE || COLLAB_DEBUG
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
#else
            CollabProjectVisibility = Visibility.Collapsed;
            InitializeCollaborationVisibility = Visibility.Collapsed;
#endif
        }

        private bool IsDashboardRunningAlready()
        {
            var process = Process.GetProcessesByName("ClearDashboard.Wpf.Application");
            return process.Length > 1;
        }

        private async void ListenForParatextStart()
        {
            while (!IsParatextRunning)
            {
                IsParatextRunning = await Task.Run(()=>_paratextProxy.IsParatextRunning()).ConfigureAwait(false);
                Thread.Sleep(1000);
            }
        }

        public ObservableCollection<DashboardProject>? CopyDashboardProjectsToAnother(ObservableCollection<DashboardProject> original, ObservableCollection<DashboardProject>? copy)
        {
            copy.Clear();
            foreach (var project in original)
            {
                copy.Add(project);
            }
            return copy;
        }
        public ObservableCollection<DashboardCollabProject>? CopyDashboardCollabProjectsToAnother(ObservableCollection<DashboardCollabProject> original, ObservableCollection<DashboardCollabProject>? copy)
        {
            copy.Clear();
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

        public void NavigateToMainViewModel(DashboardProject project, MouseButtonEventArgs args)
        {

            // Only respond to a Left button click otherwise,
            // the context menu will not be shown on a right click.
            if (args.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
      
            if (CheckIfConnectedToParatext() == false)
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
            

            ProjectManager!.CurrentDashboardProject = project;
            EventAggregator.PublishOnUIThreadAsync(new DashboardProjectNameMessage(ProjectManager!.CurrentDashboardProject.ProjectName));

            OpenProjectManager.AddProjectToOpenProjectList(ProjectManager);

            ParentViewModel!.ExtraData = project;
            ParentViewModel.Ok();
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

            await GetCollabProjects().ConfigureAwait(false);
            SetCollabVisibility();
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
                return;
            }
            
            if (project.ProjectId == Guid.Empty)
            {
                return;
            }

            var importServerProjectViewModel = LifetimeScope?.Resolve<MergeServerProjectDialogViewModel>();
            if (importServerProjectViewModel != null)
            {
                importServerProjectViewModel.ProjectId = project.ProjectId;
                importServerProjectViewModel.ProjectName = project.ProjectName;
                importServerProjectViewModel.CollaborationDialogAction = CollaborationDialogAction.Import;
                var result = await _windowManager.ShowDialogAsync(importServerProjectViewModel, null, importServerProjectViewModel.DialogSettings());

                await GetProjectsVersion().ConfigureAwait(false);
                await GetCollabProjects().ConfigureAwait(false);
            }
        }

        private async Task SendUiLanguageChangeMessage(string language)
        {
            await EventAggregator.PublishOnUIThreadAsync(new UiLanguageChangedMessage(language)).ConfigureAwait(false);
        }

        private bool CheckIfConnectedToParatext()
        {
            if (ProjectManager?.HasCurrentParatextProject == false)
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
            var fileInfo = new FileInfo(project.FullFilePath ?? throw new InvalidOperationException("Project full file path is null."));

            try
            {
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

        public void Create()
        {
            MoveForwards();
            EventAggregator.PublishOnUIThreadAsync(new CreateProjectMessage(SearchText));
        }

#endregion  Methods

        public async Task HandleAsync(ParatextConnectedMessage message, CancellationToken cancellationToken)
        {
            Connected = message.Connected;
            IsParatextRunning = true;

            await Task.CompletedTask;
        }

        public async Task HandleAsync(UserMessage message, CancellationToken cancellationToken)
        {
            ParatextUserName = message.User.ParatextUserName;
            await Task.CompletedTask;
        }
    }
}
