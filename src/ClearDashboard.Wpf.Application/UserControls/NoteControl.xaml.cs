using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Wpf.Application.Events;
using NotesLabel = ClearDashboard.DAL.Alignment.Notes.Label;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control that displays the details of a single <see cref="Note"/>.
    /// </summary>
    public partial class NoteControl : UserControl
    {
        #region Static Routed Events
        /// <summary>
        /// Identifies the NoteApplied routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAppliedEvent = EventManager.RegisterRoutedEvent
            ("NoteApplied", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteControl));

        /// <summary>
        /// Identifies the NoteCancelled routed event.
        /// </summary>
        public static readonly RoutedEvent NoteCancelledEvent = EventManager.RegisterRoutedEvent
            ("NoteCancelled", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteControl));

        /// <summary>
        /// Identifies the LabelSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelSelectedEvent = EventManager.RegisterRoutedEvent
            ("LabelSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteControl));

        /// <summary>
        /// Identifies the LabelAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelAddedEvent = EventManager.RegisterRoutedEvent
            ("LabelAdded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteControl));

        #endregion Static Routed Events
        #region Static Dependency Properties
        /// <summary>
        /// Identifies the EntityId dependency property.
        /// </summary>
        public static readonly DependencyProperty EntityIdProperty = DependencyProperty.Register("EntityId", typeof(IId), typeof(NoteControl));

        /// <summary>
        /// Identifies the Note dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register("Note", typeof(Note), typeof(NoteControl));

        /// <summary>
        /// Identifies the LabelBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelBackgroundProperty = DependencyProperty.Register("LabelBackground", typeof(SolidColorBrush), typeof(NoteControl));

        /// <summary>
        /// Identifies the NoteFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteFontSizeProperty = DependencyProperty.Register("NoteFontSize", typeof(double), typeof(NoteControl),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the NoteMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteMarginProperty = DependencyProperty.Register("NoteMargin", typeof(Thickness), typeof(NoteControl),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the TimestampFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampFontSizeProperty = DependencyProperty.Register("TimestampFontSize", typeof(double), typeof(NoteControl),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the TimestampMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampMarginProperty = DependencyProperty.Register("TimestampMargin", typeof(Thickness), typeof(NoteControl),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the LabelSuggestions dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelSuggestionsProperty = DependencyProperty.Register("LabelSuggestions", typeof(IEnumerable<NotesLabel>), typeof(NoteControl));

        /// <summary>
        /// Identifies the LabelMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelMarginProperty = DependencyProperty.Register("LabelMargin", typeof(Thickness), typeof(NoteControl),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the LabelPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelPaddingProperty = DependencyProperty.Register("LabelPadding", typeof(Thickness), typeof(NoteControl),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the LabelCornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelCornerRadiusProperty = DependencyProperty.Register("LabelCornerRadius", typeof(CornerRadius), typeof(NoteControl),
            new PropertyMetadata(new CornerRadius(0)));
       
        /// <summary>
        /// Identifies the LabelFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelFontSizeProperty = DependencyProperty.Register("LabelFontSize", typeof(double), typeof(NoteControl),
            new PropertyMetadata(11d));
        #endregion
        #region Private event handlers
        private void ApplyNote(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new NoteEventArgs()
            {
                RoutedEvent = NoteAppliedEvent,
                EntityId = EntityId,
                Note = Note
            });
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs { RoutedEvent = NoteCancelledEvent });
        }
        #endregion Private event handlers
        #region Public Properties
        /// <summary>
        /// Gets or sets the <see cref="EntityId{T}"/> that contains the note.
        /// </summary>
        public IId? EntityId
        {
            get => (IId)GetValue(EntityIdProperty);
            set => SetValue(EntityIdProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Note"/> being displayed.
        /// </summary>
        public Note Note
        {
            get => (Note)GetValue(NoteProperty);
            set => SetValue(NoteProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for the note edit box.
        /// </summary>
        public Thickness NoteMargin
        {
            get => (Thickness)GetValue(NoteMarginProperty);
            set => SetValue(NoteMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for the note textbox.
        /// </summary>
        public double NoteFontSize
        {
            get => (double)GetValue(NoteFontSizeProperty);
            set => SetValue(NoteFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for the timestamp and user.
        /// </summary>
        public Thickness TimestampMargin
        {
            get => (Thickness)GetValue(TimestampMarginProperty);
            set => SetValue(TimestampMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for the timestamp and user.
        /// </summary>
        public double TimestampFontSize
        {
            get => (double)GetValue(TimestampFontSizeProperty);
            set => SetValue(TimestampFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush for individual label boxes.
        /// </summary>
        public SolidColorBrush LabelBackground
        {
            get => (SolidColorBrush)GetValue(LabelBackgroundProperty);
            set => SetValue(LabelBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for individual label boxes.
        /// </summary>
        public Thickness LabelMargin
        {
            get => (Thickness)GetValue(LabelMarginProperty);
            set => SetValue(LabelMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for individual label boxes.
        /// </summary>
        public Thickness LabelPadding
        {
            get => (Thickness)GetValue(LabelPaddingProperty);
            set => SetValue(LabelPaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets the corner radius for individual label boxes.
        /// </summary>
        public CornerRadius LabelCornerRadius
        {
            get => (CornerRadius)GetValue(LabelCornerRadiusProperty);
            set => SetValue(LabelCornerRadiusProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for individual label boxes.
        /// </summary>
        public double LabelFontSize
        {
            get => (double)GetValue(LabelFontSizeProperty);
            set => SetValue(LabelFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="DAL.Alignment.Notes.Label"/> objects for auto selection in the control.
        /// </summary>
        public IEnumerable<NotesLabel> LabelSuggestions
        {
            get => (IEnumerable<NotesLabel>)GetValue(LabelSuggestionsProperty);
            set => SetValue(LabelSuggestionsProperty, value);
        }

        /// <summary>
        /// Gets a formatted string corresponding to the date the note was created.
        /// </summary>
        public string Created => Note.NoteId?.Created != null ? Note.NoteId.Created.Value.ToString("u") : string.Empty;

        /// <summary>
        /// Gets a formatted string corresponding to the date the note was modified.
        /// </summary>
        public string Modified => Note.NoteId?.Modified != null ? Note.NoteId.Modified.Value.ToString("u") : string.Empty;

        /// <summary>
        /// Gets the UserId of the user that last modified the note.
        /// </summary>
        public string? UserId => Note.NoteId?.UserId?.ToString();

        #endregion
        #region Public events

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
        #endregion Public events

        public NoteControl()
        {
            InitializeComponent();
        }

    }
}
