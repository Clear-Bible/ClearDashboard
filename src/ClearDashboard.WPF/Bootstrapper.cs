using Caliburn.Micro;
using ClearDashboard.Wpf.Properties;
using ClearDashboard.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
//using Microsoft.Extensions.Http;
using System.Windows.Threading;


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

            // register the instance of ILog with the container.
            _container.RegisterInstance(typeof(ILog), null, _log);

            // wire up the interfaces required by Caliburn.Micro
            _container
                .Singleton<IWindowManager, WindowManager>()
                .Singleton<IEventAggregator, EventAggregator>();

              
            // wire up all of the view models in the project.
            GetType().Assembly.GetTypes()
                .Where(type => type.IsClass)
                .Where(type => type.Name.EndsWith("ViewModel"))
                .ToList()
                .ForEach(viewModelType => _container.RegisterPerRequest(
                    viewModelType, null, viewModelType));


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
