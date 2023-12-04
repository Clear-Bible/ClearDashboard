using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public class RichTextBoxHelper : DependencyObject
    {
        public static string GetDocumentRtf(DependencyObject obj)
        {
            return (string)obj.GetValue(DocumentRtfProperty);
        }

        public static void SetDocumentRtf(DependencyObject obj, string value)
        {
            obj.SetValue(DocumentRtfProperty, value);
        }

        public static readonly DependencyProperty DocumentRtfProperty =
            DependencyProperty.RegisterAttached(
                "DocumentRtf",
                typeof(string),
                typeof(RichTextBoxHelper),
                new FrameworkPropertyMetadata
                {
                    BindsTwoWayByDefault = true,
                    PropertyChangedCallback = (obj, e) =>
                    {
                        var richTextBox = (RichTextBox)obj;

                        // Parse the XAML to a document (or use XamlReader.Parse())
                        var xaml = GetDocumentRtf(richTextBox);
                        var doc = new FlowDocument();
                        var range = new TextRange(doc.ContentStart, doc.ContentEnd);

                        range.Load(new MemoryStream(Encoding.UTF8.GetBytes(xaml)),
                            DataFormats.Rtf);

                        // Set the document
                        richTextBox.Document = doc;

                        // When the document changes update the source
                        range.Changed += (obj2, e2) =>
                        {
                            if (richTextBox.Document == doc)
                            {
                                MemoryStream buffer = new MemoryStream();
                                range.Save(buffer, DataFormats.Xaml);
                                SetDocumentRtf(richTextBox,
                                    Encoding.UTF8.GetString(buffer.ToArray()));
                            }
                        };
                    }
                });
    }
}
