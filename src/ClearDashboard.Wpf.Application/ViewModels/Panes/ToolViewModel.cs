using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Services;
using ClearDashboard.DataAccessLayer.Features;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Panes
{
    /// <summary>
    /// 
    /// </summary>
    public class ToolViewModel : PaneViewModel, IAvalonDockWindow
    {
        #region Member Variables
        private bool _isVisible = true;
        private bool _canClose = false;
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

        public ToolViewModel(INavigationService navigationService, ILogger logger,
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator, IMediator mediator,
            ILifetimeScope lifetimeScope, ILocalizationService localizationService) : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope, localizationService)
        {

        }
        #endregion //Constructor

        #region Methods


        #endregion // Methods

    }
}
