using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Wpf.Infrastructure
{
    public abstract class DashboardApplicationScreen : ApplicationScreen
    {
        protected DashboardApplicationScreen() : base()
        {
        }

        protected DashboardApplicationScreen(DashboardProjectManager? projectManager,
            INavigationService? navigationService, ILogger? logger, IEventAggregator? eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope) : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;
        }

        public DashboardProjectManager? ProjectManager { get; private set; }

    }
}
