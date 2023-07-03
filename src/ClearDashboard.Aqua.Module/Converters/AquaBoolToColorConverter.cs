using ClearDashboard.DAL.CQRS;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ClearDashboard.Aqua.Module.Converters
{
    public sealed class AquaBoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is bool && parameter is string)
            {
                var parameters = ((string) parameter).Split(',');
                if (parameters.Length == 2)
                {
                    bool boolResult;
                    if (bool.TryParse(parameters[0], out boolResult))
                    {
                        if (((bool)value) == boolResult)
                            return new BrushConverter().ConvertFromString(parameters[1]) as SolidColorBrush ?? Brushes.Transparent;
                    }
                }

            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
