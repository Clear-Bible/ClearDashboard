using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Aqua.Module.Converters
{

  [ValueConversion(typeof(Boolean), typeof(Visibility))]
    public sealed class BoolTrueToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool IsCollapsed = value == null ? false : (bool)value;
            return IsCollapsed ? Visibility.Collapsed : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
