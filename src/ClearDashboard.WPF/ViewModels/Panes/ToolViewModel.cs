﻿using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Panes
{
    /// <summary>
    /// 
    /// </summary>
    public class ToolViewModel : PaneViewModel, IAvalonDockWindow
    {
        #region Member Variables
        private bool _isVisible = true;
        private bool _canClose = true;
        #endregion //Member Variables

        #region Public Properties
        public string Name { get; private set; }


        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    NotifyOfPropertyChange(() => IsVisible);
                }
            }
        }

        public bool CanClose
        {
            get => _canClose;
            set
            {
                _canClose = value;
                NotifyOfPropertyChange(() => CanClose);
            }
        }


        #endregion //Public Properties

        #region Observable Properties

        #endregion //Observable Properties

        #region Constructor
        public ToolViewModel()
        {

        }

        public ToolViewModel(INavigationService navigationService, ILogger logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator) : base(navigationService, logger, projectManager, eventAggregator)
        {

        }
        #endregion //Constructor

        #region Methods


        #endregion // Methods

    }
}
