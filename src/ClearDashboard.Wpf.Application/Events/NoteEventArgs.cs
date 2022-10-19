using System;
using System.Windows;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.Events
{
    public class NoteEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The <see cref="TokenDisplayViewModel"/> that triggered this event.
        /// </summary>
        public TokenDisplayViewModel TokenDisplayViewModel { get; set; }

        /// <summary>
        /// The collection of selected <see cref="TokenDisplayViewModel"/>s.
        /// </summary>
        public TokenDisplayViewModelCollection SelectedTokens { get; set; } = new();

        public IId? EntityId { get; set; }

        public EntityIdCollection EntityIds { get; set; } = new();

        public Note? Note { get; set; }
    }
}
