using Caliburn.Micro;
using ClearDashboard.Wpf.Properties;
using ClearDashboard.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace ClearDashboard.Wpf
{
    public class Bootstrapper : BootstrapperBase
    {
        #region Properties

        private FrameSet _frameSet;
        private Log _log;
        private SimpleContainer _container;

        protected IServiceCollection ServiceCollection { get; private set; }
        protected IServiceProvider ServiceProvider { get; private set; }

        #endregion

        #region Contructor

        public Bootstrapper()
        {
            Initialize();

            //set the light/dark
            ((App)Application.Current).SetTheme(Settings.Default.Theme);
        }

        #endregion

        #region Configure
        protected override void Configure()
        {
            _log = new Log();
            _log.Info("Configuring dependency injection for the application.");
            // put a reference into the App
            ((App)Application.Current).Log = _log;

            
            
            ServiceCollection = new ServiceCollection();

            // register the instance of ILog with the container.
            ServiceCollection.AddSingleton<ILog>(sp => _log);


            // wire up the interfaces required by Caliburn.Micro
            ServiceCollection.AddSingleton<IWindowManager, WindowManager>();
            ServiceCollection.AddSingleton<IEventAggregator, EventAggregator>();

            // Register the FrameAdapter which wraps a Frame as INavigationService
            _frameSet = new FrameSet();
            ServiceCollection.AddSingleton<INavigationService>(sp=> _frameSet.NavigationService);


            // wire up all of the view models in the project.
            GetType().Assembly.GetTypes()
                .Where(type => type.IsClass)
                .Where(type => type.Name.EndsWith("ViewModel"))
                .ToList()
                .ForEach(viewModelType => ServiceCollection.AddScoped(viewModelType));


            ServiceProvider = ServiceCollection.BuildServiceProvider();
            //SetupLogging();
        }

        #endregion

        #region Startup
        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            // Allow the ShellView to be created.
            await DisplayRootViewForAsync<ShellViewModel>();

            // Now add the Frame to be added to the Grid in ShellView
            AddFrameToMainWindow(_frameSet.Frame);

            // Navigate to the LandingView.
            _frameSet.NavigationService.NavigateToViewModel(typeof(LandingViewModel));
        }

        private void AddFrameToMainWindow(Frame frame)
        {
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
            return ServiceProvider.GetService(service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return ServiceProvider.GetServices(service);
        }

        protected override void BuildUp(object instance)
        {
            //no-op
        }

        #endregion

        #region Logging
        private void SetupLogging()
        {

            var fullPath = $"\\Logs\\ClearDashboard.log";
            Console.WriteLine($"Log file located: {fullPath}");
            var level = LogEventLevel.Information;
#if DEBUG
            level = LogEventLevel.Verbose;
#endif

            var log = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.File(@"ClearDashboard.log", rollingInterval: RollingInterval.Day)
                //.WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}", restrictedToMinimumLevel: level)
                .CreateLogger();
            var loggerFactory = ServiceProvider.GetService<ILoggerFactory>();
            loggerFactory.AddSerilog(log);
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
            _log.Error(e.Exception);
            MessageBox.Show(e.Exception.Message, "An error as occurred", MessageBoxButton.OK);
        }
        #endregion 
    }
}
