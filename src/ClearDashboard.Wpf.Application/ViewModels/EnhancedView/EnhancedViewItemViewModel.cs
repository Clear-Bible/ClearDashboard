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




namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public abstract class EnhancedViewItemViewModel : DashboardApplicationScreen
{
   
    private Brush _borderColor = Brushes.Blue;
    public Brush BorderColor
    {
        get => _borderColor;
        set => Set(ref _borderColor, value);
    }

    public EnhancedViewModel ParentViewModel => (EnhancedViewModel)Parent;

    public virtual Task GetData(EnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected EnhancedViewItemViewModel(DashboardProjectManager? projectManager,
        INavigationService? navigationService, ILogger? logger, IEventAggregator? eventAggregator,
        IMediator? mediator, ILifetimeScope? lifetimeScope) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {
        // no-op
    }

}