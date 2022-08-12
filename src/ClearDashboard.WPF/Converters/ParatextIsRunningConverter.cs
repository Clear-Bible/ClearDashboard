using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Converters
{
    public class ParatextIsRunningConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Process.GetProcessesByName("Paratext").Length > 0 ? Visibility.Visible : Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Process.GetProcessesByName("Paratext").Length > 0 ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
