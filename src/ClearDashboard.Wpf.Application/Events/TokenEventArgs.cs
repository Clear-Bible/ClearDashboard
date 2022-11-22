using System;
using System.Windows;
using System.Windows.Input;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.ViewModels.Display;

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

        public Guid VerseDisplayId { get; set; }
    }
}
