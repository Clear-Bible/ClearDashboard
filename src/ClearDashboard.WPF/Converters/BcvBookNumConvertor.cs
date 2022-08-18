using System;
using System.Globalization;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Converters
{
    /// <summary>
    /// Used to convert between zero and one based systems used by the dropdowns
    /// </summary>
    public class BcvBookNumConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value) - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value) + 1;
        }
    }
}
