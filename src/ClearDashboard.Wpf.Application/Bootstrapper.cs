using Autofac;
using ClearApplicationFoundation;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using ClearDashboard.Wpf.Application.Validators;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DashboardApplication = System.Windows.Application;


namespace ClearDashboard.Wpf.Application
{
    internal class Bootstrapper : FoundationBootstrapper
    {
        protected override void PreInitialize()
        {

            DashboardApplication.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            base.PreInitialize();
        }

        protected override void PostInitialize()
        {
            LogDependencyInjectionRegistrations();
           
            base.PostInitialize();
        }

        protected override void SetupLogging()
        {
            SetupLogging(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects\\Logs\\ClearDashboard.log"));
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
           await ShowStartupDialog<StartupDialogViewModel, MainViewModel>();
        }

        #region Application exit
        protected override void OnExit(object sender, EventArgs e)
        {
            Logger?.LogInformation("ClearDashboard application is exiting.");
            base.OnExit(sender, e);
        }
        #endregion

    }
}
