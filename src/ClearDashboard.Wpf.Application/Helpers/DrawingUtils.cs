using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class DrawingUtils
    {
        /// <summary>
        /// Used to get the size of the string so we know how far to offset the
        /// next boxs
        /// </summary>
        /// <param name="candidate"></param>
        /// <param name="tb"></param>
        /// <returns></returns>
        public static Size MeasureString(string candidate, TextBlock tb)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch),
                tb.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}
