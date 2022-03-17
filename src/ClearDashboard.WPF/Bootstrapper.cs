using Caliburn.Micro;
using ClearDashboard.Wpf.Properties;
using ClearDashboard.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ClearDashboard.DataAccessLayer.Context;
using Microsoft.Extensions.Configuration;
using ClearDashboard.DataAccessLayer.Extensions;

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
            EnsureDatabase();

            Initialize();

            //set the light/dark
            ((App)Application.Current).SetTheme(Settings.Default.Theme);
        }

        private void EnsureDatabase()
        {
            // Ask for the database context.  This will create the database
            // and apply migrations if required.
            _ = Host.Services.GetService<AlignmentContext>();
        }

        #endregion

        #region Configure
        protected  void ConfigureServices(IServiceCollection serviceCollection)
        {

            serviceCollection.AddAlignmentDatabase("alignment.sqlite");

            // wire up the interfaces required by Caliburn.Micro
            serviceCollection.AddSingleton<IWindowManager, WindowManager>();
            serviceCollection.AddSingleton<IEventAggregator, EventAggregator>();

            // Register the FrameAdapter which wraps a Frame as INavigationService
            FrameSet = new FrameSet();
            serviceCollection.AddSingleton<INavigationService>(sp=> FrameSet.NavigationService);
            
            // wire up all of the view models in the project.
            GetType().Assembly.GetTypes()
                .Where(type => type.IsClass)
                .Where(type => type.Name.EndsWith("ViewModel"))
                .ToList()
                .ForEach(viewModelType => serviceCollection.AddScoped(viewModelType));

            serviceCollection.AddLogging();

            
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

            var fullPath = Path.Combine(Environment.CurrentDirectory, "Logs\\ClearDashboard.log");
            var level = LogEventLevel.Information;
#if DEBUG
            level = LogEventLevel.Verbose;
#endif
            var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}";

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

            // write to the log file
            Logger.LogError(e.Exception, "An unhandled error as occurred");
            MessageBox.Show(e.Exception.Message, "An error as occurred", MessageBoxButton.OK);
        }
        #endregion 
    }
}
