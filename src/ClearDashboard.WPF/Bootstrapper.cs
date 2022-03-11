using Caliburn.Micro;
using ClearDashboard.Wpf.Properties;
using ClearDashboard.Wpf.ViewModels;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Action = System.Action;

namespace ClearDashboard.Wpf
{


    public class Bootstrapper : BootstrapperBase
    {
        #region Props

        private SimpleContainer _container;
        private Log _log;

        #endregion

        #region Startup

        public Bootstrapper()
        {
            Initialize();

            //set the light/dark
            ((App)Application.Current).SetTheme(Settings.Default.Theme);
        }

        #endregion


        protected override void Configure()
        {
            _log = new Log();

            _log.Info("Configuring dependency injection for the application.");

            // put a reference into the App
            ((App)Application.Current).Log = _log;


            _container = new SimpleContainer();
            _container.Instance(_container);
            _container.Singleton<ILog, Log>();

            _container
                .Singleton<IWindowManager, WindowManager>()
                .Singleton<IEventAggregator, EventAggregator>()

                .PerRequest<ShellViewModel>()
                .PerRequest<CreateNewProjectsViewModel>()
                .PerRequest<SettingsViewModel>()
                .PerRequest<WorkSpaceViewModel>()
                .PerRequest<LandingViewModel>();
             
            
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            await DisplayRootViewForAsync<ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }


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
    }
}
