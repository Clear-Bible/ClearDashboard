using System;
using System.Windows;
using System.Windows.Input;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.Events
{
    public class TranslationEventArgs : RoutedEventArgs
    {

        /// <summary>
        /// The <see cref="TokenDisplay"/> that triggered this event.
        /// </summary>
        public TokenDisplayViewModel? TokenDisplay { get; set; }

        public VerseDisplayViewModel? VerseDisplay { get; set; }

        /// <summary>
        /// The <see cref="Translation"/> to which this event pertains.
        /// </summary>
        public Translation Translation { get; set; }

        public string TranslationActionType { get; set; } = string.Empty;

        /// <summary>
        /// The keyboard <see cref="ModifierKeys"/> at the time of the event.
        /// </summary>
        public ModifierKeys ModifierKeys { get; set; }
    }
}
