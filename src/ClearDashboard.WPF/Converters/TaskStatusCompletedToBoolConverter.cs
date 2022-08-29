using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ClearDashboard.Wpf.ViewModels;


namespace ClearDashboard.Wpf.Converters
{
    public class TaskStatusCompletedToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusEnum status)
            {
                if (status == StatusEnum.Completed)
                {
                    return false;
                }
            }
            return true;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

