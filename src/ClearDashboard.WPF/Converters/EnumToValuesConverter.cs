using System;
using System.Windows;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace ClearDashboard.Wpf.Converters;

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