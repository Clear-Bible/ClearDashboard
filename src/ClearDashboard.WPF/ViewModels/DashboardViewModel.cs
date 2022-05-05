using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ClearDashboard.DataAccessLayer.Models;


namespace ClearDashboard.Wpf.ViewModels
{
    public class DashboardViewModel : PaneViewModel
    {
        #region Member Variables
      
        private bool _firstLoad;

        #endregion //Member Variables

        #region Public Properties

        public string ContentID { get; set; }
        public DashboardProject DashboardProject { get; set; }

        #endregion //Public Properties

        #region Observable Properties

       
        #endregion //Observable Properties



        #region Constructor

        public DashboardViewModel()
        {
            Initialize();
        }


        public DashboardViewModel(INavigationService navigationService, ILogger<DashboardViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            Initialize();
        }

        private void Initialize()
        {

            this.Title = "📐 DASHBOARD";
            this.ContentId = "DASHBOARD";

            if (!_firstLoad)
            {
                Logger.LogInformation("Not the first load");
            }
        }

        #endregion //Constructor

        #region Methods


        #endregion // Methods


    }
}
