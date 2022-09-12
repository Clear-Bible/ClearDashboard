using Caliburn.Micro;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ClearDashboard.Wpf.Application.ViewModels.Menus;
using ClearDashboard.Wpf.Application.ViewModels.Project;

namespace ClearDashboard.Wpf.Application.ViewModels.ProjectDesignSurface
{

    public class NodeMenuItemViewModel : PropertyChangedBase
    {

        public NodeMenuItemViewModel()
        {
            _command = new CommandViewModel(Execute);
        }

        private ProjectDesignSurfaceViewModel _viewModel;

        public ProjectDesignSurfaceViewModel ViewModel
        {
            get => _viewModel;
            set { _viewModel = value; }
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

        private string _iconSource = string.Empty;
        public string IconSource
        {
            get => _iconSource;
            set
            {
                _iconSource = value;
                NotifyOfPropertyChange(() => IconSource);
            }
        }

        private string _iconKind;
        public string IconKind
        {
            get => _iconKind;
            set
            {
                _iconKind = value;
                NotifyOfPropertyChange(() => IconKind);
            }
        }


        private bool _isSeparator = false;
        public bool IsSeparator
        {
            get => _isSeparator;
            set
            {
                _isSeparator = value;
                NotifyOfPropertyChange(() => IsSeparator);
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

        public ObservableCollection<NodeMenuItemViewModel> MenuItems { get; set; }

        private readonly ICommand _command;
        public ICommand Command
        {
            get
            {
                return _command;
            }
        }

        private void Execute()
        {
            if (ViewModel is not null)
            {
                ViewModel.MenuCommmand(this);
            }
        }

        
    }
}
