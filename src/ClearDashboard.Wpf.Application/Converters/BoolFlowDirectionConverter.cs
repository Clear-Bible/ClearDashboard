using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    /// <summary>
    /// Converts a logical "IsRtl" bool to a <see cref="FlowDirection"/>, where <b>false</b> = <see cref="FlowDirection.LeftToRight"/>
    /// and <b>true</b> = <see cref="FlowDirection.RightToLeft"/>.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(FlowDirection))]
    public class BoolFlowDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (FlowDirection)value == FlowDirection.RightToLeft;
        }
    }

}
