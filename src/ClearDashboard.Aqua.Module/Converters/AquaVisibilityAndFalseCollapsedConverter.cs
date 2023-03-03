using System;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Aqua.Module.Converters
{
    public sealed class AquaVisibilityAndFalseCollapsedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (object value in values)
            {
                if ((value is Visibility) && (Visibility)value != Visibility.Visible)
                {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Visible;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
        }
    }
}
