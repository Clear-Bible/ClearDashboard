using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Features.DashboardProjects;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Strings;
using ClearDashboard.Wpf.Application.ViewModels.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class ProjectPickerViewModel : ApplicationWorkflowStepViewModel
    {
        #region Member Variables
        //protected IWindowManager? _windowManager;
        private readonly TranslationSource? _translationSource;
        #endregion

        #region Observable Objects
        public ObservableCollection<DashboardProject>? DashboardProjects { get; set; } =
            new ObservableCollection<DashboardProject>();

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

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                NotifyOfPropertyChange(() => SearchText);
            }
        }
        #endregion

        #region Constructor
        public ProjectPickerViewModel(DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ProjectPickerViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource) 
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            Logger?.LogInformation("Project Picker constructor called.");
            //_windowManager = windowManager;
            _translationSource = translationSource;
            AlertVisibility = Visibility.Collapsed;
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var results = await ExecuteRequest(new GetDashboardProjectQuery(), CancellationToken.None);
            if (results.Success && results.HasData)
            {
                DashboardProjects = results.Data;
            }

            await base.OnActivateAsync(cancellationToken);
        }
        #endregion Constructor

        #region Methods
        public void AlertClose()
        {
            AlertVisibility = Visibility.Collapsed;
        }


        //public void CreateNewProject()
        //{
        //    if (CheckIfConnectedToParatext() == false)
        //    {
        //        return;
        //    }

        //    Logger.LogInformation("CreateNewProject called.");
        //    //NavigationService.NavigateToViewModel<CreateNewProjectWorkflowShellViewModel>();

        //    NavigationService.NavigateToViewModel<CreateNewProjectWorkflowShellViewModel>();
        //}


        //public async void NewProject()
        //{
        //    if (CheckIfConnectedToParatext() == false)
        //    {
        //        return;
        //    }

        //    Logger.LogInformation("NewProject called.");

        //    if (ProjectManager.HasDashboardProject)
        //    {
        //        ProjectManager.CreateDashboardProject();
        //    }

        //    await ProjectManager.InvokeDialog<NewProjectDialogViewModel, WorkSpaceViewModel>(
        //        DashboardProjectManager.NewProjectDialogSettings, (Func<NewProjectDialogViewModel, Task<bool>>)Callback);

        //    //await ProjectManager.InvokeDialog<NewProjectDialogViewModel, ProjectWorkspaceViewModel>(
        //    //    DashboardProjectManager.NewProjectDialogSettings, (Func<NewProjectDialogViewModel, Task<bool>>)Callback);


        //    //await ProjectManager.InvokeDialog<NewProjectDialogViewModel, ProjectWorkspaceWithGridSplitterViewModel>(
        //    //    DashboardProjectManager.NewProjectDialogSettings, (Func<NewProjectDialogViewModel, Task<bool>>)Callback);
        //    // Define a callback method to create a new project if we
        //    // have a valid project name

        //    async Task<bool> Callback(NewProjectDialogViewModel viewModel)
        //    {
        //        if (viewModel.ProjectName != null)
        //        {
        //            await ProjectManager.CreateNewProject(viewModel.ProjectName);
        //            return true;
        //        }

        //        return false;
        //    }
        //}

        public void NavigateToMainViewModel(DashboardProject project)
        {
            if (CheckIfConnectedToParatext() == false)
            {
                return;
            }

            ProjectManager!.CurrentDashboardProject = project;
            var startupDialogViewModel = Parent as StartupDialogViewModel;
            startupDialogViewModel!.ExtraData = project;
            startupDialogViewModel.Ok();
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
            var fi = new FileInfo(project.FullFilePath ?? throw new InvalidOperationException("Project full file path is null."));

            try
            {
                var di = new DirectoryInfo(fi.DirectoryName ?? throw new InvalidOperationException("File directory name is null."));

                foreach (var file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (var dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                di.Delete();

                DashboardProjects.Remove(project);
            }
            catch (Exception e)
            {
                Logger?.LogError(e, "An unexpected error occurred while deleting a project.");
            }
        }

        //public void Workspace(DashboardProject project)
        //{
        //    if (CheckIfConnectedToParatext() == false)
        //    {
        //        return;
        //    }

        //    //// TODO HACK TO READ IN PROJECT AS OBJECT
        //    string sTempFile = @"c:\temp\project.json";
        //    if (File.Exists(sTempFile) == false)
        //    {
        //        MessageBox.Show($"MISSING TEMP PROJECT FILE : {sTempFile}");
        //    }

        //    var jsonString = File.ReadAllText(@"c:\temp\project.json");
        //    project = JsonSerializer.Deserialize<DashboardProject>(jsonString);


        //    Logger.LogInformation("Workspace called.");
        //    ProjectManager.CurrentDashboardProject = project;




        //    NavigationService.NavigateToViewModel<WorkSpaceViewModel>();
        //}

        //public void Settings()
        //{
        //    Logger.LogInformation("Settings called.");
        //    NavigationService.NavigateToViewModel<SettingsViewModel>();

        //}
        //public void AlignmentSample()
        //{
        //    Logger.LogInformation("AlignmentSample called.");
        //    NavigationService.NavigateToViewModel<AlignmentSampleViewModel>();
        //}

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

        private static void SaveUserLanguage(string language)
        {
            Settings.Default.language_code = language;
            Settings.Default.Save();
        }

        #endregion  Methods
    }
}
