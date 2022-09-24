using System;
using System.Globalization;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters;

[ValueConversion(typeof(Boolean), typeof(Boolean))]
public sealed class BooleanInversionConverter : IValueConverter
{
    public  object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null && value is bool)
        {
            return !((bool)value);
        }

        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}