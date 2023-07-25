using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Services;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Infrastructure
{
    public abstract class DashboardApplicationWorkflowStepViewModel<TParentViewModel>: WorkflowStepViewModel
    where TParentViewModel : class
    {
        protected DashboardProjectManager? ProjectManager { get; }
        protected ILocalizationService? LocalizationService { get; }
        public TParentViewModel? ParentViewModel => Parent as TParentViewModel;

        protected DashboardApplicationWorkflowStepViewModel()
        {
           
        }

        protected DashboardApplicationWorkflowStepViewModel (DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;
            LocalizationService = localizationService;
        }
        
    }
}
