using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using static ClearDashboard.Aqua.Module.ViewModels.AquaCorpusAnalysisEnhancedViewItemViewModel;

namespace ClearDashboard.Aqua.Module.Converters
{
    public sealed class AquaTypeHasVisualizationVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is TypeAnalysisConfiguration && parameter is string && value is TypeAnalysisConfiguration)
            {
                if ( ((TypeAnalysisConfiguration) value).visualizations
                    .Where(v => v.ToString().Equals(parameter.ToString()))
                    .Count() > 0)
                    return Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
