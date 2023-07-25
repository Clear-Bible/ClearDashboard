using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Services;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public class TestEnhancedViewItemViewModel : EnhancedViewItemViewModel
{
    public string? Message { get; set; }

    public TestEnhancedViewItemViewModel(DashboardProjectManager? projectManager, IEnhancedViewManager enhancedViewManager,
        INavigationService? navigationService, ILogger<TestEnhancedViewItemViewModel>? logger, IEventAggregator? eventAggregator,
        IMediator? mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService) : base(projectManager, enhancedViewManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
    {
        Title = "TestEnhancedViewItemViewModel";
        Message = "This message was created in the TestEnhancedViewItemViewModel constructor.";
    }

    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        return base.OnActivateAsync(cancellationToken);
    }
}