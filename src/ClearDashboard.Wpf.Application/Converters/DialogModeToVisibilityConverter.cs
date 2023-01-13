using ClearDashboard.Wpf.Application.Infrastructure;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters;

public class DialogModeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return Visibility.Collapsed;
        }

        var mode = (DialogMode)value;

        return mode switch
        {
            DialogMode.Add => Visibility.Collapsed,
            DialogMode.Edit => Visibility.Visible,
            _ => Visibility.Collapsed
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}