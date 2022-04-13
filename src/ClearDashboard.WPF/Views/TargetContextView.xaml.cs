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

            //Browser.NavigationStarting += EnsureHttps;

            // initialize the webview2 control
            Browser.EnsureCoreWebView2Async();


            //Browser.Source = new Uri("127.0.0.1");

            // InitializeAsync();
        }

        /// <summary>
        /// Makes sure that we are running HTTPS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void EnsureHttps(object? sender, CoreWebView2NavigationStartingEventArgs args)
        {
            String uri = args.Uri;
            if (!uri.StartsWith("https://"))
            {
                Browser.CoreWebView2.ExecuteScriptAsync($"alert('{uri} is not safe, try an https link')");
                args.Cancel = true;
            }
        }
        private async void InitializeAsync()
        {
            // set the directory for which the webview control can write data to
            var userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ClearDashboard";
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await Browser.EnsureCoreWebView2Async(env);

            // point the source to to Documents\CLEAR_Resources\build\
            var webFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            webFolder = System.IO.Path.GetFullPath(System.IO.Path.Combine(webFolder, @"CLEAR_Resources\build"));

            if (Directory.Exists(webFolder))
            {
                Browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "Clear.Server", webFolder,
                    CoreWebView2HostResourceAccessKind.Allow);
                Browser.Source = new Uri("https://Clear.Server/index.html");
                //Browser.CoreWebView2.WebMessageReceived += MessageHandler;
            }
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
                        Browser.NavigateToString(html);
                        _lastHTML = html;
                    }
                    return;
                }
            };
        }

        private void UserControl_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            // oddly this gets triggered before the UserControl_Loaded
            if (_vm is not null)
            {
                string html = _vm.TargetHTML;
                if (_lastHTML != html)
                {
                    Browser.NavigateToString(html);
                    _lastHTML = html;
                }
            }
        }
    }
}
