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
            AlignmentTypes mask = (AlignmentTypes)parameter;
            this._target = (AlignmentTypes)value;
            return ((mask & this._target) != 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            this._target ^= (AlignmentTypes)parameter;
            return this._target;
        }
    }
}
