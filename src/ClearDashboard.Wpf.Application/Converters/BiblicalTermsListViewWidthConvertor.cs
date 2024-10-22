﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ClearDashboard.Wpf.Application.Converters
{
    internal class BiblicalTermsListViewWidthConvertor : MarkupExtension, IValueConverter
    {
        private static BiblicalTermsListViewWidthConvertor _instance;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var width = System.Convert.ToDouble(value) - System.Convert.ToDouble(parameter);

            if (width < 0)
            {
                width = 0;
            }

            return width;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new BiblicalTermsListViewWidthConvertor());
        }
    }
}
