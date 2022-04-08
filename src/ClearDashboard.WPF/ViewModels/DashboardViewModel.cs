using ClearDashboard.Common.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using ClearDashboard.Wpf.Helpers;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;


namespace ClearDashboard.Wpf.ViewModels
{
    public class DashboardViewModel : PaneViewModel
    {
        #region Member Variables
        private readonly ILogger _logger;
        private readonly ProjectManager _projectManager;
        private bool _firstLoad;

        #endregion //Member Variables

        #region Public Properties

        public string ContentID { get; set; }
        public DashboardProject DashboardProject { get; set; }

        #endregion //Public Properties

        #region Observable Properties

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

        #endregion //Observable Properties



        #region Constructor

        public DashboardViewModel()
        {
            Initialize();
        }


        public DashboardViewModel(ILogger<DashboardViewModel> logger, ProjectManager projectManager)
        {
            _projectManager = projectManager;

            flowDirection = _projectManager.CurrentLanguageFlowDirection;
            _logger = logger;
            Initialize();

        }

        private void Initialize()
        {

            this.Title = "📐 DASHBOARD";
            this.ContentId = "DASHBOARD";

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
