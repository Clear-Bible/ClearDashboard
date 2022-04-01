using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.Wpf.Helpers;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using ClearDashboard.DataAccessLayer.Utility;
using ClearDashboard.DataAccessLayer;

namespace ClearDashboard.Wpf.ViewModels
{
    public class LandingViewModel: ApplicationScreen
    {
        #region   Member Variables

        

        private readonly INavigationService _navigationService;
        private readonly ProjectManager _projectManager;

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

        public LandingViewModel(ProjectManager projectManager, INavigationService navigationService, ILogger<LandingViewModel> logger): base(navigationService, logger)
        {
           
            _navigationService = navigationService;
            _projectManager = projectManager;

          
            // get the clearsuite projects
            DashboardProjects = projectManager.LoadExistingProjects();

            Logger.LogInformation("LandingViewModel constructor called.");
        }

        protected override void OnViewAttached(object view, object context)
        { base.OnViewAttached(view, context);
        }

        #endregion



        #region Methods

        public void CreateNewProject()
        {
           Logger.LogInformation("CreateNewProject called.");
           _navigationService.NavigateToViewModel<CreateNewProjectsViewModel>();
        }

        public void Workspace()
        {
           Logger.LogInformation("Workspace called.");
           _navigationService.NavigateToViewModel<WorkSpaceViewModel>();
        }

        public void Settings()
        {
            Logger.LogInformation("Settings called.");
            _navigationService.NavigateToViewModel<SettingsViewModel>();

        }

        #endregion // Methods
    }
}
