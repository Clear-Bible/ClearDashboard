using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Wpf.Infrastructure
{
    public abstract class DashboardApplicationWorkflowStepViewModel<TParentViewModel>: WorkflowStepViewModel
    where TParentViewModel : class
    {
        protected DashboardProjectManager? ProjectManager { get; }
        public TParentViewModel? ParentViewModel => Parent as TParentViewModel;

        protected DashboardApplicationWorkflowStepViewModel()
        {
        }

        protected DashboardApplicationWorkflowStepViewModel (DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope)
            : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;
        }
        
    }
}
