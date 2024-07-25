using System;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters;

[ValueConversion(typeof(Enum), typeof(Array))]
public class EnumToValuesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        return Enum.GetNames(value.GetType());
    }

    public object ConvertBack(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}