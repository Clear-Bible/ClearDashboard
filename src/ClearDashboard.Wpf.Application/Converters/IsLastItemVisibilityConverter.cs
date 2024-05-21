using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters;

public class IsLastItemVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Count() < 2)
        {
            return Visibility.Hidden;
        }

        if (values[0] is int count && values[1] is int index)
        {
            return index < count - 1 ? Visibility.Visible : Visibility.Hidden;
        }
        return Visibility.Hidden;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}