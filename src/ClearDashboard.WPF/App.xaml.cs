using ClearDashboard.Wpf.ViewModels;
using ClearDashboard.Wpf.Views;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using ILogger = Serilog.ILogger;
using Settings = ClearDashboard.Wpf.Properties.Settings;

namespace ClearDashboard.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;
        public readonly ILogger _logger;
        public event Action ThemeChanged;



        private MaterialDesignThemes.Wpf.BaseTheme _theme;
        public MaterialDesignThemes.Wpf.BaseTheme Theme
        {
            get { return _theme; }
            set
            {
                _theme = value;
                OnThemeChanged();
            }
        }

        // trigger the change event
        private void OnThemeChanged()
        {
            ThemeChanged?.Invoke();
        }


        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            MainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();

            //set the light/dark
            SetTheme(Settings.Default.Theme);

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }
            _host.Dispose();

            base.OnExit(e);
        }

        /// <summary>
        /// Gets the current theme for the application from the app settings and sets them
        /// </summary>
        /// <param name="theme"></param>
        public void SetTheme(BaseTheme theme)
        {
            //Copied from the existing ThemeAssist class
            //https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/MaterialDesignThemes.Wpf/ThemeAssist.cs

            string lightSource = "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml";
            string darkSource = "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml";

            foreach (ResourceDictionary resourceDictionary in Resources.MergedDictionaries)
            {
                if (string.Equals(resourceDictionary.Source?.ToString(), lightSource, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(resourceDictionary.Source?.ToString(), darkSource, StringComparison.OrdinalIgnoreCase))
                {
                    Resources.MergedDictionaries.Remove(resourceDictionary);
                    break;
                }
            }

            if (theme == BaseTheme.Dark)
            {
                Resources.MergedDictionaries.Insert(0, new ResourceDictionary { Source = new Uri(darkSource) });
            }
            else
            {
                //This handles both Light and Inherit
                Resources.MergedDictionaries.Insert(0, new ResourceDictionary { Source = new Uri(lightSource) });
            }
        }

        public App()
        {
            // setup to catch global unhandled exceptions
            SetupUnhandledExceptionHandling();

            // setup dependency injection
            _host = Host.CreateDefaultBuilder()
                .UseSerilog((host, loggerConfiguration) =>
                {
                    loggerConfiguration.WriteTo.File("ClearDashboard.log", rollingInterval: RollingInterval.Day)
                        .WriteTo.Debug()
                        .MinimumLevel.Debug();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<MainWindowViewModel>();

                    services.AddSingleton<MainWindow>(s => new MainWindow()
                    {
                        DataContext = s.GetRequiredService<MainWindowViewModel>()
                    });

                    services.AddTransient<LandingViewModel>();

                    services.AddTransient<Landing>(s => new Landing()
                    {
                        DataContext = s.GetRequiredService<LandingViewModel>()
                    });
                })
                .Build();

            // connect up to an instance of the logger
            _logger = (ILogger)_host.Services.GetService(typeof(ILogger));
            _logger.Information("Configured Serilog");
        }

        /// <summary>
        /// Catch unhandled global exceptions
        /// </summary>
        private void SetupUnhandledExceptionHandling()
        {
            // Catch exceptions from all threads in the AppDomain.
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false);

            // Catch exceptions from each AppDomain that uses a task scheduler for async operations.
            TaskScheduler.UnobservedTaskException += (sender, args) =>
                ShowUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException", false);

            // Catch exceptions from a single specific UI dispatcher thread.
            Dispatcher.UnhandledException += (sender, args) =>
            {
                // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
                if (!Debugger.IsAttached)
                {
                    args.Handled = true;
                    ShowUnhandledException(args.Exception, "Dispatcher.UnhandledException", true);
                }
            };

            // Catch exceptions from the main UI dispatcher thread.
            // Typically we only need to catch this OR the Dispatcher.UnhandledException.
            // Handling both can result in the exception getting handled twice.
            Application.Current.DispatcherUnhandledException += (sender, args) =>
            {
                // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
                if (!Debugger.IsAttached)
                {
                    args.Handled = true;
                    ShowUnhandledException(args.Exception, "Application.Current.DispatcherUnhandledException", true);
                }
            };
        }

        /// <summary>
        /// Show the exception to the user
        /// </summary>
        /// <param name="e"></param>
        /// <param name="unhandledExceptionType"></param>
        /// <param name="promptUserForShutdown"></param>
        void ShowUnhandledException(Exception e, string unhandledExceptionType, bool promptUserForShutdown)
        {
            var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
            var messageBoxMessage = $"The following exception occurred:\n\n{e}";
            var messageBoxButtons = MessageBoxButton.OK;

            _logger.Error(e, messageBoxTitle, messageBoxMessage);

            if (promptUserForShutdown)
            {
                messageBoxMessage += "\n\nNormally the app would die now. Should we let it die?";
                messageBoxButtons = MessageBoxButton.YesNo;
            }

            // Let the user decide if the app should die or not (if applicable).
            if (MessageBox.Show(messageBoxMessage, messageBoxTitle, messageBoxButtons) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
