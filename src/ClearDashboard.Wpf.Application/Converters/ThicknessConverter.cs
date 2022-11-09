using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    /// <summary>
    /// Converts four doubles bool to a <see cref="Thickness"/>.
    /// </summary>
    [ValueConversion(typeof(double[]), typeof(Thickness))]
    public class ThicknessConverter : IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new Thickness(System.Convert.ToDouble(values[0]),
                                  System.Convert.ToDouble(values[1]), 
                                  System.Convert.ToDouble(values[2]),
                                System.Convert.ToDouble(values[3]));
        }

        public object[] ConvertBack(object value, System.Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
