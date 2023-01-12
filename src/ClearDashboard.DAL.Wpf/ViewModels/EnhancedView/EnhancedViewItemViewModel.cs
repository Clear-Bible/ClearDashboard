using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using ClearDashboard.Wpf.Application.Services;


namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public abstract class EnhancedViewItemViewModel : DashboardApplicationScreen
{
    protected ILocalizationService LocalizationService { get; }

    private Brush _borderColor = Brushes.Blue;
    public Brush BorderColor
    {
        get => _borderColor;
        set => Set(ref _borderColor, value);
    }

   // public EnhancedViewModel ParentViewModel => (EnhancedViewModel)Parent;

    public virtual Task GetData(EnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected EnhancedViewItemViewModel(DashboardProjectManager? projectManager,
        INavigationService? navigationService, ILogger? logger, IEventAggregator? eventAggregator,
        IMediator? mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {
        LocalizationService = localizationService;
        // no-op
    }

}