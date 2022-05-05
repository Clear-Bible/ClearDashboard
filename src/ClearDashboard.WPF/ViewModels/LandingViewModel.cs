using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DataAccessLayer.Features.DashboardProjects;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Workflows.NewProject;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    public class LandingViewModel: ApplicationScreen
    {
        #region   Member Variables
        
     
        
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

        public LandingViewModel(DashboardProjectManager projectManager, INavigationService navigationService, IEventAggregator eventAggregator, ILogger<LandingViewModel> logger)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            Logger.LogInformation("LandingViewModel constructor called.");
        }

        protected override void OnViewAttached(object view, object context)
        { base.OnViewAttached(view, context);
        }

        protected  override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
           var results = await ExecuteCommand(new GetDashboardProjectsCommand(), CancellationToken.None);
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

        public void Workspace(DashboardProject project)
        {
            if (project is null)
            {
                return;
            }

            // TODO HACK TO READ IN PROJECT AS OBJECT
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
