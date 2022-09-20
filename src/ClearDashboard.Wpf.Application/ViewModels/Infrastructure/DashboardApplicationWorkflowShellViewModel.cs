using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Wpf;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Infrastructure
{
    public class DashboardApplicationWorkflowShellViewModel : WorkflowShellViewModel
    {

        public DashboardApplicationWorkflowShellViewModel()
        {
            WorkflowMode = WorkflowMode.Add;
        }

        public DashboardApplicationWorkflowShellViewModel(DashboardProjectManager? projectManager, 
            INavigationService navigationService,
            ILogger logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) : base(
            navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;
            WorkflowMode = WorkflowMode.Add;
        }

        public DashboardProjectManager? ProjectManager { get; private set; }

        public WorkflowMode WorkflowMode { get; set; }
    }
}
