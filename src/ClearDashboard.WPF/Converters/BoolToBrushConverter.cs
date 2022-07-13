using System;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var input = (bool)value;
            switch (input)
            {
                case true:
                    return Application.Current.TryFindResource("SecondaryHueMidBrush");
                default:
                    return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
