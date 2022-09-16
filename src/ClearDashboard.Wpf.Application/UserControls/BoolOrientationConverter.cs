using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.UserControls
{
    public class BoolOrientationConverter : IValueConverter   
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? Orientation.Vertical : Orientation.Horizontal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Orientation)value == Orientation.Vertical;
        }
    }
}
