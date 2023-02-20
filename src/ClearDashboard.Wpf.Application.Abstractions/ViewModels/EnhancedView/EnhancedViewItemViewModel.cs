using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClearApplicationFoundation.Framework.Input;


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

    private bool _hasFocus;
    public bool HasFocus
    {
        get => _hasFocus;
        set => Set(ref _hasFocus, value);
    }

    // public EnhancedViewModel ParentViewModel => (EnhancedViewModel)Parent;

    public virtual Task GetData(EnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected EnhancedViewItemViewModel(DashboardProjectManager? projectManager,
        INavigationService? navigationService, ILogger? logger, IEventAggregator? eventAggregator,
        IMediator? mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
    {
        LocalizationService = localizationService;
        // no-op
    }

    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        //HasFocus = true;
        return base.OnActivateAsync(cancellationToken);
    }

    protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        //HasFocus = false;
        return base.OnDeactivateAsync(close, cancellationToken);
    }

    public void SetFocus(object sender, MouseButtonEventArgs e)
    {
        var element = (UIElement)sender;
        EnhancedFocusScope.SetFocusOnActiveElementInScope(element);
    }
}