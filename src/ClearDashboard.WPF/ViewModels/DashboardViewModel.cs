using ClearDashboard.Common.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using ClearDashboard.Wpf.Helpers;
using Caliburn.Micro;


namespace ClearDashboard.Wpf.ViewModels
{
    public class DashboardViewModel : PaneViewModel
    {
        #region Member Variables
        private readonly ILog _logger;
        private bool _firstLoad;

        #endregion //Member Variables

        #region Public Properties

        public string ContentID { get; set; }
        public DashboardProject DashboardProject { get; set; }

        #endregion //Public Properties

        #region Observable Properties


        #endregion //Observable Properties



        #region Constructor

        public DashboardViewModel(ILog logger)
        {
            this.Title = "📐 DASHBOARD";
            this.ContentId = "DASHBOARD";

            _logger = logger;


            if (Application.Current is ClearDashboard.Wpf.App)
            {
                //_logger = (Application.Current as ClearDashboard.Wpf.App)._logger;
                //DashboardProject = (Application.Current as ClearDashboard.Wpf.App).SelectedDashboardProject;
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
