﻿using Caliburn.Micro;
using ClearDashboard.Wpf.Extensions;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Models;
using ClearDashboard.Wpf.Properties;
using ClearDashboard.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using ClearDashboard.Wpf.Validators;
using FluentValidation;
using SIL.Extensions;


namespace ClearDashboard.Wpf
{
    public class Bootstrapper : BootstrapperBase
    {
        #region Properties

        protected FrameSet FrameSet { get; private set; }
        public static IHost Host { get; private set; }
        protected ILogger<Bootstrapper> Logger { get; private set; }

        
        #endregion

        #region Contructor

        public Bootstrapper()
        {
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();

            SetupLogging();
            SetupLanguage();
            Initialize();

            //set the light/dark
            ((App)Application.Current).SetTheme(Settings.Default.Theme);
        }

        #endregion

        #region Configure
        protected  void ConfigureServices(IServiceCollection serviceCollection)
        {
            FrameSet = serviceCollection.AddCaliburnMicro();
            serviceCollection.AddClearDashboardDataAccessLayer();
            serviceCollection.AddValidatorsFromAssemblyContaining<ProjectValidator>();
            //serviceCollection.AddScoped<FluentValidation.IValidator<Project>, ProjectValidator>();
            serviceCollection.AddLogging();
            serviceCollection.AddLocalization();
        }

        #endregion

        #region Startup
        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            Logger.LogInformation("ClearDashboard application is starting.");

            // Allow the ShellView to be created.
            await DisplayRootViewForAsync<ShellViewModel>();

            // Now add the Frame to be added to the Grid in ShellView
            AddFrameToMainWindow(FrameSet.Frame);

            // Navigate to the LandingView.
            FrameSet.NavigationService.NavigateToViewModel(typeof(LandingViewModel));
        }

        private static void SetupLanguage()
        {
            var selectedLanguage = CultureInfo.CurrentCulture.Name;

            LanguageTypeValue languageType;
            try
            {
                languageType = (LanguageTypeValue)Enum.Parse(typeof(LanguageTypeValue), selectedLanguage.Replace("-", string.Empty));
                Settings.Default.language_code = selectedLanguage.Replace("-",string.Empty);
                Settings.Default.Save();
            }
            catch (Exception)
            {
                languageType = LanguageTypeValue.enUS;
            }

            var translationSource = Host.Services.GetService<TranslationSource>();
            if (translationSource != null)
            {
                translationSource.Language = EnumHelper.GetDescription(languageType);
            }
            else
            {
                throw new NullReferenceException("'TranslationSource' needs to registered with the DI container.");
            }
        }

      

        /// <summary>
        /// Adds the Frame to the Grid control on the ShellView
        /// </summary>
        /// <param name="frame"></param>
        /// <exception cref="NullReferenceException"></exception>
        private void AddFrameToMainWindow(Frame frame)
        {
            Logger.LogInformation("Adding Frame to ShellView grid control.");

            var mainWindow = Application.MainWindow;
            if (mainWindow == null)
            {
                throw new NullReferenceException("'Application.MainWindow' is null.");
            }


            if (mainWindow.Content is not Grid grid)
            {
                throw new NullReferenceException("The grid on 'Application.MainWindow' is null.");
            }

            Grid.SetRow(frame, 1);
            Grid.SetColumn(frame, 0);
            Panel.SetZIndex(frame, 0);
            grid.Children.Add(frame);
        }

        #endregion

        #region DependencyInjection

        protected override object GetInstance(Type service, string key)
        {
            return Host.Services.GetService(service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return Host.Services.GetServices(service);
        }
        #endregion

        #region Logging
        private void SetupLogging()
        {
            var fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects\\Logs\\ClearDashboard.log");
            //create if does not exist
            DirectoryInfo di = new DirectoryInfo(fullPath);
            if (di.Exists == false)
            {
                di.Create();
            }

            var level = LogEventLevel.Information;
#if DEBUG
            level = LogEventLevel.Verbose;
#endif
            const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}";

            var log = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.File(fullPath, outputTemplate: outputTemplate, rollingInterval: RollingInterval.Day)
                .WriteTo.Debug(outputTemplate: outputTemplate)
                .CreateLogger();

            var loggerFactory = Host.Services.GetService<ILoggerFactory>();
            loggerFactory.AddSerilog(log);

            Logger = Host.Services.GetService<ILogger<Bootstrapper>>();
        }
        #endregion

        #region Application exit
        protected override void OnExit(object sender, EventArgs e)
        {
            Logger.LogInformation("ClearDashboard application is exiting.");
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

            Logger.LogError(e.Exception, "An unhandled error as occurred");
            MessageBox.Show(e.Exception.Message, "An error as occurred", MessageBoxButton.OK);
        }
        #endregion 
    }
}
