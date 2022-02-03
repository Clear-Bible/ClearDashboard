using MvvmHelpers;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Windows.Input;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Views;

namespace ClearDashboard.Wpf.ViewModels
{
    public class MainWindowViewModel: ObservableObject
    {
        #region Props

        private string _version;
        public string Version
        {
            get => _version;
            set { SetProperty(ref _version, value); }
        }

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


        #region Startup

        public MainWindowViewModel()
        {
            //get the assembly version
            Version thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";


            // wire up the commands
            //ColorStylesCommand = new RelayCommand(ShowColorStyles, param => this.canExecute);
            ColorStylesCommand = new RelayCommand(ShowColorStyles);
        }



        #endregion

        #region Methods

        private void ShowColorStyles(object obj)
        {
            ColorStyles frm = new ColorStyles();
            frm.Show();
        }

        #endregion
    }
}
