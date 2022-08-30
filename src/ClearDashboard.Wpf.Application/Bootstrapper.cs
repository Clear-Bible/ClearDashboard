using Autofac;
using ClearApplicationFoundation;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using FluentValidation;
using System.IO;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.Validators;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace ClearDashboard.Wpf.Application
{
    internal class Bootstrapper : FoundationBootstrapper
    {

        protected override void SetupLogging()
        {
            SetupLogging(Path.Combine(Path.GetTempPath(), "ClearDashboard\\logs\\ClearDashboard.log"));
        }

        protected override void LoadModules(ContainerBuilder builder)
        {
            base.LoadModules(builder);
            builder.RegisterModule<ApplicationModule>();
            //builder.RegisterModule<DataAccessLayerModule>();
        }

        protected override async Task NavigateToMainWindow()
        {
            base.EnsureApplicationMainWindowVisible();
            NavigateToViewModel<MainViewModel>();
           // await base.NavigateToMainWindow();
            // Show the StartupViewModel as a dialog, then navigate to HomeViewModel
            // if the dialog result is "true"
            // await ShowStartupDialog<StartupViewModel, HomeViewModel>();
            //await ShowStartupDialog<ProjectPickerViewModel, ProjectSetupViewModel>();
        }

        protected override void PostInitialize()
        {
            base.PostInitialize();
        }

        protected override void PopulateServiceCollection(ServiceCollection serviceCollection)
        {
            serviceCollection.AddClearDashboardDataAccessLayer();
            serviceCollection.AddValidatorsFromAssemblyContaining<ProjectValidator>();

            base.PopulateServiceCollection(serviceCollection);
        }
    }
}
