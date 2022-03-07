using Caliburn.Micro;
using ClearDashboard.Wpf.Properties;
using ClearDashboard.Wpf.ViewModels;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Action = System.Action;

namespace ClearDashboard.Wpf
{
    public class Bootstrapper : BootstrapperBase
    {
        #region Props

        private SimpleContainer container;

        // Theme related
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

        #endregion

        #region Startup

        public Bootstrapper()
        {
            Initialize();


            //set the light/dark
            SetTheme(Settings.Default.Theme);

            // setup to catch global unhandled exceptions
            //SetupUnhandledExceptionHandling();
        }

        #endregion

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

            foreach (ResourceDictionary resourceDictionary in Application.Resources.MergedDictionaries)
            {
                if (string.Equals(resourceDictionary.Source?.ToString(), lightSource, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(resourceDictionary.Source?.ToString(), darkSource, StringComparison.OrdinalIgnoreCase))
                {
                    Application.Resources.MergedDictionaries.Remove(resourceDictionary);
                    break;
                }
            }

            if (theme == BaseTheme.Dark)
            {
                Application.Resources.MergedDictionaries.Insert(0, new ResourceDictionary { Source = new Uri(darkSource) });
            }
            else
            {
                //This handles both Light and Inherit
                Application.Resources.MergedDictionaries.Insert(0, new ResourceDictionary { Source = new Uri(lightSource) });
            }
        }


        protected override void Configure()
        {
            //container = new SimpleContainer();

            //container.Instance(container);

            //container
            //    .Singleton<IWindowManager, WindowManager>()
            //    .Singleton<IEventAggregator, EventAggregator>();

            //container
            //    .PerRequest<ShellViewModel>()
            //    .PerRequest<MenuViewModel>()
            //    .PerRequest<BindingsViewModel>()
            //    .PerRequest<ActionsViewModel>()
            //    .PerRequest<CoroutineViewModel>()
            //    .PerRequest<ExecuteViewModel>()
            //    .PerRequest<EventAggregationViewModel>()
            //    .PerRequest<DesignTimeViewModel>()
            //    .PerRequest<ConductorViewModel>()
            //    .PerRequest<BubblingViewModel>()
            //    .PerRequest<NavigationSourceViewModel>()
            //    .PerRequest<NavigationTargetViewModel>();
        }

        //protected override IEnumerable<Assembly> SelectAssemblies()
        //{
        //    var assemblies = base.SelectAssemblies().ToList();
        //    assemblies.Add(typeof(ClearDashboard.Wpf.Views.MainWindowView ).Assembly);

        //    return assemblies;
        //}

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            await DisplayRootViewForAsync<MainWindowViewModel>();
        }

        //protected override object GetInstance(Type service, string key)
        //{
        //    return container.GetInstance(service, key);
        //}

        //protected override IEnumerable<object> GetAllInstances(Type service)
        //{
        //    return container.GetAllInstances(service);
        //}

        //protected override void BuildUp(object instance)
        //{
        //    container.BuildUp(instance);
        //}

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show(e.Exception.Message, "An error as occurred", MessageBoxButton.OK);
        }

        ///// <summary>
        ///// Catch unhandled global exceptions
        ///// </summary>
        //private void SetupUnhandledExceptionHandling()
        //{
        //    // Catch exceptions from all threads in the AppDomain.
        //    AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        //        ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false);

        //    // Catch exceptions from each AppDomain that uses a task scheduler for async operations.
        //    TaskScheduler.UnobservedTaskException += (sender, args) =>
        //        ShowUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException", false);

        //    // Catch exceptions from a single specific UI dispatcher thread.
        //    Dispatcher.UnhandledException += (sender, args) =>
        //    {
        //        // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
        //        if (!Debugger.IsAttached)
        //        {
        //            args.Handled = true;
        //            ShowUnhandledException(args.Exception, "Dispatcher.UnhandledException", true);
        //        }
        //    };

        //    // Catch exceptions from the main UI dispatcher thread.
        //    // Typically we only need to catch this OR the Dispatcher.UnhandledException.
        //    // Handling both can result in the exception getting handled twice.
        //    Application.Current.DispatcherUnhandledException += (sender, args) =>
        //    {
        //        // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
        //        if (!Debugger.IsAttached)
        //        {
        //            args.Handled = true;
        //            ShowUnhandledException(args.Exception, "Application.Current.DispatcherUnhandledException", true);
        //        }
        //    };
        //}

        ///// <summary>
        ///// Show the exception to the user
        ///// </summary>
        ///// <param name="e"></param>
        ///// <param name="unhandledExceptionType"></param>
        ///// <param name="promptUserForShutdown"></param>
        //void ShowUnhandledException(Exception e, string unhandledExceptionType, bool promptUserForShutdown)
        //{
        //    var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
        //    var messageBoxMessage = $"The following exception occurred:\n\n{e}";
        //    var messageBoxButtons = MessageBoxButton.OK;

        //    // TODO
        //    //_logger.Error(e, messageBoxTitle, messageBoxMessage);

        //    if (promptUserForShutdown)
        //    {
        //        messageBoxMessage += "\n\nNormally the app would die now. Should we let it die?";
        //        messageBoxButtons = MessageBoxButton.YesNo;
        //    }

        //    // Let the user decide if the app should die or not (if applicable).
        //    if (MessageBox.Show(messageBoxMessage, messageBoxTitle, messageBoxButtons) == MessageBoxResult.Yes)
        //    {
        //        Application.Current.Shutdown();
        //    }
        //}
    }
}
