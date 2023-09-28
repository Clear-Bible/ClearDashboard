using System.Windows;
using System.Windows.Input;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Events
{
    public class TokenCharacterEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The <see cref="TokenCharacterViewModel"/> that triggered this event.
        /// </summary>
        public TokenCharacterViewModel TokenCharacter { get; set; } = new();

        /// <summary>
        /// The keyboard <see cref="ModifierKeys"/> at the time of the event.
        /// </summary>
        public ModifierKeys ModifierKeys { get; set; }

        /// <summary>
        /// Gets whether the Ctrl key is pressed at the time of the event.
        /// </summary>
        public bool IsControlPressed => (ModifierKeys & ModifierKeys.Control) > 0;

        /// <summary>
        /// Gets whether the Shift key is pressed at the time of the event.
        /// </summary>
        public bool IsShiftPressed => (ModifierKeys & ModifierKeys.Shift) > 0;

        /// <summary>
        /// Gets whether the Alt key is pressed at the time of the event.
        /// </summary>
        public bool IsAltPressed => (ModifierKeys & ModifierKeys.Alt) > 0;
    }
}
