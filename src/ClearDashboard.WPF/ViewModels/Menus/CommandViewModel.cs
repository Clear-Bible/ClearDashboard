using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClearDashboard.Wpf.Models.Menus
{
    public class CommandViewModel : ICommand
    {
        private readonly Action _action;

        public CommandViewModel(Action action)
        {
            _action = action;
        }

        public void Execute(object o)
        {
            _action();
        }

        public bool CanExecute(object o)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
