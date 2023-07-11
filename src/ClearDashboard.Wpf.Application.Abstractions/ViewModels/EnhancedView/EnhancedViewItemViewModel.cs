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
using ClearDashboard.Wpf.Application.Converters;


namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;



public abstract class EnhancedViewItemViewModel : DashboardApplicationScreen
{
    protected ILocalizationService LocalizationService => _localizationService;

    private Brush _borderColor = Brushes.Blue;

    protected IEnhancedViewManager EnhancedViewManager => _enhancedViewManager;

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

    private bool _showEditButton;
    public bool ShowEditButton
    {
        get => _showEditButton;
        set => Set(ref _showEditButton, value);
    }

    private bool _enableEditMode;
    public bool EnableEditMode
    {
        get => _enableEditMode;
        set => Set(ref _enableEditMode, value);
    }

    public async void ToggleEditMode()
    {
        EnableEditMode = !EnableEditMode;

        if (EnableEditMode)
        {
            await GetEditorData();
        }
        
    }

    protected virtual async Task GetEditorData()
    {
        await Task.CompletedTask;
    }

    private string _editModeButtonLabel;

    public string EditModeButtonLabel
    {
        get => _editModeButtonLabel1;
        set => Set(ref _editModeButtonLabel1, value);
    }


    // public EnhancedViewModel ParentViewModel => (EnhancedViewModel)Parent;

    public bool FetchingData
    {
        get => _fetchData;
        set
        {
            Set(ref _fetchData, value);
            NotifyOfPropertyChange(nameof(DisableDeleteButton));
        }
    }

    public bool DisableDeleteButton => !FetchingData;

    private EnhancedViewItemMetadatum? _enhancedViewItemMetadatum;
    private bool _fetchData;
    private readonly ILocalizationService _localizationService;
    private readonly IEnhancedViewManager _enhancedViewManager;
    private string _editModeButtonLabel1;
    private EditMode _editMode;


    public EnhancedViewItemMetadatum? EnhancedViewItemMetadatum
    {
        get => _enhancedViewItemMetadatum;
        set => Set(ref _enhancedViewItemMetadatum, value);
    }

    //public virtual Task GetData(EnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken)
    //{
    //    return Task.CompletedTask;
    //}

    public virtual Task GetData(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public EditMode EditMode
    {
        get => _editMode;
        set => Set(ref _editMode, value);
    }

    protected EnhancedViewItemViewModel(DashboardProjectManager? projectManager, IEnhancedViewManager enhancedViewManager,
        INavigationService? navigationService, ILogger? logger, IEventAggregator? eventAggregator,
        IMediator? mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService, EditMode editMode = EditMode.MainViewOnly) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
    {
        _localizationService = localizationService;
        _enhancedViewManager = enhancedViewManager;
        EditMode = editMode;

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