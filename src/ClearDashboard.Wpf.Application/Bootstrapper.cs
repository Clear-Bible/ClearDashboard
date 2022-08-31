using Autofac;
using ClearApplicationFoundation;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using ClearDashboard.Wpf.Application.Validators;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;

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
            serviceCollection.AddValidatorsFromAssemblyContaining<AddParatextCorpusDialogViewModelValidator>();

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

            NavigateToViewModel<MainViewModel>();

            // await base.NavigateToMainWindow();
            // Show the StartupViewModel as a dialog, then navigate to HomeViewModel
            // if the dialog result is "true"
            //await ShowStartupDialog<StartupViewModel, MainViewModel>();
            //await ShowStartupDialog<ProjectPickerViewModel, ProjectSetupViewModel>();
            
        }


      
    }
}
