using Autofac;
using ClearApplicationFoundation;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
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
using ClearDashboard.Wpf.Application.ViewModels.Display;
using Microsoft.Extensions.Hosting;
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
            SetupLanguage();

            base.PostInitialize();
        }

        protected override void SetupLogging()
        {
            SetupLogging(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects\\Logs\\ClearDashboard.log"));
        }

        private void SetupLanguage()
        {
            var selectedLanguage = Settings.Default.language_code;
            if (string.IsNullOrEmpty(selectedLanguage))
            {
                selectedLanguage = "enUS";
            }

            var languageType = (LanguageTypeValue)Enum.Parse(typeof(LanguageTypeValue), selectedLanguage.Replace("-", string.Empty));
            var translationSource = Container?.Resolve<TranslationSource>();
            if (translationSource != null)
            {
                translationSource.Language = EnumHelper.GetDescription(languageType);
            }
            else
            {
                throw new NullReferenceException("'TranslationSource' needs to registered with the DI container.");
            }
        }

        protected override void PopulateServiceCollection(ServiceCollection serviceCollection)
        {
            serviceCollection.AddClearDashboardDataAccessLayer(registerDatabaseAbstractions:false);
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

        protected override void RestoreMainWindowState()
        {
            var applicationWindowState = new ApplicationWindowState();
            var mainWindow = DashboardApplication.Current.MainWindow;
            if (mainWindow != null)
            {
                // NB:  Do not restore the WindowState here -- it will cause the window 
                // to not be displayed until AvalonDock has restored the dockable window
                // layout for the MainView.
                mainWindow.Height = applicationWindowState.WindowHeight;
                mainWindow.Width = applicationWindowState.WindowWidth;
                mainWindow.Top = applicationWindowState.WindowTop;
                mainWindow.Left = applicationWindowState.WindowLeft;
            }
            base.RestoreMainWindowState();
        }

        protected override void SaveMainWindowState()
        {
            var mainWindow = DashboardApplication.Current.MainWindow;
            if (mainWindow != null)
            {
                var applicationWindowState = new ApplicationWindowState
                {
                    WindowHeight = mainWindow.Height,
                    WindowWidth = mainWindow.Width,
                    WindowTop = mainWindow.Top,
                    WindowLeft = mainWindow.Left,
                    WindowState = mainWindow.WindowState
                };
                applicationWindowState.Save();
            }
            base.SaveMainWindowState();
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
