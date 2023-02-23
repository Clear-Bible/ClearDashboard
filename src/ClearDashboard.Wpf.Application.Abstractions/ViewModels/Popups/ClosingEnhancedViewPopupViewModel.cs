using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Popups;

public class ClosingEnhancedViewPopupViewModel : SimpleMessagePopupViewModel
{

    public ClosingEnhancedViewPopupViewModel()
    {
        // required for designer support
    }

    public ClosingEnhancedViewPopupViewModel(INavigationService navigationService, 
        ILogger<ClosingEnhancedViewPopupViewModel> logger,
        DashboardProjectManager? projectManager, 
        IEventAggregator eventAggregator, 
        IMediator mediator,
        ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
        : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope,
            localizationService)
    {
        //no-op
    }

    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {

        return base.OnInitializeAsync(cancellationToken);
    }

    public new string Title 
    {
        get
        {
            switch (SimpleMessagePopupMode)
            {
                case SimpleMessagePopupMode.Delete:
                    return "Delete this EnhancedView?"; ;
                default:
                    return string.Empty;
            }
        }
    }

    public override string? OkLabel => LocalizationService!["Yes"];
    public override string? CancelLabel => LocalizationService!["No"];

}