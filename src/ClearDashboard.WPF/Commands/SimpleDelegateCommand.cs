using System;
using System.Windows.Input;

namespace ClearDashboard.Wpf.Commands
{
    /// <summary>
    /// A simple command class that implements <see cref="ICommand"/> and accepts a delegate action.
    /// </summary>
    /// <remarks>
    /// See https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.inputbinding?view=windowsdesktop-6.0 for implementation details.
    /// </remarks>
    public class SimpleDelegateCommand : ICommand
    {
        /// <summary>
        /// Gets and sets the <see cref="Key"/> that invokes the command.
        /// </summary>
        public Key GestureKey { get; set; }

        /// <summary>
        /// Gets and sets the <see cref="ModifierKeys"/> for the <see cref="Key"/> that invokes the command.
        /// </summary>
        public ModifierKeys GestureModifier { get; set; }

        /// <summary>
        /// Gets and sets the <see cref="MouseAction"/> that invokes the command.
        /// </summary>
        public MouseAction MouseGesture { get; set; }

        /// <summary>
        /// The delegate to invoke when the command is executed.
        /// </summary>
        private readonly Action<object> _executeDelegate;

        /// <summary>
        /// Public constructor for design-time use.
        /// </summary>
        public SimpleDelegateCommand()
        {
        }

        /// <summary>
        /// Public constructor accepting a delegate to execute.
        /// </summary>
        /// <param name="executeDelegate">The <see cref="Action{T}"/> to invoke when the command is executed.</param>
        public SimpleDelegateCommand(Action<object> executeDelegate)
        {
            _executeDelegate = executeDelegate;
        }

        /// <summary>
        /// Execute the command by passing with the specified parameter to the delegate.
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            _executeDelegate(parameter);
        }

        /// <summary>
        /// Determine whether the command can execute in its current state. Returns true unless overridden.
        /// </summary>
        /// <param name="parameter">The parameter to use in determining whether the command can execute in its current state.</param>
        /// <returns>True</returns>
        public virtual bool CanExecute(object parameter) { return true; }

        /// <summary>
        /// An event that occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;
    }
}
