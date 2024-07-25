using System;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace ClearDashboard.Wpf.Application.Converters;

[ValueConversion(typeof(string), typeof(PackIconKind))]
public class StringToMaterialDesignKindConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        var kind = (string)value;
        return Enum.Parse<PackIconKind>(kind);
    }

    public object ConvertBack(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}