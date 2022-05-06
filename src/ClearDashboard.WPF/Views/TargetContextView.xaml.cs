using ClearDashboard.Wpf.ViewModels;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// Interaction logic for TargetContext.xaml
    /// </summary>
    public partial class TargetContextView : UserControl
    {
        private TargetContextViewModel _vm;
        private string _lastHtml = "";


        public TargetContextView()
        {
            InitializeComponent();

            // initialize the webview2 control
            Browser.EnsureCoreWebView2Async();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _vm = (TargetContextViewModel)this.DataContext;

            // trigger the loading of the unformatted
            radUnformatted_Checked(null, null);


            // rarely get's hit because the webview2 control doesn't activate until it 
            // is visible to the user
            INotifyPropertyChanged viewModel = (INotifyPropertyChanged)this.DataContext;
            viewModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName is null)
                {
                    return;
                }

                if (radFormatted.IsChecked == true)
                {
                    if (args.PropertyName.Equals("FormattedHTML"))
                    {
                        string html = _vm.FormattedHTML;
                        if (_lastHtml != html)
                        {
                            _lastHtml = html;

                            UriBuilder uriBuilder = new UriBuilder(_vm.HtmlPath)
                            {
                                Fragment = _vm.FormattedAnchorRef
                            };
                            Browser.Source = uriBuilder.Uri;
                        }

                        return;
                    }

                    if (args.PropertyName.Equals("FormattedAnchorRef"))
                    {
                        // update the verse location
                        UriBuilder uriBuilder = new UriBuilder(_vm.HtmlPath)
                        {
                            Fragment = _vm.FormattedAnchorRef
                        };
                        Browser.Source = uriBuilder.Uri;
                    }
                }
                else
                {
                    if (args.PropertyName.Equals("UnformattedHTML"))
                    {
                        string html = _vm.UnformattedHTML;
                        if (_lastHtml != html)
                        {
                            _lastHtml = html;

                            UriBuilder uriBuilder = new UriBuilder(_vm.UnformattedPath)
                            {
                                Fragment = _vm.UnformattedAnchorRef
                            };
                            Browser.Source = uriBuilder.Uri;
                        }

                        return;
                    }

                    if (args.PropertyName.Equals("UnformattedAnchorRef"))
                    {
                        // update the verse location
                        UriBuilder uriBuilder = new UriBuilder(_vm.UnformattedPath)
                        {
                            Fragment = _vm.UnformattedAnchorRef
                        };
                        Browser.Source = uriBuilder.Uri;
                    }
                }
            };
        }


        private void radFormatted_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_vm is not null)
            {
                string html = _vm.FormattedHTML;
                if (html == null)
                {
                    return;
                }
                if (_lastHtml.Length != html.Length)
                {
                    //Browser.NavigateToString(html);
                    _lastHtml = html;


                    UriBuilder uriBuilder = new UriBuilder(_vm.HtmlPath)
                    {
                        Fragment = _vm.FormattedAnchorRef
                    };
                    Browser.Source = uriBuilder.Uri;
                }
            }
        }

        private void radUnformatted_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_vm is not null)
            {
                string html = _vm.UnformattedHTML;
                if (_lastHtml.Length != html.Length)
                {
                    //Browser.NavigateToString(html);
                    _lastHtml = html;


                    UriBuilder uriBuilder = new UriBuilder(_vm.UnformattedPath)
                    {
                        Fragment = _vm.UnformattedAnchorRef
                    };
                    Browser.Source = uriBuilder.Uri;
                }
            }
        }
    }
}
