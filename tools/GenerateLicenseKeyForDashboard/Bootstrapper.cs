using System.Windows;
using Caliburn.Micro;
using GenerateLicenseKeyForDashboard.ViewModels;

namespace GenerateLicenseKeyForDashboard
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            await DisplayRootViewForAsync(typeof(ShellViewModel));
        }
    }
}
