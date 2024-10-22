﻿

using System.Diagnostics;
using System;
using System.IO;
using System.Windows.Interop;
using CefSharp.Wpf;
using CefSharp;
using System.Reflection;
using System.Threading;

namespace ClearDashboard.Wpf.Application
{

    public class BindingErrorListener : TraceListener
    {
        private readonly Action<string> _errorHandler;

        public BindingErrorListener(Action<string> errorHandler)
        {
            _errorHandler = errorHandler;
            TraceSource bindingTrace = PresentationTraceSources
                .DataBindingSource;

            bindingTrace.Listeners.Add(this);
            bindingTrace.Switch.Level = SourceLevels.Error;
        }

        public override void WriteLine(string message)
        {
            _errorHandler?.Invoke(message);
        }

        public override void Write(string message)
        {
        }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public App()
        {
            // ReSharper disable once ObjectCreationAsStatement
            //new BindingErrorListener(msg => Debugger.Break());


            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            };

            //Example of setting a command line argument
            //Enables WebRTC
            // - CEF Doesn't currently support permissions on a per browser basis see https://bitbucket.org/chromiumembedded/cef/issues/2582/allow-run-time-handling-of-media-access
            // - CEF Doesn't currently support displaying a UI for media access permissions
            //
            //NOTE: WebRTC Device Id's aren't persisted as they are in Chrome see https://bitbucket.org/chromiumembedded/cef/issues/2064/persist-webrtc-deviceids-across-restart
            settings.CefCommandLineArgs.Add("enable-media-stream");
            // https://peter.sh/experiments/chromium-command-line-switches/#use-fake-ui-for-media-stream
            settings.CefCommandLineArgs.Add("use-fake-ui-for-media-stream");
            //For screen sharing add (see https://bitbucket.org/chromiumembedded/cef/issues/2582/allow-run-time-handling-of-media-access#comment-58677180)
            settings.CefCommandLineArgs.Add("enable-usermedia-screen-capturing");

            //Example of checking if a call to Cef.Initialize has already been made, we require this for
            //our .Net 5.0 Single File Publish example, you don't typically need to perform this check
            //if you call Cef.Initialze within your WPF App constructor.
            if (!Cef.IsInitialized)
            {
                //Perform dependency check to make sure all relevant resources are in our output directory.
                Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
            }

            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
   

        private Assembly CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            try
            {
                if (args.Name != null && args.Name.Contains(".resources") && !args.Name.Contains("ClearDashboard.Wpf.Application.resources"))
                {
                    var assemblyName = args.Name.Split(",")[0];
                    string assemblyPath = $"{AppDomain.CurrentDomain.BaseDirectory}{Thread.CurrentThread.CurrentUICulture.Name}\\{assemblyName}.dll";
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    return assembly;
                }

                return null;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error loading translations for {args.Name}");
                Trace.WriteLine(e);
                return null;
            }
        }

    }
}
