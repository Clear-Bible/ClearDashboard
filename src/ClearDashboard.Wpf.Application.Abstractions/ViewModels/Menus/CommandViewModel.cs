using System;
using System.Threading;
using System.Windows.Input;

namespace ClearDashboard.Wpf.Application.ViewModels.Menus
{
    public class CommandViewModel : ICommand
    {
        private readonly Action<CancellationToken> _action;

        public CommandViewModel(Action<CancellationToken> action)
        {
            _action = action;
        }

        public void Execute(object o)
        {
			// TODO:  this should get a real cancellation token...from where??:
			_action(CancellationToken.None);
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
