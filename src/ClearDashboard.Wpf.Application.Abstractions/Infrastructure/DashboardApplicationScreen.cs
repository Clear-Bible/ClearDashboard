using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Services;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Infrastructure
{
    public abstract class DashboardApplicationScreen : ApplicationScreen
    {
        protected DashboardApplicationScreen()
        {
          
        }

        protected DashboardApplicationScreen(DashboardProjectManager? projectManager,
            INavigationService? navigationService, ILogger? logger, IEventAggregator? eventAggregator,
            IMediator? mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService) : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;
            LocalizationService = localizationService;
        }

        public DashboardProjectManager? ProjectManager { get; private set; }

        protected ILocalizationService? LocalizationService { get; }

    }
}
