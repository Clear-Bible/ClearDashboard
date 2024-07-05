using System;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters;

[ValueConversion(typeof(System.Enum), typeof(bool))]
public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        return value.Equals(parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        if (value.Equals(false))
            return DependencyProperty.UnsetValue;
        else
            return parameter;
    }
}