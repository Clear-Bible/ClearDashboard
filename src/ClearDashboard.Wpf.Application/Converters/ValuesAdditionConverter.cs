using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    public class ValuesAdditionConverter : IMultiValueConverter
    {
        /// <summary>
        /// Adds all the values passed as parameters and returns the sum.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values == null || values.Count() < 2)
            {
                return 0;
            }

            double sum = 0;
            for (int i = 0; i < values.Count(); i++)
            {
                sum += (double)values[i];
            }

            return sum;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
