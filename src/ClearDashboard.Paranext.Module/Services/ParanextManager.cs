using Caliburn.Micro;
using CefSharp.OffScreen;
using CefSharp;
using ClearDashboard.DAL.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SIL.Scripture;
using CefSharp.JavascriptBinding;
using ClearDashboard.Wpf.Application.Messages;
using System.Threading;
using MailKit;

namespace ClearDashboard.Paranext.Module.Services
{
    public class ParanextManager : 
        IParanextManager, 
        IDisposable, 
        IHandle<VerseChangedMessage>
    {
        private readonly string rendererUrl = "localhost:1212/paranextExtensionDashboard_services";
        private ChromiumWebBrowser? browser;

        protected IEventAggregator eventAggregator_;
        protected ILogger<ParanextManager>? logger_;
        protected IMediator mediator_;
        protected IUserProvider userProvider_;
        protected HttpClient httpClient_;

        public ParanextManager(
            IEventAggregator eventAggregator,
            ILogger<ParanextManager>? logger,
            IMediator mediator,
            IUserProvider userProvider)
        {
            eventAggregator_ = eventAggregator;
            logger_ = logger;
            mediator_ = mediator;
            userProvider_ = userProvider;

            eventAggregator_.SubscribeOnUIThread(this);
        }

        public async Task LoadRenderer()
        {
            CefSharpSettings.ConcurrentTaskExecution = true;
            var settings = new CefSettings();
            settings.CachePath = Path.GetFullPath("cache");
            var success = await Cef.InitializeAsync(settings);
            if (!success) throw new Exception("Javascript runtime failed to initialize");

            var browserSettings = new BrowserSettings
            {
                //Reduce rendering speed to one frame per second so it's easier to take screen shots
                WindowlessFrameRate = 1
            };

            var requestContextSettings = new RequestContextSettings
            {
                CachePath = Path.GetFullPath("cache\\path1")
            };

            // RequestContext can be shared between browser instances and allows for custom settings
            // e.g. CachePath
            var requestContext = new RequestContext(requestContextSettings);
            browser = new ChromiumWebBrowser(rendererUrl, browserSettings, requestContext, automaticallyCreateBrowser: false, useLegacyRenderHandler: false);

            //if (zoomLevel > 1)
            //{
            //    browser.FrameLoadStart += (s, argsi) =>
            //    {
            //        var b = (ChromiumWebBrowser)s;
            //        if (argsi.Frame.IsMain)
            //        {
            //            b.SetZoomLevel(zoomLevel);
            //        }
            //    };
            //}
            browser.ConsoleMessage += (sender, e) =>
            {
                Debug.WriteLine($"BROWSERMESSAGE: {e.Message}");
            };
            browser.JavascriptObjectRepository.ObjectBoundInJavascript += (sender, e) =>
            {
                var name = e.ObjectName;

                Debug.WriteLine($"Object {e.ObjectName} was bound successfully.");
            };

            // var bindingOptions = new BindingOptions { Binder = new DashboardJavascriptApiBinder() };
            browser.JavascriptObjectRepository.NameConverter = new CamelCaseJavascriptNameConverter();
            browser.JavascriptObjectRepository.Register("dashboardAsync", new DashboardJavascriptApi(), options: BindingOptions.DefaultBinder);

            //browser.JavascriptObjectRepository.ResolveObject += (sender, e) =>
            //{
            //    var repo = e.ObjectRepository;
            //    if (e.ObjectName == "dashboardAsync")
            //    {
            //        BindingOptions bindingOptions = null; //Binding options is an optional param, defaults to null
            //        bindingOptions = BindingOptions.DefaultBinder; //Use the default binder to serialize values into complex objects

            //        bindingOptions = new BindingOptions { Binder = new DashboardJavascriptApiBinder() }; //Specify a custom binder
            //        repo.NameConverter = null; //No CamelCase of Javascript Names
            //                                   //For backwards compatability reasons the default NameConverter doesn't change the case of the objects returned from methods calls.
            //                                   //https://github.com/cefsharp/CefSharp/issues/2442
            //                                   //Use the new name converter to bound object method names and property names of returned objects converted to camelCase
            //        repo.NameConverter = new CamelCaseJavascriptNameConverter();
            //        repo.Register("dashboardAsync", new DashboardJavascriptApi(eventAggregator_), options: bindingOptions);
            //    }
            //};

            browser.CreateBrowser();
            await browser.WaitForInitialLoadAsync();

            //Check preferences on the CEF UI Thread
            //await Cef.UIThreadTaskFactory.StartNew(delegate
            //{
            //    var preferences = requestContext.GetAllPreferences(true);

            //    //Check do not track status
            //    var doNotTrack = (bool)preferences["enable_do_not_track"];

            //    Debug.WriteLine("DoNotTrack: " + doNotTrack);
            //});

            //var onUi = Cef.CurrentlyOnThread(CefThreadIds.TID_UI);

            // For Google.com pre-pupulate the search text box
            //if (url.Contains("google.com"))
            //{
            //    await browser.EvaluateScriptAsync("document.querySelector('[name=q]').value = 'CefSharp Was Here!'");
            //}
        }

        public async Task EvaluateScriptInRendererAsync(string script)
        {
            await browser.EvaluateScriptAsync(script);
        }
        public async Task SendVerseChangeCommandAsync(VerseRef verseRef)
        {
            await browser.EvaluateScriptAsync($"papi.commands.sendCommand('platform.verseChange', '{verseRef}', 0);");
        }

        public void Dispose()
        {
            browser?.Dispose();
        }

        public Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
