using System;
using System.Globalization;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    public class EnumMatchToBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType,
            object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            var checkValue = value.ToString();
            var targetValue = parameter.ToString();
            return checkValue != null && checkValue.Equals(targetValue,
                StringComparison.InvariantCultureIgnoreCase);
        }

        public object? ConvertBack(object? value, Type targetType,
            object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            var useValue = (bool)value;

            if (!useValue)
            {
                return null;
            }

            var targetValue = parameter.ToString();
           
            return targetValue != null ? Enum.Parse(targetType, targetValue) : null;
        }
    }
}
