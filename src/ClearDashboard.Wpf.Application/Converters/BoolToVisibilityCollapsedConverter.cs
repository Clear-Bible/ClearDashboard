using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    /// <summary>
  /// Source: http://stackoverflow.com/questions/534575/how-do-i-invert-booleantovisibilityconverter
  /// 
  /// Implements a Boolean to Visibility converter, using <see cref="Visibility.Collapsed"/> for false.
  /// Use ConverterParameter=true to negate the visibility - boolean interpretation.
  /// </summary>
  [ValueConversion(typeof(Boolean), typeof(Visibility))]
    public sealed class BoolToVisibilityCollapsedConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <seealso cref="Boolean"/> value
        /// into a <seealso cref="Visibility"/> value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool IsInverted = false;

            if (parameter == null)
                IsInverted = false;
            else if (parameter == "true")
                IsInverted = true;
            
            if (parameter == "true" )
                return Visibility.Collapsed;

            //bool IsInverted = parameter == null ? false : (bool)parameter;
            bool IsVisible = value == null ? false : (bool)value;
            if (IsVisible)
                return IsInverted ? Visibility.Collapsed : Visibility.Visible;
                
            return IsInverted ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a <seealso cref="Visibility"/> value
        /// into a <seealso cref="Boolean"/> value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visiblility = value == null ? Visibility.Collapsed : (Visibility)value;
            bool IsInverted = parameter == null ? false : (bool)parameter;

            return (visiblility == Visibility.Visible) != IsInverted;
        }
    }
}
