using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    /// <summary>
    /// Converts a logical "IsCentered" bool to a <see cref="HorizontalAlignment"/>, where <b>false</b> = <see cref="HorizontalAlignment.Left"/>
    /// and <b>true</b> = <see cref="HorizontalAlignment.Center"/>.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(HorizontalAlignment))]
    public class BoolHorizontalAlignmentConverter : IValueConverter   
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? HorizontalAlignment.Center : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (HorizontalAlignment)value == HorizontalAlignment.Center;
        }
    }
}
