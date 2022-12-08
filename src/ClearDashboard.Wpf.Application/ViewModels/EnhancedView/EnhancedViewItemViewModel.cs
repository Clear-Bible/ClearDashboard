using System.Windows;
using System.Windows.Media;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming


namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public abstract class EnhancedViewItemViewModel : DashboardApplicationScreen
{
   
    protected Visibility _visibility = Visibility.Collapsed;
    public Visibility Visibility
    {
        get => _visibility;
        set => Set(ref _visibility, value);
    }


    protected Brush _borderColor = Brushes.Blue;
    public Brush BorderColor
    {
        get => _borderColor;
        set => Set(ref _borderColor, value);
    }


    protected string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => Set(ref _title, value);
    }

    public EnhancedViewModel ParentViewModel => (EnhancedViewModel)Parent;

    protected EnhancedViewItemViewModel(DashboardProjectManager? projectManager,
        INavigationService? navigationService, ILogger logger, IEventAggregator? eventAggregator,
        IMediator? mediator, ILifetimeScope? lifetimeScope) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {
        
    }

}