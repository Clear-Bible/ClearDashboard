using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.Display;

using NotesLabel = ClearDashboard.DAL.Alignment.Notes.Label;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A user control that displays a collection of <see cref="Note"/> instances.
    /// </summary>
    public partial class NotesControl : UserControl
    {
        /// <summary>
        /// Identifies the NoteApplied routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAppliedEvent = EventManager.RegisterRoutedEvent
            ("NoteApplied", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotesControl));

        /// <summary>
        /// Identifies the NoteCancelled routed event.
        /// </summary>
        public static readonly RoutedEvent NoteCancelledEvent = EventManager.RegisterRoutedEvent
            ("NoteCancelled", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotesControl));

        /// <summary>
        /// Identifies the EntityId dependency property.
        /// </summary>
        public static readonly DependencyProperty EntityIdProperty = DependencyProperty.Register("EntityId", typeof(IId), typeof(LabelSelector));

        /// <summary>
        /// Identifies the LabelSuggestions dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelSuggestionsProperty = DependencyProperty.Register("LabelSuggestions", typeof(IEnumerable<NotesLabel>), typeof(LabelSelector));

        /// <summary>
        /// Gets or sets the <see cref="EntityId{T}"/> that this control is operating on..
        /// </summary>
        public IId? EntityId
        {
            get => (IId)GetValue(EntityIdProperty);
            set => SetValue(EntityIdProperty, value);
        }


        /// <summary>
        /// Gets or sets a collection of <see cref="Label"/> objects for auto selection in the control.
        /// </summary>
        public IEnumerable<NotesLabel> LabelSuggestions
        {
            get => (IEnumerable<NotesLabel>)GetValue(LabelSuggestionsProperty);
            set => SetValue(LabelSuggestionsProperty, value);
        }

        /// <summary>
        /// Occurs when an existing label suggesting is selected.
        /// </summary>
        public event RoutedEventHandler LabelSelected
        {
            add => AddHandler(LabelSelectedEvent, value);
            remove => RemoveHandler(LabelSelectedEvent, value);
        }

        /// <summary>
        /// Occurs when an new label is added.
        /// </summary>
        public event RoutedEventHandler LabelAdded
        {
            add => AddHandler(LabelAddedEvent, value);
            remove => RemoveHandler(LabelAddedEvent, value);
        }

        /// <summary>
        /// Occurs when a note is applied.
        /// </summary>
        public event RoutedEventHandler NoteApplied
        {
            add => AddHandler(NoteAppliedEvent, value);
            remove => RemoveHandler(NoteAppliedEvent, value);
        }

        /// <summary>
        /// Occurs when a note is cancelled.
        /// </summary>
        public event RoutedEventHandler NoteCancelled
        {
            add => AddHandler(NoteCancelledEvent, value);
            remove => RemoveHandler(NoteCancelledEvent, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Note"/> to display in this control.
        /// </summary>
        public TokenDisplay TokenDisplay
        {
            get => (TokenDisplay)GetValue(NoteProperty);
            set => SetValue(NoteProperty, value);
        }

        public NoteControl()
        {
            InitializeComponent();
        }

        private void ApplyNote(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new NoteEventArgs()
            {
                RoutedEvent = NoteAppliedEvent,
                TokenDisplay = TokenDisplay
            });
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs{RoutedEvent = NoteCancelledEvent});
        }
    }
}
