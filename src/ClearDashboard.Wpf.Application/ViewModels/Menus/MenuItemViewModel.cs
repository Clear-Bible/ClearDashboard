using Caliburn.Micro;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading;
using System.Windows.Input;

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
            set => _mainViewModel = value;
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

        private string _id;
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
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

        public ICommand Command => _command;


        private async void Execute(CancellationToken token)
        {
            await ViewModel.ExecuteMenuCommand(this, token);
        }
    }
}