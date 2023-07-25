using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Services;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Collaboration
{
    public class CollabManagerViewModel : DashboardApplicationScreen
    {
        private readonly ILogger<ProjectSetupViewModel> _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILocalizationService _localizationService;

        #region Member Variables   

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        #endregion //Observable Properties


        #region Constructor


        public CollabManagerViewModel()
        {
            // no-op
        }

        public CollabManagerViewModel(CollaborationManager collaborationManager, 
            DashboardProjectManager projectManager,
            INavigationService navigationService, 
            ILogger<ProjectSetupViewModel> logger, 
            IEventAggregator eventAggregator,
            IMediator mediator, 
            ILifetimeScope? lifetimeScope,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            _localizationService = localizationService;
        }

        #endregion //Constructor


        #region Methods

        #endregion // Methods


    }
}
