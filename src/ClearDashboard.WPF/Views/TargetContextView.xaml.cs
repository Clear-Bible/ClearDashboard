using Microsoft.Web.WebView2.Core;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using ClearDashboard.Wpf.ViewModels;

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// Interaction logic for TargetContext.xaml
    /// </summary>
    public partial class TargetContextView : UserControl
    {
        private TargetContextViewModel _vm;
        private string _lastHTML = "";


        public TargetContextView()
        {
            InitializeComponent();

            // initialize the webview2 control
            Browser.EnsureCoreWebView2Async();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _vm = (TargetContextViewModel)this.DataContext;

            // rarely get's hit because the webview2 control doesn't activate until it 
            // is visible to the user
            INotifyPropertyChanged viewModel = (INotifyPropertyChanged)this.DataContext;
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals("TargetHTML"))
                {
                    string html = _vm.TargetHTML;
                    if (_lastHTML != html)
                    {
                        _lastHTML = html;

                        UriBuilder uriBuilder = new UriBuilder(@"D:\temp\output.html")
                        {
                            Fragment = _vm.AnchorRef
                        };
                        Browser.Source = uriBuilder.Uri;
                    }
                    return;
                } 
                else if (args.PropertyName.Equals("AnchorRef"))
                {
                    // update the verse location
                    UriBuilder uriBuilder = new UriBuilder(@"D:\temp\output.html")
                    {
                        Fragment = _vm.AnchorRef
                    };
                    Browser.Source = uriBuilder.Uri;
                }
            };
        }


        private void radFormatted_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            // oddly this gets triggered before the UserControl_Loaded
            if (_vm is not null)
            {
                string html = _vm.TargetHTML;
                if (_lastHTML.Length != html.Length)
                {
                    //Browser.NavigateToString(html);
                    _lastHTML = html;


                    UriBuilder uriBuilder = new UriBuilder(@"D:\temp\output.html")
                    {
                        Fragment = _vm.AnchorRef
                    };
                    Browser.Source = uriBuilder.Uri;
                }
            }
        }
    }
}
