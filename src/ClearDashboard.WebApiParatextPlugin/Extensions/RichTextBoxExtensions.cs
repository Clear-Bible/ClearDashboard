using System.Drawing;
using System.Windows.Forms;

namespace ClearDashboard.WebApiParatextPlugin.Extensions
{
    public static class RichTextBoxExtensions
    {
        private delegate void AppendTextDelegate(RichTextBox rtb, string text, Color color);
        private delegate void AppendTextFontSizeDelegate(string text, Color color, float iFontSize, RichTextBox rtb);

        public static void AppendText(this RichTextBox rtb, string text, Color color)
        {
            //check for threading issues
            if (rtb.InvokeRequired)
            {
                text = text + " ****** other thread ******";
                rtb.Invoke(new AppendTextDelegate(AppendText), new object[] { rtb, text, color});
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
    }
}
