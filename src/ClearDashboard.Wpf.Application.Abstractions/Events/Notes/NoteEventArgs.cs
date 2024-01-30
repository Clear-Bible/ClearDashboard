using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using System.Windows;

namespace ClearDashboard.Wpf.Application.Events.Notes
{
    public class NoteEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The <see cref="NoteViewModel"/> to which this event pertains.
        /// </summary>
        public NoteViewModel Note { get; set; }

        /// <summary>
        /// The <see cref="TokenDisplayViewModel"/> that triggered this event.
        /// </summary>
        public TokenDisplayViewModel TokenDisplayViewModel { get; set; }

        /// <summary>
        /// The collection of selected <see cref="TokenDisplayViewModel"/>s.
        /// </summary>
        public TokenDisplayViewModelCollection SelectedTokens { get; set; } = new();

        public EntityIdCollection EntityIds { get; set; } = new();

        /// <summary>
        /// The position of the mouse relative to the control which fired the event.
        /// </summary>
        public Point MousePosition { get; set; }
    }
}
