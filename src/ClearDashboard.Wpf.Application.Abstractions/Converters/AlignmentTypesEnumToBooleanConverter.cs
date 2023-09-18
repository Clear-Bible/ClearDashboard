using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Wpf.Application.Converters
{
    public class AlignmentTypesEnumToBooleanConverter : IValueConverter
    {
        private AlignmentTypes _target;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mask = (AlignmentTypes)parameter;
            _target = (AlignmentTypes)value;
            return ((mask & _target) != 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _target ^= (AlignmentTypes)parameter;
            return _target;
        }
    }
}
