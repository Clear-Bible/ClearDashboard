using Autofac;
using Autofac.Extensions.DependencyInjection;
using Caliburn.Micro;
using ClearApplicationFoundation;
using ClearApplicationFoundation.Extensions;
using ClearDashboard.DAL.Alignment.BackgroundServices;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.Validators;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac.Configuration;
using ClearDashboard.Collaboration.Features;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.Configuration;
using DashboardApplication = System.Windows.Application;

namespace ClearDashboard.Wpf.Application
{
    internal class Bootstrapper : FoundationBootstrapper
    {
        private IHost _host;
        private bool _restoredMainWindowState = false;

        public Bootstrapper()
        {
            _host = CreateDenormalizationHost();
        }


        /// <summary>
        /// Creates an instance of IHost used for denormalizing project databases.
        /// </summary>
        /// <remarks>
        /// 
        ///     PLEASE READ!
        ///
        /// 
        ///     This DI setup is for setting up a DI container for denormalizing project databases. Please do not add 
        ///     or change any DI registrations here unless you know what you are doing!
        /// 
        ///     INSTEAD - add your DI registrations to ApplicationModule
        /// 
        /// </remarks>
        /// <returns></returns>
        private IHost CreateDenormalizationHost()
        {


            // Autofac container should already be built by call to base() constructor
            // (which calls Caliburn.Micro "Initialize", which calls FoundationBootstrapper
            // "configure").  So, 

           return Host.CreateDefaultBuilder()
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

        protected override void Configure()
        {
            //ConfigureKeyTriggerBindings();

            base.Configure();
        }

        //private static void ConfigureKeyTriggerBindings()
        //{
        //    var defaultCreateTrigger = Parser.CreateTrigger;

        //    Parser.CreateTrigger = (target, triggerText) =>
        //    {
        //        if (triggerText == null)
        //        {
        //            return defaultCreateTrigger(target, null);
        //        }

        //        var triggerDetail = triggerText
        //            .Replace("[", string.Empty)
        //            .Replace("]", string.Empty);

        //        var splits = triggerDetail.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        //        switch (splits[0])
        //        {
        //            case "Key":
        //                var key = (Key)Enum.Parse(typeof(Key), splits[1], true);
        //                return new KeyTrigger { Key = key };

        //            case "Gesture":
        //                if (splits.Length == 2)
        //                {
        //                    var mkg = (MultiKeyGesture)new MultiKeyGestureConverter().ConvertFrom(splits[1])!;
        //                    return new KeyTrigger { Modifiers = mkg.KeySequences[0].Modifiers, Key = mkg.KeySequences[0].Keys[0] };
        //                }

        //                return defaultCreateTrigger(target, triggerText);
        //        }

        //        return defaultCreateTrigger(target, triggerText);
        //    };
        //}

        protected override void PreInitialize()
        {
            DashboardApplication.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

#if DEBUG
            DependencyInjectionLogging = true;
#endif
            base.PreInitialize();

            // Set up DPI awareness
            //if (Environment.OSVersion.Version >= new Version(6, 3, 0)) // win 8.1 added support for per monitor dpi
            //{
            //    if (Environment.OSVersion.Version >= new Version(10, 0, 15063)) // win 10 creators update added support for per monitor v2
            //    {
            //        NativeMethods.SetProcessDpiAwarenessContext((int)NativeMethods.DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            //    }
            //    else NativeMethods.SetProcessDpiAwareness(NativeMethods.PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);
            //}
            //else NativeMethods.SetProcessDPIAware();
        }

        protected override void PostInitialize()
        {
            LogDependencyInjectionRegistrations();
            SetupLanguage();

#if DEBUG
            if (DependencyInjectionLogging)
            {
                DependencyInjectionLogging = false;
            }
#endif

            base.PostInitialize();
        }

        protected override void SetupLogging()
        {
            SetupLogging(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                "ClearDashboard_Projects\\Logs\\ClearDashboard.log"), 
                namespacesToExclude: new [] { "ClearDashboard.Wpf.Application.Services.NoteManager", "ClearDashboard.DAL.Alignment.BackgroundServices.AlignmentTargetTextDenormalizer" });
        }

        private void SetupLanguage()
        {
            var selectedLanguage = Settings.Default.language_code;
            if (string.IsNullOrEmpty(selectedLanguage))
            {
                var currentCulture = Thread.CurrentThread.CurrentCulture;
                var cultureName = currentCulture.Parent.Name is not "zh" or "pt" ? currentCulture.Parent.Name : currentCulture.Name;

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
                throw new NullReferenceException("'TranslationSource' needs to be registered with the DI container.");
            }
        }

        private ConfigurationModule configurationModule;
        protected override void PopulateServiceCollection(ServiceCollection serviceCollection)
        {
            serviceCollection.AddClearDashboardDataAccessLayer();
            serviceCollection.AddValidatorsFromAssemblyContaining<ProjectValidator>();

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddUserSecrets<Bootstrapper>();

            var config = configBuilder.Build();
            configurationModule = new ConfigurationModule(config);
            serviceCollection.AddSingleton<IConfiguration>(config);
            serviceCollection.AddSingleton<CollaborationConfiguration>(sp =>
            {
                var c = sp.GetService<IConfiguration>();
                var section = c.GetSection("Collaboration");

                int userId;
                int nameSpaceId;
                try
                {
                    userId = Convert.ToInt16(section["userId"]);
                    nameSpaceId = Convert.ToInt16(section["NamespaceId"]);
                }
                catch (Exception )
                {
                    userId = 2;
                    nameSpaceId = 0;
                }
                return new CollaborationConfiguration()
                {
                    RemoteUrl = section["RemoteUrl"],
                    RemoteEmail = section["RemoteEmail"],
                    RemoteUserName = section["RemoteUserName"],
                    RemotePersonalAccessToken = section["RemotePersonalAccessToken"],
                    Group  = section["Group"],
                    RemotePersonalPassword = section["RemotePersonalPassword"], 
                    UserId = userId,
                    NamespaceId = nameSpaceId
                };
            });

            base.PopulateServiceCollection(serviceCollection);
        }

        protected override void LoadModules(ContainerBuilder builder)
        {
            base.LoadModules(builder);
            builder.RegisterModule<ApplicationModule>();
            builder.RegisterModule<AbstractionsModule>();
            builder.RegisterModule(configurationModule);
            builder.RegisterType<CollaborationManager>().AsSelf().SingleInstance();
            builder.RegisterType<JiraClientServices>();

            builder
                .RegisterAssemblyTypes(typeof(GetProjectSnapshotQueryHandler).Assembly)
                .AsClosedTypesOf(typeof(IRequestHandler<,>));
            }

        protected override async Task NavigateToMainWindow()
        {
            //EnsureApplicationMainWindowVisible();
            //NavigateToViewModel<EnhancedViewDemoViewModel>();
            var mainWindow = DashboardApplication.Current.MainWindow;

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
                mainWindow.WindowState = applicationWindowState.WindowState;
            }
            base.RestoreMainWindowState();
            _restoredMainWindowState = true;
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            var assemblies = base.SelectAssemblies().ToList();

            assemblies.Add(typeof(AbstractionsModule).Assembly);

            return assemblies.LoadModuleAssemblies();
        }

        protected override void SaveMainWindowState()
        {
            var mainWindow = DashboardApplication.Current.MainWindow;
            if (mainWindow != null && _restoredMainWindowState)
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
            // remove extra Paratext Plugin for beta users
            RemoveExtraParatextPluginInstance();

            _host.StartAsync();

            Logger?.LogInformation("ClearDashboard application is starting.");

            Telemetry.TrackEvent("ClearDashboard application is starting.");
            Telemetry.StartStopwatch(Telemetry.TelemetryDictionaryKeys.AppHours);
            
            base.OnStartup(sender, e);
        }

        /// <summary>
        /// For beta users starting with ver 0.4.7, a second entry was accidentally made in the
        /// installer that has the plugin directory name of "Clear Suite" instead of "ClearDashboardWebApiPlugin"
        /// This removes that previous instance
        /// </summary>
        private void RemoveExtraParatextPluginInstance()
        {
            ParatextProxy paratextUtils = new ParatextProxy(null);
            string paratextInstallPath = string.Empty;
            if (paratextUtils.IsParatextInstalled())
            {
                if (paratextUtils.ParatextInstallPath != string.Empty)
                {
                    paratextInstallPath = paratextUtils.ParatextInstallPath;
                }
                else if (paratextUtils.ParatextBetaInstallPath != string.Empty)
                {
                    paratextInstallPath = paratextUtils.ParatextBetaInstallPath;
                }

                
                paratextInstallPath = Path.Combine(paratextInstallPath, "plugins", "Clear Dashboard");
                if (Directory.Exists(paratextInstallPath))
                {
                    try
                    {
                        var dir = new DirectoryInfo(paratextInstallPath);
                        dir.Delete(true);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Old Paratext Plugin Found: " + e.Message);
                    }
                }
            }
        }

        #region Application exit
        protected override void OnExit(object sender, EventArgs e)
        {
            Logger?.LogInformation("ClearDashboard application is exiting.");

            Telemetry.SendFullReport("ClearDashboard application is exiting.");
#if RELEASE
            Telemetry.Flush();
            Task.Delay(5000).Wait();
#endif
            StopParatext();

            StopAndDisposeHost();

            StopTailBlazer();

            DisposeLifetimeScope();

            Logger.LogInformation($"Bootstrapper.OnExit hit.");
            CheckToInstallAqua();

            base.OnExit(sender, e);
        }

        private void StopTailBlazer()
        {
            if (Container!.Resolve<TailBlazerProxy>() is { } tailBlazerProxy)
            {
                tailBlazerProxy.StopTailBlazer();
            }
        }

        private void StopParatext()
        {
            #if DEBUG
            if (Container!.Resolve<ParatextProxy>() is { } paratextProxy)
            {
                paratextProxy.StopParatext();
            }
            #endif
        }

        private void StopAndDisposeHost()
        {
            _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
        }

        private void DisposeLifetimeScope()
        {
            Logger?.LogInformation("Disposing ILifetimeScope");
            var lifetimeScope = Container!.Resolve<ILifetimeScope>();
            lifetimeScope.Dispose();
        }

        private void CheckToInstallAqua()
        {
            Logger.LogInformation($"Bootstrapper.CheckToInstallAqua hit.");
            Logger.LogInformation($"Settings.Default.RunAquaInstall is: " + Settings.Default.RunAquaInstall);
            if (Settings.Default.RunAquaInstall)
            {
                Logger.LogInformation($"RunAquaInstall was true so continuing...");
                Settings.Default.RunAquaInstall = false;
                Settings.Default.Save();
                Logger.LogInformation($"Settings.Default.RunAquaInstall is: " + Settings.Default.RunAquaInstall);

                var startupPath = Environment.CurrentDirectory;
                Logger.LogInformation($"Dashboard Startup Path: {startupPath}");

                var filename = Path.Combine(startupPath, "PluginManager.exe");
                Logger.LogInformation($"Full PluginManager FilePath: {filename}");

                if (File.Exists(filename))
                {
                    Logger.LogInformation($"The Full FilePath existed.");
                    var psi = new ProcessStartInfo();
                    psi.FileName = filename;
                    psi.UseShellExecute = true;
                    psi.Verb = "runas"; //This is what actually runs the command as administrator
                    psi.WorkingDirectory = startupPath;
                    try
                    {
                        Logger.LogInformation($"Entered Try block.");
                        var process = new Process();
                        process.StartInfo = psi;
                        process.Start();
                        Logger.LogInformation($"Process Started.");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogInformation($"In Catch, Process Failed or was denied admin privileges: " + ex);
                        //If you are here the user clicked decline to grant admin privileges (or he's not administrator)
                    }
                }
                else
                {
                    Logger.LogInformation($"The Full FilePath did not exist.");
                }

            }
        }

        #endregion

    }
}
