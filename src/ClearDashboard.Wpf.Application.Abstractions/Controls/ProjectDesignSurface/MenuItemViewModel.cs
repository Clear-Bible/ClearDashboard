using System.Collections.ObjectModel;
using System.Windows.Input;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.ViewModels.Menus;
using ClearDashboard.Wpf.Application.ViewModels.Project;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;

public abstract class MenuItemViewModel<TMenuItemViewModel> : PropertyChangedBase
{
    protected MenuItemViewModel()
    {
        MenuItems = new BindableCollection<TMenuItemViewModel>();
        Command = new CommandViewModel(Execute);
        Enabled = true;
    }
    private bool? _isChecked = false;
    public bool? IsChecked
    {
        get => _isChecked;
        set => Set(ref _isChecked, value);
    }

    private string? _iconKind;
    public string? IconKind
    {
        get => _iconKind;
        set => Set(ref _iconKind, value);
    }

    private bool? _isSeparator = false;
    public bool? IsSeparator
    {
        get => _isSeparator;
        set => Set(ref _isSeparator, value);
    }


    private string? _id;
    public string? Id
    {
        get => _id;
        set => Set(ref _id, value);
    }

    private bool  _enabled;
    public bool Enabled
    {
        get => _enabled;
        set => Set(ref _enabled, value);
    }


    private string? _header;
    public string? Header
    {
        get => _header;
        set => Set(ref _header, value);
    }

    private IProjectDesignSurfaceViewModel? _projectDesignSurfaceViewModel;
    public IProjectDesignSurfaceViewModel? ProjectDesignSurfaceViewModel
    {
        get => _projectDesignSurfaceViewModel;
        set => Set(ref _projectDesignSurfaceViewModel, value);
    }

    public BindableCollection<TMenuItemViewModel>? MenuItems { get; set; }

    public ICommand? Command { get; protected set; }

    protected abstract void Execute();
}