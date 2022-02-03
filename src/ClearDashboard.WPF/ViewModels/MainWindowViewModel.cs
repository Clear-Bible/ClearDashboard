using MvvmHelpers;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Windows.Input;
using ClearDashboard.DAL.Events;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Views;

namespace ClearDashboard.Wpf.ViewModels
{
    public class MainWindowViewModel: ObservableObject
    {
        #region Props

        //Connection to the DAL
        DAL.StartUp _startup;

        private string _paratextUserName;
        public string ParatextUserName
        {
            get => _paratextUserName;
            set { SetProperty(ref _paratextUserName, value); }
        }


        private string _version;
        public string Version
        {
            get => _version;
            set { SetProperty(ref _version, value); }
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


        #endregion


        #region Startup

        public MainWindowViewModel()
        {
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
        /// Capture the current Paratext username
        /// </summary>
        /// <param name="e"></param>
        private void HandleSetParatextUserNameEvent(object sender, EventArgs e)
        {
            var args = (CustomEvents.ParatextUsernameEventArgs)e;
            ParatextUserName = args.ParatextUserName;
        }

        /// <summary>
        /// Show the ColorStyles form
        /// </summary>
        /// <param name="obj"></param>
        private void ShowColorStyles(object obj)
        {
            ColorStyles frm = new ColorStyles();
            frm.Show();
        }

        #endregion
    }
}
