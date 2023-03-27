using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    [ValueConversion(typeof(System.String), typeof(bool))]
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return ((string) value)?.Equals(((string) parameter) ?? throw new Exception()) ?? false;
            }
            catch (Exception) //if cast exception.
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool || parameter is not string || !(bool)value)
                return Binding.DoNothing;

            try
            {
                return parameter;
            }
            catch (Exception)
            {
                return Binding.DoNothing;   
            }
        }
    }
}
