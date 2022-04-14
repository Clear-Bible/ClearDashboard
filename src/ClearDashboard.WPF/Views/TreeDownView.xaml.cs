using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Microsoft.Web.WebView2.Core;

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// Interaction logic for TreeDown.xaml
    /// </summary>
    public partial class TreeDownView : UserControl
    {
        public TreeDownView()
        {
            InitializeComponent();
            Browser.NavigationStarting += EnsureHttps;

            InitializeAsync();
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
    }
}
