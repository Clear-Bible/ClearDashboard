using Caliburn.Micro;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using SIL.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.Converters
{
    /// <summary>
    /// Gets the index of an item in a collection
    /// </summary>
    public class GetIndexBindableCollectionMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = (BindableCollection<ExternalNote>)values[1];
            var itemIndex = collection.IndexOf(values[0]);

            if (collection.Count == 1)
            {
                return string.Empty;
            }
            else
            {
                return $"Note: {itemIndex + 1}";
            }
            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("GetIndexMultiConverter_ConvertBack");
        }
    }
}
