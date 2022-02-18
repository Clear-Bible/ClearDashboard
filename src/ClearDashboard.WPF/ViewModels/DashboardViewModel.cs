using ClearDashboard.Common.Models;
using System;
using System.Diagnostics;
using System.Windows;
using Serilog;

namespace ClearDashboard.Wpf.ViewModels
{
    public class DashboardViewModel : PaneViewModel
    {
        #region Member Variables
        private readonly ILogger _logger;
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
            this.Title = "📐 DASHBOARD";
            this.ContentId = "Dashboard_ContentId";

            if (Application.Current is ClearDashboard.Wpf.App)
            {
                _logger = (Application.Current as ClearDashboard.Wpf.App)._logger;
                DashboardProject = (Application.Current as ClearDashboard.Wpf.App).SelectedDashboardProject;
            }
        }

        internal void Init()
        {
            if (!_firstLoad)
            {
                Debug.WriteLine("Not the first load");
            }
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods


    }
}
