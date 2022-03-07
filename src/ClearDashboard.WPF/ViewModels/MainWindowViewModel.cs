using MvvmHelpers;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Windows.Input;
using Caliburn.Micro;
using ClearDashboard.DAL.Events;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Views;


namespace ClearDashboard.Wpf.ViewModels
{
    public class MainWindowViewModel: Screen
    {
        #region Props

        private readonly ILog _logger;


        //Connection to the DAL
        DAL.StartUp _startup;

        private string _paratextUserName;
        public string ParatextUserName
        {
            get => _paratextUserName;

            set
            {
                NotifyOfPropertyChange(() => ParatextUserName);
                //SetProperty(ref _paratextUserName, value);
            }
        }


        private string _version;
        public string Version
        {
            get => _version;
            set
            {
                NotifyOfPropertyChange(() => Version);
                //SetProperty(ref _version, value);
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

        public MainWindowViewModel()
        {
            // default one for the XAML page
        }

        /// <summary>
        /// Overload for DI of the logger
        /// </summary>
        /// <param name="logger"></param>
        public MainWindowViewModel(ILog logger)
        {
            _logger = logger;

            _logger.Info("In MainWindowViewModel ctor");

            //get the assembly version
            Version thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";


            // wire up the commands
            ColorStylesCommand = new RelayCommand(ShowColorStyles);

            // listen for username changes in Paratext
            DAL.StartUp.ParatextUserNameEventHandler += HandleSetParatextUserNameEvent;
            _startup = new DAL.StartUp();
        }

        #endregion

        #region Methods

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
