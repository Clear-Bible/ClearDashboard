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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Wpf.Application.ViewModels.Display;
using Microsoft.Extensions.Hosting;
using DashboardApplication = System.Windows.Application;
using Autofac.Extensions.DependencyInjection;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using Caliburn.Micro;
using MediatR;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Wpf;
using System.Net;
using SIL.Machine.Utils;
using ClearDashboard.DAL.Alignment.Features.Denormalization;
using ClearDashboard.DataAccessLayer.Models;
using System.Data.Common;
using System.Xml.Linq;
using ClearDashboard.DAL.Alignment.BackgroundServices;
using Autofac.Core.Lifetime;

namespace ClearDashboard.Wpf.Application
{
    internal class Bootstrapper : FoundationBootstrapper
    {
        private readonly IHost _host;
        public Bootstrapper() : base()
        {
            // Autofac container should already be built by call to base() constructor
            // (which calls Caliburn.Micro "Initialize", which calls FoundationBootstrapper
            // "configure").  So, 

            _host = Host.CreateDefaultBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureServices(services =>
                {
                    services.Configure<HostOptions>(options =>
                    {
                        //Service Behavior in case of exceptions - defautls to StopHost
//                        options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                        //Host will try to wait 30 seconds before stopping the service. 
                        options.ShutdownTimeout = TimeSpan.FromSeconds(5);
                    });
                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder
                        .RegisterType<AlignmentTargetTextDenormalizer>()
                        .As<IHostedService>()
                        // Passing this in so that the hosted service can subscribe to its
                        // CurrentScopeEnding event and avoid using any shared registrations
                        // after that event fires.  
                        .WithParameter(new TypedParameter(typeof(ILifetimeScope), Container!))
                        .SingleInstance();

                    builder.RegisterInstance(Container!.Resolve<IEventAggregator>()).ExternallyOwned();
                    builder.RegisterInstance(Container!.Resolve<IMediator>()).ExternallyOwned();
                    builder.RegisterInstance(Container!.Resolve<IProjectProvider>()).ExternallyOwned();
                    builder.RegisterInstance(Container!.Resolve<ILoggerFactory>()).SingleInstance().ExternallyOwned();
                    builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
                    builder.RegisterDatabaseDependencies();

                    // TODO:  localize ProcessName value:
                    builder.RegisterType<BackgroundServiceDelegateProgress<AlignmentTargetTextDenormalizer>>().WithProperty(
                        "ProcessName",
                        ClearDashboard.DAL.Alignment.Features.Denormalization.LocalizationStrings.Get(
                            "Denormalization_AlignmentTopTargets_BackgroundTaskName", 
                            Container!.Resolve<ILogger<Bootstrapper>>()));
                })
                .Build();
        }

        protected override void PreInitialize()
        {
            DashboardApplication.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

#if DEBUG
            DependencyInjectionLogging = true;
#endif
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
                var cultureName = "";
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                if (currentCulture.Parent.Name is not "zh" or "pt")
                {
                    cultureName = currentCulture.Parent.Name;
                }
                else
                {
                    cultureName = currentCulture.Name;
                }

                try
                {
                    selectedLanguage = ((LanguageTypeValue)Enum.Parse(typeof(LanguageTypeValue),cultureName.Replace("-", string.Empty))).ToString();
                }
                catch
                {
                    selectedLanguage = "en";
                }
            }

            if (selectedLanguage == "enUS")
            {
                selectedLanguage = "en";
            }

            Settings.Default.language_code = selectedLanguage;
            Settings.Default.Save();
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
            //EnsureApplicationMainWindowVisible();
            //NavigateToViewModel<EnhancedViewDemoViewModel>();
            
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

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            _host.StartAsync();

            Logger?.LogInformation("ClearDashboard application is starting.");
            base.OnStartup(sender, e);
        }

        #region Application exit
        protected override void OnExit(object sender, EventArgs e)
        {
            _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();

            Logger?.LogInformation("ClearDashboard application is exiting.");
            base.OnExit(sender, e);
        }
        #endregion

    }
}
