using Caliburn.Micro;
using PluginManager.ViewModels;
using System.Windows;

namespace PluginManager
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();

            LogManager.GetLog = type => new DebugLog(type);
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            await DisplayRootViewForAsync(typeof(ShellViewModel));
        }

    }
}
