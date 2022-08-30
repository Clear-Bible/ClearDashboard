using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Wpf;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Infrastructure
{
    public abstract class DashboardApplicationScreen : ApplicationScreen
    {
        protected DashboardApplicationScreen()
        {

        }

        protected DashboardApplicationScreen(DashboardProjectManager projectManager,
            INavigationService? navigationService, ILogger? logger, IEventAggregator? eventAggregator,
            IMediator mediator) : base(navigationService, logger, eventAggregator, mediator)
        {
            ProjectManager = projectManager;
        }

        public DashboardProjectManager? ProjectManager { get; private set; }

    }
}
