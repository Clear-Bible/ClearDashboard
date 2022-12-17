using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Navigation;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using ClearDashboard.Wpf.Application.Views.Startup;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClearDashboard.Wpf.Application.ViewModels.Menus
{
    public class MenuItemViewModel : PropertyChangedBase
    {
        private readonly ICommand _command;

        public MenuItemViewModel()
        {
            _command = new CommandViewModel(Execute);
        }

        private MainViewModel _mainViewModel;

        public MainViewModel ViewModel
        {
            get => _mainViewModel;
            set { _mainViewModel = value; }
        }



        private bool _isChecked = false;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                NotifyOfPropertyChange(() => IsChecked);
            }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                NotifyOfPropertyChange(() => IsEnabled);
            }
        }
        
        private Icon _icon;
        public Icon Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                NotifyOfPropertyChange(() => Icon);
            }
        }

        private string _iconSource;
        public string IconSource
        {
            get => _iconSource;
            set
            {
                _iconSource = value;
                NotifyOfPropertyChange(() => IconSource);
            }
        }



        private string _Id;
        public string Id
        {
            get => _Id;
            set
            {
                _Id = value;
                NotifyOfPropertyChange(() => Id);
            }
        }


        private string _header;
        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                NotifyOfPropertyChange(() => Header);
            }
        }

        public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }

        public ICommand Command
        {
            get
            {
                return _command;
            }
        }

        
        private async void Execute()
        {
            if (Id is "NewID" or "OpenID")
            {
                await ViewModel.ExecuteMenuCommand(this);
                return;
            }
            if (ViewModel != null)
            {
                ViewModel.WindowIdToLoad = Id;
            }


        }
    }
}
