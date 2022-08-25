using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Features.DashboardProjects;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Project;
using ClearDashboard.Wpf.ViewModels.Workflows.CreateNewProject;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;
using NewProjectDialogViewModel = ClearDashboard.Wpf.ViewModels.Project.NewProjectDialogViewModel;

namespace ClearDashboard.Wpf.ViewModels
{
    public class LandingViewModel : ApplicationScreen
    {
        #region   Member Variables

        protected IWindowManager _windowManager;

        #endregion

        #region Observable Objects



        public ObservableCollection<DashboardProject> DashboardProjects { get; set; } =
            new ObservableCollection<DashboardProject>();



        private Visibility _alertVisibility = Visibility.Hidden;

        public Visibility AlertVisibility
        {
            get => _alertVisibility;
            set
            {
                _alertVisibility = value;
                NotifyOfPropertyChange(() => AlertVisibility);
            }
        }


        #endregion

        #region Constructor

        /// <summary>
        /// Required for design-time support.
        /// </summary>
        public LandingViewModel()
        {

        }

        public LandingViewModel(IWindowManager windowManager, DashboardProjectManager projectManager, INavigationService navigationService, IEventAggregator eventAggregator, ILogger<LandingViewModel> logger)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            Logger.LogInformation("LandingViewModel constructor called.");
            _windowManager = windowManager;
        }



        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var results = await ExecuteRequest(new GetDashboardProjectQuery(), CancellationToken.None);
            if (results.Success)
            {
                DashboardProjects = results.Data;
            }
        }

        #endregion

        #region Methods

        public void AlertClose()
        {
            AlertVisibility = Visibility.Collapsed;
        }


        public void CreateNewProject()
        {
            if (CheckIfConnectedToParatext() == false)
            {
                return;
            }

            Logger.LogInformation("CreateNewProject called.");
            //NavigationService.NavigateToViewModel<CreateNewProjectWorkflowShellViewModel>();

            NavigationService.NavigateToViewModel<CreateNewProjectWorkflowShellViewModel>();
        }


        public async void NewProject()
        {
            if (CheckIfConnectedToParatext() == false)
            {
                return;
            }

            Logger.LogInformation("NewProject called.");

            if (ProjectManager.HasDashboardProject)
            {
                ProjectManager.CreateDashboardProject();
            }

            await ProjectManager.InvokeDialog<NewProjectDialogViewModel, WorkSpaceViewModel>(
                DashboardProjectManager.NewProjectDialogSettings, (Func<NewProjectDialogViewModel, Task<bool>>)Callback);

            //await ProjectManager.InvokeDialog<NewProjectDialogViewModel, ProjectWorkspaceViewModel>(
            //    DashboardProjectManager.NewProjectDialogSettings, (Func<NewProjectDialogViewModel, Task<bool>>)Callback);


            //await ProjectManager.InvokeDialog<NewProjectDialogViewModel, ProjectWorkspaceWithGridSplitterViewModel>(
            //    DashboardProjectManager.NewProjectDialogSettings, (Func<NewProjectDialogViewModel, Task<bool>>)Callback);
            // Define a callback method to create a new project if we
            // have a valid project name

            async Task<bool> Callback(NewProjectDialogViewModel viewModel)
            {
                if (viewModel.ProjectName != null)
                {
                    await ProjectManager.CreateNewProject(viewModel.ProjectName);
                    return true;
                }

                return false;
            }
        }

        public void ProjectWorkspace(DashboardProject project)
        {
            if (CheckIfConnectedToParatext() == false)
            {
                return;
            }

            ProjectManager.CurrentDashboardProject = project;
            //NavigationService.NavigateToViewModel<ProjectWorkspaceWithGridSplitterViewModel>();
            NavigationService.NavigateToViewModel<ProjectWorkspaceViewModel>();

        }

        private bool CheckIfConnectedToParatext()
        {
            if (ProjectManager.HasCurrentParatextProject ==false)
            {
                AlertVisibility = Visibility.Visible;
                return false;
            }
            return true;
        }

        public void DeleteProject(DashboardProject project)
        {
            FileInfo fi = new FileInfo(project.FullFilePath);

            try
            {
                DirectoryInfo di = new DirectoryInfo(fi.DirectoryName);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                di.Delete();

                DashboardProjects.Remove(project);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Workspace(DashboardProject project)
        {
            if (CheckIfConnectedToParatext() == false)
            {
                return;
            }

            //// TODO HACK TO READ IN PROJECT AS OBJECT
            string sTempFile = @"c:\temp\project.json";
            if (File.Exists(sTempFile) == false)
            {
                MessageBox.Show($"MISSING TEMP PROJECT FILE : {sTempFile}");
            }

            var jsonString = File.ReadAllText(@"c:\temp\project.json");
            project = JsonSerializer.Deserialize<DashboardProject>(jsonString);


            Logger.LogInformation("Workspace called.");
            ProjectManager.CurrentDashboardProject = project;




            NavigationService.NavigateToViewModel<WorkSpaceViewModel>();
        }

        public void Settings()
        {
            Logger.LogInformation("Settings called.");
            NavigationService.NavigateToViewModel<SettingsViewModel>();

        }
        public void AlignmentSample()
        {
            Logger.LogInformation("AlignmentSample called.");
            NavigationService.NavigateToViewModel<AlignmentSampleViewModel>();
        }
        #endregion // Methods
    }
}
