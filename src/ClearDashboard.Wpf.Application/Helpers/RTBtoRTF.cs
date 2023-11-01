using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class RtBtoRtf
    {

        /// <summary>
        /// This function takes in text and converts it to a flowdocument.  Going through the rtb control
        /// has the benefit of handing UTF characters correctly.  But it is slow...
        /// </summary>
        /// <param name="s">The verse text</param>
        /// <param name="value">List of words to highlight</param>
        /// <param name="index">List of starting indexes to highlight</param>
        /// <param name="length">List of lengths to highlight</param>
        /// <returns></returns>
        public static string ConvertToRtf(string s, List<string> value, List<int> index, List<int> length, List<bool> colorList, string fontFamily, float fontSize = 14)
        {

            RichTextBox rtb = new RichTextBox();
            FlowDocument doc = new FlowDocument();
            // add in the verse text as a Run
            Paragraph p = new Paragraph(new Run(s))
            {
                FontSize = fontSize,
                Foreground = Brushes.Black,
                FontFamily = new FontFamily(fontFamily)
            };
            doc.Blocks.Add(p);
            rtb.Document = doc;

            // need to select all so that we can compute the text ranges for our indexs
            rtb.SelectAll();

            // iterate through and do the word highlighting
            for (int i = 0; i < value.Count; i++)
            {
                int start = index[i];
                int size = length[i];
                if (size == 1)
                {
                    start = start - 1;
                    if (start < 0)
                    {
                        start = 0;
                    }

                    size += 1;
                }

                if (colorList[i])
                {
                    ColorAndBoldSelection(start, size, (Color)ColorConverter.ConvertFromString("Orange"), rtb);
                }
                else
                {
                    ColorSelection(start, size, Color.FromRgb(175, 140, 0), rtb);
                }
                
            }



            // Convert to RTF and extract from the RTB control
            string rtfFromRtb = string.Empty;
            using (MemoryStream ms = new MemoryStream())
            {
                TextRange range2 = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                range2.Save(ms, System.Windows.DataFormats.Rtf);
                ms.Seek(0, SeekOrigin.Begin);
                using (StreamReader sr = new StreamReader(ms))
                {
                    rtfFromRtb = sr.ReadToEnd();
                }
            }

            return rtfFromRtb;
        }

        private static void ColorAndBoldSelection(int offset, int length, Color color, RichTextBox rtb)
        {
            var textRange = rtb.Selection;
            var start = rtb.Document.ContentStart;
            var startPos = GetPoint(start, offset);
            var endPos = GetPoint(start, offset + length);

            textRange.Select(startPos, endPos);
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
            textRange.ApplyPropertyValue(TextElement.FontWeightProperty, System.Windows.FontWeights.Bold);
        }

        private static void ColorSelection(int offset, int length, Color color, RichTextBox rtb)
        {
            var textRange = rtb.Selection;
            var start = rtb.Document.ContentStart;
            var startPos = GetPoint(start, offset);
            var endPos = GetPoint(start, offset + length);

            textRange.Select(startPos, endPos);
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
        }

        private static TextPointer GetPoint(TextPointer start, int x)
        {

            var ret = start;
            var i = 0;
            while (ret != null)
            {
                string stringSoFar = new TextRange(ret, ret.GetPositionAtOffset(i, LogicalDirection.Forward)).Text;
                if (stringSoFar.Length == x)
                    break;
                i++;
                if (ret.GetPositionAtOffset(i, LogicalDirection.Forward) == null)
                    return ret.GetPositionAtOffset(i - 1, LogicalDirection.Forward);
    
            }
            ret = ret.GetPositionAtOffset(i, LogicalDirection.Forward);
            return ret;
        }


    }
}
