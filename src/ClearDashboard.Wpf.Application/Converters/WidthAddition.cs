using LiveChartsCore.Measure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ClearDashboard.Wpf.Application.Converters
{
    public class WidthAddition : MarkupExtension, IValueConverter
    {
        private static WidthAddition _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int width = 0;

            if (value is System.Windows.Thickness margin)
            {
                width = System.Convert.ToInt16(margin.Left) + System.Convert.ToInt16(parameter);
            }
            
            if (value is int)
            {
                width = System.Convert.ToInt16(value) + System.Convert.ToInt16(parameter);
            }

            return width;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new WidthAddition());
        }
    }
}
