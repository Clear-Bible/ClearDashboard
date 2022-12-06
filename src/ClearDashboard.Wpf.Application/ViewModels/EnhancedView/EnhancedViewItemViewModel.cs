using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;

// ReSharper disable InconsistentNaming


namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public abstract class EnhancedViewItemViewModel : Screen
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

}