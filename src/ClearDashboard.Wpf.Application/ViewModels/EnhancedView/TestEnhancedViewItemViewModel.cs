using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public class TestEnhancedViewItemViewModel : EnhancedViewItemViewModel
{
    public string? Message { get; set; }

    public TestEnhancedViewItemViewModel(DashboardProjectManager? projectManager,
        INavigationService? navigationService, ILogger<TestEnhancedViewItemViewModel>? logger, IEventAggregator? eventAggregator,
        IMediator? mediator, ILifetimeScope? lifetimeScope) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {
        Title = "TestEnhancedViewItemViewModel";
        Message = "This message was created in the TestEnhancedViewItemViewModel constructor.";
    }

    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        return base.OnActivateAsync(cancellationToken);
    }
}