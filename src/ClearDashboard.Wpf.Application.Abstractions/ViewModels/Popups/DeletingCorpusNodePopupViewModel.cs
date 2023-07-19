using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Popups;

public class DeletingCorpusNodePopupViewModel : SimpleMessagePopupViewModel
{

    public DeletingCorpusNodePopupViewModel()
    {
        // required for designer support
    }

    public DeletingCorpusNodePopupViewModel(INavigationService navigationService, 
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
            return string.Empty;//LocalizationService!["Pds_DeletingCorpusNode"];
        }
    }

    public override string? OkLabel => LocalizationService!["Yes"];
    public override string? CancelLabel => LocalizationService!["No"];

    protected override string? CreateMessage()
    {
            return LocalizationService!["Pds_DeletingCorpusNode"];
    }
}