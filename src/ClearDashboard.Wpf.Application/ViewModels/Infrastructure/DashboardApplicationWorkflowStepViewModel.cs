using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Wpf;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Infrastructure
{
    public abstract class DashboardApplicationWorkflowStepViewModel<TParentViewModel>: WorkflowStepViewModel
    where TParentViewModel : class
    {
        protected DashboardProjectManager? ProjectManager { get; }
        protected TParentViewModel? ParentViewModel => Parent as TParentViewModel;

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
