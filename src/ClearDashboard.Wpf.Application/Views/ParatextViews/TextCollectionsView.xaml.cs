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
    public partial class TextCollectionsView : UserControl//, IHandle<TextCollectionChangedMessage>
    {
        //public bool TextCollectionLoaded { get; set; }
        //public string DisplayHtml { get; set; }
       

        public TextCollectionsView()
        {
            InitializeComponent();

            //IEventAggregator eventAggregator = IoC.Get<IEventAggregator>();
            //eventAggregator.Subscribe(this);
        }

        //public Task HandleAsync(TextCollectionChangedMessage message, CancellationToken cancellationToken)
        //{
        //    DisplayHtml = message.TextCollections.FirstOrDefault().Data;

        //    SetHtml(DisplayHtml, TextCollectionLoaded);

        //    return Task.CompletedTask;
        //}

        //private void WebBrowser_OnLoaded(object sender, RoutedEventArgs e)
        //{
        //    TextCollectionLoaded = true;

        //    SetHtml(DisplayHtml, TextCollectionLoaded);
        //}

        //private void SetHtml(string html, bool condition)
        //{
        //    if (condition)
        //    {
        //        //webBrowser.NavigateToString(html);
        //    }
        //} 
    }
}
