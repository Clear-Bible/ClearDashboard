using ClearDashboard.Aqua.Module.Services;
using SIL.Extensions;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static ClearDashboard.Aqua.Module.ViewModels.AquaCorpusAnalysisEnhancedViewItemViewModel;
    
namespace ClearDashboard.Aqua.Module.Converters
{
    public sealed class AquaVisualizationToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is VisualizationEnum && parameter is string)
            {
                var parameters = ((string)parameter).Split(',');
                if (parameters != null && parameters?.Length > 0 && parameters.IndexOf(value?.ToString()) > -1)
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;                
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
