using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    public class LandingViewModel: ApplicationScreen
    {
        #region   Member Variables
        
        private readonly INavigationService _navigationService;
        private readonly ProjectManager _projectManager;
        private readonly ILogger _logger;
        
        #endregion

        #region Observable Objects

        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public FlowDirection flowDirection
        {
            get => _flowDirection;
            set
            {
                _flowDirection = value;
                NotifyOfPropertyChange(() => flowDirection);
            }
        }

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

        public LandingViewModel(ProjectManager projectManager, INavigationService navigationService, ILogger<LandingViewModel> logger): base(navigationService, logger)
        {
            _logger = logger;
            _navigationService = navigationService;
            _projectManager = projectManager;

            flowDirection = _projectManager.CurrentLanguageFlowDirection;

            // get the clearsuite projects
            DashboardProjects = projectManager.LoadExistingProjects();

            _logger.LogError("LandingViewModel constructor called.");
        }

        protected override void OnViewAttached(object view, object context)
        { base.OnViewAttached(view, context);
        }

        #endregion



        #region Methods

        public void CreateNewProject()
        {
            _logger.LogInformation("CreateNewProject called.");
           _navigationService.NavigateToViewModel<CreateNewProjectsViewModel>();
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


            _logger.LogInformation("Workspace called."); 
            _projectManager.CurrentDashboardProject = project;
           _navigationService.NavigateToViewModel<WorkSpaceViewModel>();
        }

        public void Settings()
        {
            _logger.LogInformation("Settings called.");
            _navigationService.NavigateToViewModel<SettingsViewModel>();

        }

        #endregion // Methods
    }
}
