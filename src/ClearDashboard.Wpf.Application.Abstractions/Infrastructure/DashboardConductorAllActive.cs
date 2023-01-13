using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Infrastructure;

public abstract class DashboardConductorAllActive<T> : ApplicationConductorAllActive<T> where T : class
{
    protected DashboardConductorAllActive() : base()
    {
    }

    protected DashboardConductorAllActive(DashboardProjectManager? projectManager,
        INavigationService? navigationService, ILogger? logger, IEventAggregator? eventAggregator,
        IMediator? mediator, ILifetimeScope? lifetimeScope) :
        base(navigationService,logger, eventAggregator, mediator, lifetimeScope) 
    {
        ProjectManager = projectManager;
    }

    public DashboardProjectManager? ProjectManager { get; private set; }

    protected BindableCollection<T> MoveableItems => (BindableCollection<T>)Items;

}