using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    /// <summary>
    /// Converts a logical "IsVertical" bool to an <see cref="Orientation"/>, where <b>false</b> = <see cref="Orientation.Horizontal"/>
    /// and <b>true</b> = <see cref="Orientation.Vertical"/>.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(HorizontalAlignment))]
    public class BoolOrientationConverter : IValueConverter   
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? Orientation.Vertical : Orientation.Horizontal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Orientation)value == Orientation.Vertical;
        }
    }
}
