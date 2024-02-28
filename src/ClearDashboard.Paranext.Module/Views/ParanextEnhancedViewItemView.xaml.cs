using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using ClearDashboard.Paranext.Module.ViewModels;
using Microsoft.Web.WebView2.Wpf;

namespace ClearDashboard.Paranext.Module.Views
{
    /// <summary>
    /// Note: using CEF rather than WebView2 due to window overlapping issue https://github.com/MicrosoftEdge/WebView2Feedback/issues/356
    /// </summary>
    public partial class ParanextEnhancedViewItemView : UserControl
    {
        public ParanextEnhancedViewItemView()
        {
            InitializeComponent();
            Cef.RenderProcessMessageHandler = new RenderProcessMessageHandler();
            
            
            //Loaded += (s, e) =>
            //{
            //    Window.GetWindow(this)
            //          .Closing += (s1, e1) => Closing();
            //};
        }

        private void Cef_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
           if (((CefSharp.Wpf.ChromiumWebBrowser) sender).IsBrowserInitialized)
            {
                // ((ParanextEnhancedViewItemViewModel)DataContext).RefreshData();
            }
        }

        private void Cef_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                Application.Current.Dispatcher.Invoke(() => ((ParanextEnhancedViewItemViewModel)DataContext).SetVerse());
            }
        }

        //private async void WebView2_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        //{
        //    // await ((WebView2)sender).CoreWebView2.Profile.ClearBrowsingDataAsync();
        //    await ((WebView2)sender).CoreWebView2.ExecuteScriptAsync("javascript:localStorage.clear()");
        //}

        //private void Closing()
        //{
        //    try
        //    {
        //        WebView2.CoreWebView2?.Stop();
        //        WebView2.Dispose();
        //    }
        //    catch (ObjectDisposedException)
        //    {
        //    }
        //}
    }

    public class RenderProcessMessageHandler : IRenderProcessMessageHandler
    {
        public void OnContextReleased(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
        }

        public void OnFocusedNodeChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IDomNode node)
        {
        }

        public void OnUncaughtException(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, JavascriptException exception)
        {
        }

        async void IRenderProcessMessageHandler.OnContextCreated(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
            if (frame.IsMain)
            {
                await frame.EvaluateScriptAsync("localStorage.removeItem('client-network-connector:clientGuid');");
                // await frame.EvaluateScriptAsync("localStorage.clear();");
            }
        }
    }
}
