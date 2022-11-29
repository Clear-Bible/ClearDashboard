

using System.Diagnostics;
using System;
using System.Windows.Interop;

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
        }
        
    }
}
