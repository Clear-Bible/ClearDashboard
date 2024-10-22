﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Input;
using Caliburn.Micro;

namespace ClearDashboard.Wpf.ViewModels.Menus
{
    public class MenuItemViewModel : PropertyChangedBase
    {
        private readonly ICommand _command;

        public MenuItemViewModel()
        {
            _command = new CommandViewModel(Execute);
        }

        private WorkSpaceViewModel _workSpaceViewModel;

        public WorkSpaceViewModel ViewModel
        {
            get => _workSpaceViewModel;
            set { _workSpaceViewModel = value; }
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

        private void Execute()
        {
            if (ViewModel != null)
            {
                ViewModel.WindowIdToLoad = Id;
            }
        }
    }
}
