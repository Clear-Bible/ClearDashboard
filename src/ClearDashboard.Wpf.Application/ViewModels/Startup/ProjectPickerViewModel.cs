using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Extensions;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Services;
using static ClearDashboard.DataAccessLayer.Features.DashboardProjects.GetProjectVersionSlice;
using Resources = ClearDashboard.Wpf.Application.Strings.Resources;
using System.Diagnostics;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using System.Dynamic;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class ProjectPickerViewModel : DashboardApplicationWorkflowStepViewModel<StartupDialogViewModel>, IHandle<ParatextConnectedMessage>, IHandle<UserMessage>
    {
        #region Member Variables
        private readonly ParatextProxy _paratextProxy;
        private readonly IMediator _mediator;
        private readonly ILocalizationService _localizationService;
        private readonly TranslationSource? _translationSource;

        private string _projectDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"ClearDashboard_Projects");
        #endregion

        #region Observable Objects
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
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            Logger?.LogInformation("Project Picker constructor called.");
            //_windowManager = windowManager;
            _paratextProxy = paratextProxy;
            _mediator = mediator;
            _localizationService = localizationService;
            AlertVisibility = Visibility.Collapsed;
            _translationSource = translationSource;
            NoProjectVisibility = Visibility.Visible;
            SearchBlankVisibility = Visibility.Collapsed;

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
            await GetProjectsVersion().ConfigureAwait(false);

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

        private async Task GetProjectsVersion(DashboardProject? updatedProject = null)
        {
            DashboardProjects.Clear();

            // check for Projects subfolder
            var directories = Directory.GetDirectories(FilePathTemplates.ProjectBaseDirectory);

            if (!IsDashboardRunningAlready())
            {
                OpenProjectManager.ClearOpenProjectList();

                OpenProjectManager.AddProjectToOpenProjectList(ProjectManager);
            }

            var currentlyOpenProjectsList = OpenProjectManager.DeserializeOpenProjectList();

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

                    bool isClosed = true;
                    if (currentlyOpenProjectsList.Contains(directoryInfo.Name))
                    {
                        if (updatedProject is not null && updatedProject.FullFilePath == file)
                        {
                            isClosed = true; // post migration project
                        }
                        else
                        {
                            isClosed = false;
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
                        IsClosed = isClosed
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

        public async Task RefreshProjectList(DashboardProject? dashboardProject)
        {
            await GetProjectsVersion(dashboardProject);
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

            ProjectManager!.CurrentDashboardProject = project;
            
            OpenProjectManager.AddProjectToOpenProjectList(ProjectManager);

            ParentViewModel!.ExtraData = project;
            ParentViewModel.Ok();
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
