using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Converters
{
    public class IsParatextRunningBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Process.GetProcessesByName("Paratext").Length > 0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Process.GetProcessesByName("Paratext").Length <= 0;
        }
    }
}
