using System;
using System.Globalization;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    public class InvertedEnumMatchToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string checkValue = value.ToString();
            string targetValue = parameter.ToString();

            if(checkValue.Equals(targetValue,
                StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            bool useValue = (bool)value;
            string targetValue = parameter.ToString();
            if (useValue)
                return Enum.Parse(targetType, targetValue);

            return null;
        }
    }
}
