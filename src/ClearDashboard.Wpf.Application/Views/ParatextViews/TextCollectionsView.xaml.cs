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

        private void FindNextButtonClick(object sender, EventArgs e)
        {
            Find(true);
        }

        private void FindPreviousButtonClick(object sender, EventArgs e)
        {
            Find(false);
        }

        private void Find(bool next)
        {
            if (!string.IsNullOrEmpty(SearchBox.Text))
            {
                TextCollectionWebBrowser.Find(SearchBox.Text, next, false, true);
            }
            else
            {
                TextCollectionWebBrowser.StopFinding(true);
            }
        }

        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Find(true);
        }
    }
}
