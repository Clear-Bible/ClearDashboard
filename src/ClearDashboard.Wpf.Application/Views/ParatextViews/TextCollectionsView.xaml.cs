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
using CefSharp;
using CefSharp.Wpf;
using ClearDashboard.DataAccessLayer.Annotations;

namespace ClearDashboard.Wpf.Application.Views.ParatextViews
{
    /// <summary>
    /// Interaction logic for TextCollectionsView.xaml
    /// </summary>
    public partial class TextCollectionsView : UserControl//, IHandle<TextCollectionChangedMessage>
    {
        public TextCollectionsView()
        {
            InitializeComponent();
        }

        private void Chromium_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = Helpers.Helpers.GetChildOfType<ScrollViewer>(TextCollectionListView);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta/3);
            e.Handled = true;
        }

        private void TextCollectionWebBrowser_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is ChromiumWebBrowser webBrowser && webBrowser.BrowserCore != null)
            {
                webBrowser.Reload(true);
            }
        }
    }
}
