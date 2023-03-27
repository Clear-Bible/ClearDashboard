using ClearDashboard.Aqua.Module.Services;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ClearDashboard.Aqua.Module.Converters
{
    public sealed class AquaAssessmentStatusToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is string)
            {
                if (value.Equals(IAquaManager.Status_Finished))
                    return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
