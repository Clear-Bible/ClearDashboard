﻿using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Wpf.Infrastructure;

public abstract class DashboardApplicationValidatingWorkflowStepViewModel<TParentViewModel,TEntity> : ValidatingWorkflowStepViewModel<TEntity>
where TParentViewModel : class
{
    protected DashboardProjectManager? ProjectManager { get; }
    public TParentViewModel? ParentViewModel => Parent as TParentViewModel;
    protected DashboardApplicationValidatingWorkflowStepViewModel() {}
    protected DashboardApplicationValidatingWorkflowStepViewModel(DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, IValidator<TEntity> validator) : 
        base(navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
    {
        ProjectManager = projectManager;
    }
}