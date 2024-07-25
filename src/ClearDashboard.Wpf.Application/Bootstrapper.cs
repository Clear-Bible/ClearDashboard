using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.AttributeFilters;
using MessagePack;
using MessagePack.Resolvers;
using Caliburn.Micro;
using ClearApi.Command.Alignment.JsonRpc.DataTransfer.Converters;
using ClearApi.Command.CQRS.CommandReceivers;
using ClearApi.Command.CQRS.Commands;
using ClearApi.Command.CQRS.JsonRpc.CommandReceiverProxy;
using ClearApi.Command.CommandReceivers;
using ClearApi.Command.Commands;
using ClearApi.Command.JsonRpc.CommandReceiverProxy;
using ClearApi.Command.JsonRpc.Network;
using ClearApi.Command.JsonRpc.Serialization;
using ClearApi.Command.Serialization;
using ClearApi.DataTransfer.Utils;
using ClearApi.Engine.Model;
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
using ClearDashboard.Wpf.Application.Localization;
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
        }

        protected override void PostInitialize()
        {
            LogDependencyInjectionRegistrations();

            SetupLanguage();

            StaticLocalizationService.SetLocalizationService(Container!.Resolve<ILocalizationService>());

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
                    NamespaceId = nameSpaceId,
                    TokenId = section["TokenId"] is not null ? Convert.ToInt16(section["TokenId"]) : 0
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

            BootstrapCommandsLocalOnly.LoadModules(builder);
            return;

            builder.RegisterType<ClearEngineClientWebSocket>()
                .WithParameter("host", "192.168.1.50:5173") // Home
                                                            //.WithParameter("host", "172.20.10.5:5173")  // Cell hotspot
                                                            //.WithParameter("host", "10.1.10.157:5173")  // TKD
                .WithParameter("path", "/ws-ces")
                .AsSelf();

            builder.RegisterGeneric(typeof(JsonRpcProxy<>))
                .WithParameter(
                    (pi, ctx) => pi.ParameterType == typeof(MessagePackSerializerOptions) && pi.Name == "serializerOptions",
                    (pi, ctx) => ContractlessStandardResolver.Options)
                .As(typeof(IJsonRpcProxy<>))
                .Keyed(nameof(ContractlessStandardResolver), typeof(IJsonRpcProxy<>))
                .SingleInstance();

            builder.RegisterGeneric(typeof(JsonRpcProxy<>))
                .WithParameter(
                    (pi, ctx) => pi.ParameterType == typeof(MessagePackSerializerOptions) && pi.Name == "serializerOptions",
                    (pi, ctx) => MessagePackSerializerOptions.Standard)
                .As(typeof(IJsonRpcProxy<>))
                .Keyed(nameof(StandardResolver), typeof(IJsonRpcProxy<>))
                .SingleInstance();

            // Setting useCurrentProject to true will cause the dashboard project id to
            // be passed in ProjectCommand execute calls.  Using false tells the server
            // to use its own default project id (we're doing that for starters since we
            // don't have any initial project creation/selection functionality integrated)
            builder.RegisterType<CurrentProjectContextProvider>()
                .WithParameter("useCurrentProject", false)
                .As<IProjectCommandContextProvider>();

            builder.RegisterType<DataTransferConverter>()
                .WithParameter(
                    (pi, ctx) => pi.ParameterType == typeof(Func<System.Type, bool>),
                    (pi, ctx) => (Func<System.Type, bool>)((type) => !DynamicCommandSerializer.IsMessagePackSupportedType(type))
                )
                .WithParameter("dataTransferModelNamespace", typeof(ClearApi.DataTransfer.MessagePack.Model.DynamicRequest).Namespace!)
                .WithParameter(
                    (pi, ctx) => pi.ParameterType == typeof(string[]) && pi.Name == "extensionNamespaces",
                    (pi, ctx) => new string[] {
                        typeof(ClearApi.Command.DataTransfer.IAlignmentExtensions).Namespace!, 					// ToEngine extensions (data transfer interface to engine model)
					    typeof(ClearApi.Command.Alignment.DataTransfer.IAlignmentSetExtensions).Namespace!,		// ToEngine extensions (data transfer interface to engine model)
					    typeof(ClearApi.Command.JsonRpc.DataTransfer.NonGenericExtensions).Namespace!, 			// ToDataTransfer extensions (engine model to JsonRpc/MessagePack data transfer model)
					    typeof(ClearApi.Command.Alignment.JsonRpc.DataTransfer.NonGenericExtensions).Namespace! // ToDataTransfer extensions (engine model to JsonRpc/MessagePack data transfer model)
					})
                .WithParameter(
                    (pi, ctx) => pi.ParameterType == typeof(DataTransferConverter.AnonymousType) && pi.Name == "anonymousEngineType",
                    (pi, ctx) => DataTransferConverter.AnonymousType.ExpandoObject
                )
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<DataTransferTypeConstructor>().AsSelf().SingleInstance();
            builder.RegisterType<DynamicCommandSerializer>().As<IDynamicCommandSerializer>();
            builder.RegisterType<DynamicCommandReceiverProxy>()
                .WithParameter(
                    (pi, ctx) => pi.ParameterType == typeof(ICommandReceiver<DynamicCommand, DynamicCommandResult>) && pi.Name == "nextReceiver",
                    (pi, ctx) => null)
                .AsSelf()   // Required, if we are registering MediatorCommandReceiverProxy
                .As<ICommandReceiver<DynamicCommand, DynamicCommandResult>>()
                .WithAttributeFiltering();

            builder.RegisterType<TokenizedTextCorpusDataTransferConverter>().As<IDataTransferConverter<ClearDashboard.DAL.Alignment.Corpora.TokenizedTextCorpus>>();
            builder.RegisterType<ParallelCorpusDataTransferConverter>().As<IDataTransferConverter<ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus>>();
            builder.RegisterType<AlignmentSetDataTransferConverter>().As<IDataTransferConverter<ClearDashboard.DAL.Alignment.Translation.AlignmentSet>>();
            builder.RegisterType<TranslationSetDataTransferConverter>().As<IDataTransferConverter<ClearDashboard.DAL.Alignment.Translation.TranslationSet>>();

            //// LOCAL GetVerseRangeTokensCommandReceiver:
            //builder.RegisterType<GetVerseRangeTokensCommandReceiver>()
            //	.Named<ICommandReceiver<GetVerseRangeTokensCommand, (IEnumerable<PaddedTokensTextRow> Rows, int IndexOfVerse)>>(serviceName: "LOCAL");

            //         // REMOTE GetVerseRangeTokensCommandReceiver:
            //         builder.RegisterType<ProjectCommandReceiverProxy<GetVerseRangeTokensCommand, (IEnumerable<PaddedTokensTextRow> Rows, int IndexOfVerse)>>()
            //             .Named<ICommandReceiver<GetVerseRangeTokensCommand, (IEnumerable<PaddedTokensTextRow> Rows, int IndexOfVerse)>>("REMOTE")
            //	.WithAttributeFiltering();

            Func<ParameterInfo, IComponentContext, bool> nullNextReceiverParameterSelector =
                (pi, ctx) => pi.ParameterType == typeof(ICommandReceiver<DynamicCommand, DynamicCommandResult>) && pi.Name == "nextReceiver";
            Func<ParameterInfo, IComponentContext, object?> nullNextReceiverValueSelector = (pi, ctx) => null;

            // Just for fun, focus on remote calls only:
            builder.RegisterGeneric(typeof(MediatorCommandQueryReceiverProxy<,>))
                .WithParameter(
                    (pi, ctx) => pi.ParameterType.IsAssignableToGenericType(typeof(IMediatorCommandReceiver<,>)) && pi.Name == "nextReceiver",
                    (pi, ctx) => null)  // If we don't explicitly set this, circular dependency error
                .As(typeof(IMediatorCommandReceiver<,>));
            builder.RegisterGeneric(typeof(MediatorCommandCommandReceiverProxy<,>))
                .WithParameter(
                    (pi, ctx) => pi.ParameterType.IsAssignableToGenericType(typeof(IMediatorCommandReceiver<,>)) && pi.Name == "nextReceiver",
                    (pi, ctx) => null)  // If we don't explicitly set this, circular dependency error
                .As(typeof(IMediatorCommandReceiver<,>));

            builder.RegisterGeneric(typeof(ProjectMediatorCommandQueryReceiverProxy<,>))
                .WithParameter(
                    (pi, ctx) => pi.ParameterType.IsAssignableToGenericType(typeof(IProjectMediatorCommandReceiver<,>)) && pi.Name == "nextReceiver",
                    (pi, ctx) => null)  // If we don't explicitly set this, circular dependency error
                .As(typeof(IProjectMediatorCommandReceiver<,>))
                .WithAttributeFiltering();
            builder.RegisterGeneric(typeof(ProjectMediatorCommandCommandReceiverProxy<,>))
                .WithParameter(
                    (pi, ctx) => pi.ParameterType.IsAssignableToGenericType(typeof(IProjectMediatorCommandReceiver<,>)) && pi.Name == "nextReceiver",
                    (pi, ctx) => null)  // If we don't explicitly set this, circular dependency error
                .As(typeof(IProjectMediatorCommandReceiver<,>))
                .WithAttributeFiltering();

            builder.RegisterGeneric(typeof(ProjectCommandReceiverProxy<,>))
                .WithParameter(
                    (pi, ctx) => pi.ParameterType.IsAssignableToGenericType(typeof(IProjectCommandReceiver<,>)) && pi.Name == "nextReceiver",
                    (pi, ctx) => null)  // If we don't explicitly set this, circular dependency error
                .As(typeof(IProjectCommandReceiver<,>))
                .WithAttributeFiltering();

            builder.RegisterType<TokenizeTextCorpusCommandReceiver>();
            builder.RegisterType<TokenizeTextCorpusCommandReceiverProxy>()
                .WithParameter(
                    (pi, ctx) => pi.ParameterType == typeof(ICommandReceiver<TokenizeTextCorpusCommand, TokensTextCorpus>) && pi.Name == "nextReceiver",
                    (pi, ctx) => ctx.Resolve<TokenizeTextCorpusCommandReceiver>())
                .As<ICommandReceiver<TokenizeTextCorpusCommand, TokensTextCorpus>>()
                .WithAttributeFiltering();

            builder.RegisterType<AlignTokenizedCorporaCommandReceiver>();
            builder.RegisterType<AlignTokenizedCorporaCommandReceiverProxy>()
                .WithParameter(
                    (pi, ctx) => pi.ParameterType == typeof(ICommandReceiver<AlignTokenizedCorporaCommand, TokensAlignment>) && pi.Name == "nextReceiver",
                    (pi, ctx) => ctx.Resolve<AlignTokenizedCorporaCommandReceiver>())
                .As<ICommandReceiver<AlignTokenizedCorporaCommand, TokensAlignment>>()
                .WithAttributeFiltering();
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
