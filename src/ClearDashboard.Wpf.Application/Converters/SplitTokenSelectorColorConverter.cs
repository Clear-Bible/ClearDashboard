using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Wpf.Application.Converters;

public class TokenTypeToWidthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var tokenType = value as string;
        return tokenType == TokenTypes.Circumfix ? parameter : 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}

public class TokenTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var tokenType = value as string;
        return tokenType == TokenTypes.Circumfix ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}
public class SplitTokenSelectorColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isSelected = value != null && (bool)value;
        var color = (Color)App.Current.FindResource("SecondaryHueLight");
        return isSelected ? new SolidColorBrush(color) : new SolidColorBrush(Colors.LightGray);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}