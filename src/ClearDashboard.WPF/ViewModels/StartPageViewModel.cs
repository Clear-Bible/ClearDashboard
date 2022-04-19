using ClearDashboard.Common;
using ClearDashboard.Common.Models;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class StartPageViewModel : PaneViewModel
    {
        #region Member Variables

        private readonly ILogger _logger;
        private readonly ProjectManager _projectManager;

        #endregion //Member Variables

        #region Public Properties

        #endregion //Public Properties

        #region Observable Properties

        #endregion //Observable Properties

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

        #region Constructor

        public StartPageViewModel()
        {

        }

        public StartPageViewModel(INavigationService navigationService, ILogger<StartPageViewModel> logger, ProjectManager projectManager)
        {
            this.Title = "⌂ START PAGE";
            this.ContentId = "{StartPage_ContentId}";

            _logger = logger;
            _projectManager = projectManager;

            flowDirection = _projectManager.CurrentLanguageFlowDirection;

        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods

    }
}
