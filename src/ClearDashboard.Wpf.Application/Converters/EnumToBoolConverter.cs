using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    [ValueConversion(typeof(System.Enum), typeof(bool))]
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString()?.Equals(parameter.ToString()) ?? false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool || parameter is not string || !(bool)value)
                return DependencyProperty.UnsetValue;

            try
            {
                return Enum.Parse(targetType, (string) parameter);
            }
            catch (Exception)
            {
                return DependencyProperty.UnsetValue;   
            }
        }
    }
}
