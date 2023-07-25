using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Services;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Infrastructure;

public abstract class DashboardApplicationValidatingWorkflowStepViewModel<TParentViewModel,TEntity> : ValidatingWorkflowStepViewModel<TEntity>
where TParentViewModel : class
{
    protected DashboardProjectManager? ProjectManager { get; }
    protected ILocalizationService? LocalizationService { get; }

    public TParentViewModel? ParentViewModel => Parent as TParentViewModel;

    protected DashboardApplicationValidatingWorkflowStepViewModel()
    {
    }

    protected DashboardApplicationValidatingWorkflowStepViewModel(DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, IValidator<TEntity> validator, ILocalizationService localizationService) : 
        base(navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
    {
        ProjectManager = projectManager;
        LocalizationService = localizationService;
    }
}