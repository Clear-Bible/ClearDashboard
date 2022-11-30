using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClearDashboard.DataAccessLayer.Annotations;

namespace ClearDashboard.Wpf.Application.Views.ParatextViews
{
    /// <summary>
    /// Interaction logic for TextCollectionsView.xaml
    /// </summary>
    public partial class TextCollectionsView : UserControl, IHandle<TextCollectionChangedMessage>
    {
        private string html = @"<!DOCTYPE html>
        <html lang=""en"">
            <body>
            <div>My Test HTML 'single quote', ""double quote""</div>
            </body>
        </html>";
        public TextCollectionsView()
        {
            InitializeComponent();

            webBrowser.NavigateToString(html);

            IEventAggregator eventAggregator = IoC.Get<IEventAggregator>();
            eventAggregator.Subscribe(this);
        }

        public Task HandleAsync(TextCollectionChangedMessage message, CancellationToken cancellationToken)
        {
            //var html = message.TextCollections.FirstOrDefault().Data;

            var html = "<html>\r\n  <head>\r\n    <title>Hello, World!</title>\r\n  </head>\r\n  <body>\r\n      <h1 class=\"title\">Hello World! </h1>\r\n  </body>\r\n</html>";

            //webBrowser.NavigateToString(html);

            return Task.CompletedTask;
        }

        private void WebBrowserTest_OnClick(object sender, RoutedEventArgs e)
        {
            //Uri uri = new Uri("page.html", UriKind.Absolute);
            //myFrame.Source = uri;

            //// Only absolute URIs can be navigated to  
            //if (!uri.IsAbsoluteUri)
            //{
            //    MessageBox.Show("The Address URI must be absolute. For example, 'http://www.microsoft.com'");
            //    return;
            //}

            // Navigate to the desired URL by calling the .Navigate method  
            //this.webBrowser.Navigate(uri);
            //webBrowser.NavigateToString("<html><body><b>Programmatic content</b></body></html>");
        }

        private void WebBrowser_OnLoaded(object sender, RoutedEventArgs e)
        {
            //Dispatcher.BeginInvoke(() =>
            //{
            //    webBrowser.NavigateToString("<html><body>This works!</body></html>");
            //});
        }
    }
}
