using System;
using System.Windows;
using System.Windows.Input;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Events
{
    public class TokenEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The <see cref="TokenDisplayViewModel"/> that triggered this event.
        /// </summary>
        public TokenDisplayViewModel TokenDisplay { get; set; }

        /// <summary>
        /// The collection of selected <see cref="TokenDisplayViewModel"/>s.
        /// </summary>
        public TokenDisplayViewModelCollection SelectedTokens { get; set; } = new();

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

        /// <summary>
        /// The left button <see cref="MouseButtonState"/> at the time of the event.
        /// </summary>
        public MouseButtonState MouseLeftButton { get; set; }

        /// <summary>
        /// Gets whether the left mouse button is pressed at the time of the event.
        /// </summary>
        public bool IsMouseLeftButtonDown => MouseLeftButton == MouseButtonState.Pressed;

        /// <summary>
        /// The middle button <see cref="MouseButtonState"/> at the time of the event.
        /// </summary>
        public MouseButtonState MouseMiddleButton { get; set; }

        /// <summary>
        /// Gets whether the middle mouse button is pressed at the time of the event.
        /// </summary>
        public bool IsMouseMiddleButtonDown => MouseMiddleButton == MouseButtonState.Pressed;

        /// <summary>
        /// The right button <see cref="MouseButtonState"/> at the time of the event.
        /// </summary>
        public MouseButtonState MouseRightButton { get; set; }

        /// <summary>
        /// Gets whether the right mouse button is pressed at the time of the event.
        /// </summary>
        public bool IsMouseRightButtonDown => MouseRightButton == MouseButtonState.Pressed;

        public Guid VerseDisplayId { get; set; }
    }
}
