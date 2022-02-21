using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;

namespace ClearDashboard.Wpf.Models.Menus
{
    public class MenuItemViewModel : ObservableObject
    {
        private readonly ICommand _command;

        public MenuItemViewModel()
        {
            _command = new CommandViewModel(Execute);
        }


        private bool _isChecked = false;
        public bool IsChecked
        {
            get => _isChecked;
            set { SetProperty(ref _isChecked, value, nameof(IsChecked)); }
        }



        private string _Id;
        public string Id
        {
            get => _Id;
            set { SetProperty(ref _Id, value, nameof(Id)); }
        }


        private string _header;
        public string Header
        {
            get => _header;
            set { SetProperty(ref _header, value, nameof(Header)); }
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
            MessageBox.Show("Clicked at " + Id);
        }
    }
}
