using System.Drawing;
using System.Windows.Forms;

namespace ClearDashboard.ParatextPlugin
{
    static class cRTB
    {
        private delegate void AppendTextDelegate(string text, Color color, RichTextBox rtb);
        private delegate void AppendTextFontSizeDelegate(string text, Color color, float iFontSize, RichTextBox rtb);

        /// <summary>
        /// Add in colored text to the RichTextBox
        /// </summary>
        /// <param name="text">Text to add</param>
        /// <param name="color">Color of text</param>
        public static void AppendText(string text, Color color, RichTextBox rtb)
        {
            //check for threading issues
            if (rtb.InvokeRequired)
            {
                rtb.Invoke(new AppendTextDelegate(AppendText), new object[] { text, color, rtb });

                //AppendTextDelegate d = new AppendTextDelegate(AppendText);
                //d(text, color);
            }
            else
            {
                rtb.SelectionStart = rtb.TextLength;
                rtb.SelectionLength = 0;

                rtb.SelectionColor = color;
                rtb.AppendText(text);
                rtb.SelectionColor = rtb.ForeColor;

                // set the current caret position to the end
                rtb.SelectionStart = rtb.Text.Length;
                // scroll it automatically
                rtb.ScrollToCaret();

            }


        }

        /// <summary>
        /// Add in colored text to the RichTextBox
        /// </summary>
        /// <param name="text">Text to add</param>
        /// <param name="color">Color of text</param>
        public static void AppendTextDelegateFontSize(string text, Color color, float iFontSize, RichTextBox rtb)
        {
            //check for threading issues
            if (rtb.InvokeRequired)
            {
                rtb.Invoke(new AppendTextFontSizeDelegate(AppendTextDelegateFontSize), new object[] { text, color, iFontSize, rtb });

                //AppendTextDelegate d = new AppendTextDelegate(AppendText);
                //d(text, color);
            }
            else
            {
                rtb.SelectionStart = rtb.TextLength;
                rtb.SelectionLength = 0;

                rtb.SelectionColor = color;
                rtb.SelectionFont = new Font("Tahoma", iFontSize);
                rtb.AppendText(text);
                rtb.SelectionColor = rtb.ForeColor;

                // set the current caret position to the end
                rtb.SelectionStart = rtb.Text.Length;
                // scroll it automatically
                rtb.ScrollToCaret();
            }
        }

    }
}
