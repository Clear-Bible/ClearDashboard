using System;
using System.Globalization;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    [ValueConversion(typeof(string), typeof(bool))]
    public class StringToBool : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() == "True")
            {
                return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}