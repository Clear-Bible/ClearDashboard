using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters;

[Flags]
public enum EditMode
{
    MainViewOnly = 1,
    EditorViewOnly = 2,
    ManualToggle = 4
}

public class EditModeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return Visibility.Collapsed;

        //var enumValue = (EditMode)value;

        if (Enum.TryParse<EditMode>(parameter.ToString(), out var parameterValue) && Enum.TryParse<EditMode>(value.ToString(), out var enumValue))
        {
            if (parameterValue.HasFlag(enumValue))
                return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}