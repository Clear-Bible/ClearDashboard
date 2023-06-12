using System;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Aqua.Module.Converters
{
    public sealed class BooleanAndVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (object value in values)
            {
                if ((value is bool) && (bool)value == false)
                {
                    return Visibility.Hidden;
                }
            }
            return Visibility.Visible;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("BooleanAndVisibilityConverter is a OneWay converter.");
        }
    }
}
