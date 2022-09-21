using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Features.DashboardProjects;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Strings;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using SIL.Extensions;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class ProjectPickerViewModel : DashboardApplicationWorkflowStepViewModel<StartupDialogViewModel>, IHandle<ParatextConnectedMessage>, IHandle<UserMessage>
    {
        #region Member Variables
        private readonly ParatextProxy _paratextProxy;
        private readonly TranslationSource? _translationSource;
        #endregion

        #region Observable Objects
        public ObservableCollection<DashboardProject>? DashboardProjects { get; set; } = new ObservableCollection<DashboardProject>();

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
                _searchText = value;
                NotifyOfPropertyChange(() => SearchText);

                if (SearchText == string.Empty)
                {
                    _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, _dashboardProjectsDisplay);
                    SearchBlankVisibility = Visibility.Collapsed;
                    NoProjectVisibility = Visibility.Visible;
                }
                else
                {
                    _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, _dashboardProjectsDisplay);
                    _dashboardProjectsDisplay.RemoveAll(project => !project.ProjectName.ToLower().Contains(SearchText.ToLower()));
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

        private ObservableCollection<DashboardProject>? _dashboardProjectsDisplay;
       
        public ObservableCollection<DashboardProject>? DashboardProjectsDisplay
        {
            get => _dashboardProjectsDisplay;
            set
            {
                _dashboardProjectsDisplay = value;
                NotifyOfPropertyChange(() => SearchText);
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
        public ProjectPickerViewModel(TranslationSource translationSource, DashboardProjectManager projectManager, ParatextProxy paratextProxy, 
            INavigationService navigationService, ILogger<ProjectPickerViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            Logger?.LogInformation("Project Picker constructor called.");
            //_windowManager = windowManager;
            _paratextProxy = paratextProxy;
            AlertVisibility = Visibility.Collapsed;
            _translationSource = translationSource;
            NoProjectVisibility = Visibility.Visible;
            SearchBlankVisibility = Visibility.Collapsed;
            IsParatextRunning = _paratextProxy.IsParatextRunning();
        }

        public async Task StartParatext()
        {
            await _paratextProxy.StartParatextAsync();

            IsParatextRunning = _paratextProxy.IsParatextRunning();
            IsParatextInstalled = _paratextProxy.IsParatextInstalled();
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {

            IsParatextRunning = _paratextProxy.IsParatextRunning();
            IsParatextInstalled = _paratextProxy.IsParatextInstalled();

            if (!IsParatextInstalled)
            {
               // await this.ShowMessageAsync("This is the title", "Some message");
            }
            var results = await ExecuteRequest(new GetDashboardProjectQuery(), CancellationToken.None);
            if (results.Success && results.HasData)
            {
                DashboardProjects = results.Data;
                _dashboardProjectsDisplay = new ObservableCollection<DashboardProject>();
                _dashboardProjectsDisplay = CopyDashboardProjectsToAnother(DashboardProjects, _dashboardProjectsDisplay);
            }

            await base.OnActivateAsync(cancellationToken);
        }
        #endregion Constructor

        #region Methods
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
           
            ParentViewModel!.ExtraData = project;
            ParentViewModel.Ok();
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
            }
            catch (Exception e)
            {
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
        }

        private static void SaveUserLanguage(string language)
        {
            Settings.Default.language_code = language;
            Settings.Default.Save();
        }

        #endregion  Methods

        public async Task HandleAsync(ParatextConnectedMessage message, CancellationToken cancellationToken)
        {
            Connected = message.Connected;
            await Task.CompletedTask;
        }

        public async Task HandleAsync(UserMessage message, CancellationToken cancellationToken)
        {
            ParatextUserName = message.User.FullName;
            await Task.CompletedTask;
        }
    }
}
