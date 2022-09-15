using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.UserControls
{
    public class BoolHorizontalAlignmentConverter : IValueConverter   
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? HorizontalAlignment.Center : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (HorizontalAlignment)value == HorizontalAlignment.Center;
        }
    }
}
