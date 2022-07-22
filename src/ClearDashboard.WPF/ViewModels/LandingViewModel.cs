using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DataAccessLayer.Features.DashboardProjects;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Popups;
using ClearDashboard.Wpf.ViewModels.Workflows.CreateNewProject;
using Microsoft.Extensions.Logging;
using MessageBox = System.Windows.Forms.MessageBox;
using System;


using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.ViewModels.Popups;

namespace ClearDashboard.Wpf.ViewModels
{
    public class LandingViewModel: ApplicationScreen
    {
        #region   Member Variables
        
        protected IWindowManager _windowManager;

        #endregion

        #region Observable Objects

       

        public ObservableCollection<DashboardProject> DashboardProjects { get; set; } =
            new ObservableCollection<DashboardProject>();

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
        { base.OnViewAttached(view, context);
        }

        protected  override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
           var results = await ExecuteRequest(new GetDashboardProjectQuery(), CancellationToken.None);
           if (results.Success)
           {
               DashboardProjects = results.Data;
           }
        }

        #endregion

        #region Methods

        public void CreateNewProject()
        {
            Logger.LogInformation("CreateNewProject called.");
            NavigationService.NavigateToViewModel<CreateNewProjectWorkflowShellViewModel>();
        }

        public async void NewProject()
        {
            Logger.LogInformation("NewProject called.");
            
            dynamic settings = new ExpandoObject();
            settings.WindowStyle = WindowStyle.ThreeDBorderWindow;
            settings.ShowInTaskbar = false;
            settings.Title = "Create New Project";
            settings.WindowState = WindowState.Normal;
            settings.ResizeMode = ResizeMode.NoResize;

            var newProjectPopupViewModel = IoC.Get<NewProjectDialogViewModel>();
            var created = await _windowManager.ShowDialogAsync(newProjectPopupViewModel, null, settings);

            //if (created.HasValue && created.Value)
            if (created)
            {
                var projectName = newProjectPopupViewModel.ProjectName;

                await ProjectManager.CreateNewProject(projectName);
                //NavigationService.NavigateToViewModel<NewProjectWorkflowShellViewModel>();
            }

        }

        public void AlignmentSample()
        {
            Logger.LogInformation("AlignmentSample called.");
            //NavigationService.NavigateToViewModel<CreateNewProjectWorkflowShellViewModel>();
        }

        public void Workspace(DashboardProject project)
        {
            if (project is null)
            {
                return;
            }

            // TODO HACK TO READ IN PROJECT AS OBJECT
            string sTempFile = @"c:\temp\project.json";
            if (File.Exists(sTempFile) == false)
            {
                MessageBox.Show($"MISSING TEMP PROJECT FILE : {sTempFile}");
            }

            var jsonString =File.ReadAllText(@"c:\temp\project.json");
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

        #endregion // Methods
    }
}
