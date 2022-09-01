using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Wpf;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public abstract class ApplicationWorkflowStepViewModel: WorkflowStepViewModel
    {
        protected DashboardProjectManager? ProjectManager { get; }
        
        protected ApplicationWorkflowStepViewModel()
        {
        }

        protected ApplicationWorkflowStepViewModel (DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope)
            : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;
        }
        
    }
}
