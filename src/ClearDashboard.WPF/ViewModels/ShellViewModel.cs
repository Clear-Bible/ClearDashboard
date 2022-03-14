using MvvmHelpers;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using ClearDashboard.DAL.Events;
using ClearDashboard.DAL.NamedPipes;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Views;
using Action = System.Action;
using System.Threading.Tasks;
using System.Threading;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ShellViewModel: Screen
    {
        #region Props

        private readonly ILog _logger;


        //Connection to the DAL
        //DAL.StartUp _startup;
        private readonly DAL.StartUp _DAL;

        private string _paratextUserName;
        public string ParatextUserName
        {
            get => _paratextUserName;

            set
            {
                _paratextUserName = value;
                NotifyOfPropertyChange(() => ParatextUserName);
            }
        }


        private string _version;
        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                NotifyOfPropertyChange(() => Version);
            }
        }

        #endregion

        #region ObservableProps


        #endregion

        #region Commands

        private ICommand _colorStylesCommand;
        public ICommand ColorStylesCommand
        {
            get => _colorStylesCommand; 
            set
            {
                _colorStylesCommand = value;
            }
        }

        #endregion

        #region events

        /// <summary>
        /// Capture the current Paratext username
        /// </summary>
        /// <param abbr="e"></param>
        private void HandleSetParatextUserNameEvent(object sender, EventArgs e)
        {
            var args = (CustomEvents.ParatextUsernameEventArgs)e;
            ParatextUserName = args.ParatextUserName;
        }

        #endregion


        #region Startup

        public ShellViewModel()
        {
            // default one for the XAML page
        }

        /// <summary>
        /// Overload for DI of the logger
        /// </summary>
        /// <param name="logger"></param>
        public ShellViewModel(ILog logger)
        {
            _logger = logger;

            _logger.Info("In ShellViewModel ctor");

            //get the assembly version
            Version thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";


            // wire up the commands
            ColorStylesCommand = new RelayCommand(ShowColorStyles);

            // listen for username changes in Paratext
            DAL.StartUp.ParatextUserNameEventHandler += HandleSetParatextUserNameEvent;
            _DAL = new DAL.StartUp();

            _DAL.NamedPipeChanged += HandleEvent;
        }

        #endregion

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            _DAL.OnClosing();

            return base.CanCloseAsync(cancellationToken);
        }


        #region Methods

        private void HandleEvent(object sender, NamedPipesClient.PipeEventArgs args)
        {
            Debug.WriteLine($"{args.Text}");

            //Application.Current.Dispatcher.Invoke(
            //    System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
            //    {
            //        rtb.AppendText($"{args.Text}{Environment.NewLine}");
            //    });
        }

        /// <summary>
        /// Show the ColorStyles form
        /// </summary>
        /// <param abbr="obj"></param>
        private void ShowColorStyles(object obj)
        {
            ColorStyles frm = new ColorStyles();
            frm.Show();
        }

        #endregion
    }
}
