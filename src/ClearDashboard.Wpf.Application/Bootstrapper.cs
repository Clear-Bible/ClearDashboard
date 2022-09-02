using Autofac;
using ClearApplicationFoundation;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using ClearDashboard.Wpf.Application.Validators;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.Validators;
using ClearDashboard.Wpf.Application.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using ClearApplicationFoundation.Framework;
using ClearApplicationFoundation.ViewModels.Shell;
using Microsoft.Extensions.Logging;
using System.Windows.Controls;
using System;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
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
            var r = Container!.ComponentRegistry.Registrations;
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
            //serviceCollection.AddValidatorsFromAssemblyContaining<AddParatextCorpusDialogViewModelValidator>();

            base.PopulateServiceCollection(serviceCollection);
        }


        
        protected override void LoadModules(ContainerBuilder builder)
        {
            base.LoadModules(builder);
            builder.RegisterModule<ApplicationModule>();
        }

        protected override async Task NavigateToMainWindow()
        {
            //EnsureApplicationMainWindowVisible();
            //NavigateToViewModel<MainViewModel>();
            //NavigateToViewModel<ProjectPickerViewModel>();
            // await base.NavigateToMainWindow();
            // Show the StartupViewModel as a dialog, then navigate to HomeViewModel
            // if the dialog result is "true"
            await ShowStartupDialog<StartupDialogViewModel, MainViewModel>();
            //await ShowStartupDialog<ProjectPickerViewModel, ProjectSetupViewModel>();            
        }

        #region Application exit
        protected override void OnExit(object sender, EventArgs e)
        {
            Logger?.LogInformation("ClearDashboard application is exiting.");
            base.OnExit(sender, e);
        }
        #endregion

        #region Global error handling
        /// <summary>
        /// Handle the system wide exceptions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            Logger?.LogError(e.Exception, "An unhandled error as occurred");
            MessageBox.Show(e.Exception.Message, "An error as occurred", MessageBoxButton.OK);
        }
        #endregion



    }
}
