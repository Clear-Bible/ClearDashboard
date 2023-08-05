using System;
using System.Windows;
using System.Windows.Input;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Events
{
    public class TranslationEventArgs : RoutedEventArgs
    {

        /// <summary>
        /// The <see cref="TokenDisplay"/> that triggered this event.
        /// </summary>
        public TokenDisplayViewModel? TokenDisplay { get; set; }

        /// <summary>
        /// The collection of selected <see cref="TokenDisplayViewModel"/>s.
        /// </summary>
        public TokenDisplayViewModelCollection SelectedTokens { get; set; } = new();

        public InterlinearDisplayViewModel? InterlinearDisplay { get; set; }

        /// <summary>
        /// The <see cref="Translation"/> to which this event pertains.
        /// </summary>
        public Translation Translation { get; set; }

        public string TranslationActionType { get; set; } = string.Empty;

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
