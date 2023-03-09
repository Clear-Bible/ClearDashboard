using ClearDashboard.Aqua.Module.Services;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static ClearDashboard.Aqua.Module.ViewModels.AquaCorpusAnalysisEnhancedViewItemViewModel;

namespace ClearDashboard.Aqua.Module.Converters
{
    public sealed class AquaVisualizationToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is VisualizationEnum && parameter is string)
            {
                if (value?.ToString()?.Equals(parameter.ToString()) ?? false)
                    return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null && value is bool && parameter is string)
                {
                    if ((bool)value)
                        return Enum.Parse(targetType, parameter?.ToString() ?? "");
                }
            }
            catch (Exception)
            {
            }
            return Binding.DoNothing;
        }
    }
}
