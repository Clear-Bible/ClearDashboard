using Autofac;
using ClearApplicationFoundation;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using FluentValidation;
using System.IO;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.Validators;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using Microsoft.Extensions.DependencyInjection;
using ClearDashboard.Wpf.Application.ViewModels.Startup;

namespace ClearDashboard.Wpf.Application
{
    internal class Bootstrapper : FoundationBootstrapper
    {

        protected override void SetupLogging()
        {
            SetupLogging(Path.Combine(Path.GetTempPath(), "ClearDashboard\\logs\\ClearDashboard.log"));
        }

        protected override void PopulateServiceCollection(ServiceCollection serviceCollection)
        {
            serviceCollection.AddClearDashboardDataAccessLayer();
            serviceCollection.AddValidatorsFromAssemblyContaining<ProjectValidator>();

            base.PopulateServiceCollection(serviceCollection);
        }

        protected override void LoadModules(ContainerBuilder builder)
        {
            base.LoadModules(builder);
            builder.RegisterModule<ApplicationModule>();
        }

        protected override async Task NavigateToMainWindow()
        {
            EnsureApplicationMainWindowVisible();
            NavigateToViewModel<ProjectPickerViewModel>();
            // await base.NavigateToMainWindow();
            // Show the StartupViewModel as a dialog, then navigate to HomeViewModel
            // if the dialog result is "true"
            //await ShowStartupDialog<StartupDialogViewModel, MainViewModel>();

            //await ShowStartupDialog<ProjectPickerViewModel, ProjectSetupViewModel>();
        }
    }
}
