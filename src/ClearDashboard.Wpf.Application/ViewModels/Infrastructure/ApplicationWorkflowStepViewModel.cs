using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Wpf;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Infrastructure
{
    public abstract class ApplicationValidatingWorkflowStepViewModel<T> : ValidatingWorkflowStepViewModel<T>
    {
        protected DashboardProjectManager? ProjectManager { get; }
        protected ApplicationValidatingWorkflowStepViewModel(DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, IValidator<T> validator) : 
            base(navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
        {
            ProjectManager = projectManager;
        }
    }

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
