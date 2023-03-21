using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    /// <summary>
    /// checks to see if a list of guids (index 0) contains a guid (index 1):
    ///     If parameter is boolean true then returns true if list of guids doesn't contain guid.
    ///     If parameter is not included or false then returns true if list of guids contains guid.
    /// </summary>
    public sealed class GuidInGuidsToBooleanConverter : IMultiValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">boolean (optional). If true checks to see if list of guids does not contain guid, else 
        /// checks to see if list of guids does contain guid.</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is HashSet<Guid> && values[1] is Guid)
            {
                if (
                    parameter == null || 
                    (
                        parameter.GetType() == typeof(bool) && 
                        ((bool) parameter) == false
                    )
                ) 
                {
                    if (((HashSet<Guid>)values[0]).Contains((Guid)values[1]))
                        return true;
                }
                else if (
                    parameter != null &&
                    parameter.GetType() == typeof(bool) &&
                    ((bool)parameter) == true
                )
                {
                    if (!((HashSet<Guid>)values[0]).Contains((Guid)values[1]))
                        return true;
                }
            }
            return false;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("UserIdsToBooleanConverterMultiValueConverter is a OneWay converter.");
        }
    }
}
