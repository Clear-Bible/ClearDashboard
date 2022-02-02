using ClearDashboard.Wpf.Helpers;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace ClearDashboard.Wpf.ViewModels
{
    public class MainWindowViewModel: BindableBase
    {
        #region Props

        private string _version;
        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged("");
            }
        }

        #endregion

        #region Commands

        private ICommand _newProjectCommand;
        public ICommand NewProjectCommand
        {
            get => _newProjectCommand; 
            set
            {
                _newProjectCommand = value;
            }
        }

        #endregion


        #region Startup

        public MainWindowViewModel()
        {
            //get the assembly version
            Version thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Revision}.{thisVersion.Build}";


            // wire up the commands
            //NewProjectCommand = new RelayCommand(ShowCreateNewProject, param => this.canExecute);
            NewProjectCommand = new RelayCommand(ShowCreateNewProject);
        }



        #endregion

        #region Methods

        private void ShowCreateNewProject(object obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
