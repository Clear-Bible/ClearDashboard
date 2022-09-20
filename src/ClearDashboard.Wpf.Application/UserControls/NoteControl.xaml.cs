using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.Wpf.Application.Events;
using MahApps.Metro.Converters;
using NotesLabel = ClearDashboard.DAL.Alignment.Notes.Label;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control that displays the details of a single <see cref="Note"/>.
    /// </summary>
    public partial class NoteControl : UserControl, INotifyPropertyChanged
    {
        #region Static Routed Events
        /// <summary>
        /// Identifies the NoteAdded routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAddedEvent = EventManager.RegisterRoutedEvent
            ("NoteAdded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteControl));

        /// <summary>
        /// Identifies the NoteUpdated routed event.
        /// </summary>
        public static readonly RoutedEvent NoteUpdatedEvent = EventManager.RegisterRoutedEvent
            ("NoteUpdated", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteControl));

        /// <summary>
        /// Identifies the NoteDeleted routed event.
        /// </summary>
        public static readonly RoutedEvent NoteDeletedEvent = EventManager.RegisterRoutedEvent
            ("NoteDeleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteControl));

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
            new PropertyMetadata(15d));

        /// <summary>
        /// Identifies the NoteMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteMarginProperty = DependencyProperty.Register("NoteMargin", typeof(Thickness), typeof(NoteControl),
            new PropertyMetadata(new Thickness(2, 2, 2, 2)));

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
            new PropertyMetadata(new Thickness(3, 0, 3, 0)));

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

        private void CloseEdit()
        {
            NoteLabelVisibility = Visibility.Visible;
            NoteTextBoxVisibility = Visibility.Hidden;
            TimestampVisibility = Visibility.Visible;
            ButtonVisibility = Visibility.Hidden;

            OnPropertyChanged(nameof(NoteLabelVisibility));
            OnPropertyChanged(nameof(NoteTextBoxVisibility));
            OnPropertyChanged(nameof(TimestampVisibility));
            OnPropertyChanged(nameof(ButtonVisibility));
        }

        private void ApplyNote(object sender, RoutedEventArgs e)
        {
            CloseEdit();
            RaiseEvent(new NoteEventArgs()
            {
                RoutedEvent = NoteUpdatedEvent,
                EntityId = EntityId,
                Note = Note
            });
        }

        public string NoteText => Note?.Text;

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Note.Text = OriginalNoteText;
            OnPropertyChanged("Note.Text");

            CloseEdit();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void NoteLabelClick(object sender, MouseButtonEventArgs e)
        {
            NoteLabelVisibility = Visibility.Collapsed;
            NoteTextBoxVisibility = Visibility.Visible;
            NoteTextBox.Focus();

            OriginalNoteText = Note.Text;

            OnPropertyChanged(nameof(NoteLabelVisibility));
            OnPropertyChanged(nameof(NoteTextBoxVisibility));
        }

        private void NoteTextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (NoteTextBoxVisibility == Visibility.Visible)
            {
                TimestampVisibility = Visibility.Hidden;
                ButtonVisibility = Visibility.Visible;

                OnPropertyChanged(nameof(TimestampVisibility));
                OnPropertyChanged(nameof(ButtonVisibility));
            }
        }

        private void RaiseLabelEvent(RoutedEvent routedEvent, NotesLabel label)
        {
            RaiseEvent(new LabelEventArgs
            {
                RoutedEvent = routedEvent,
                EntityId = EntityId,
                Label = label
            });
        }


        private void OnLabelAdded(object sender, RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        private void OnLabelSelected(object sender, RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Private event handlers
        #region Public Properties

        public Visibility NoteLabelVisibility { get; set; } = Visibility.Visible;
        public Visibility NoteTextBoxVisibility { get; set; } = Visibility.Collapsed;

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
        public string? Created => Note.NoteId?.Created != null ? Note?.NoteId.Created.Value.ToString("u") : string.Empty;

        /// <summary>
        /// Gets a formatted string corresponding to the date the note was modified.
        /// </summary>
        //public string? Modified => Note != null && Note.NoteId != null && Note.NoteId.Modified != null ? Note?.NoteId?.Modified.Value.ToString("u") : string.Empty;
        public string? Modified => DateTimeOffset.UtcNow.ToString("u");

        /// <summary>
        /// Gets the UserId of the user that last modified the note.
        /// </summary>
        //public string? UserId => Note?.NoteId?.UserId?.ToString();
        public string? UserId => "Joe Schmoe";

        #endregion
        #region Public events

        /// <summary>
        /// Occurs when a note is applied.
        /// </summary>
        public event RoutedEventHandler NoteAdded
        {
            add => AddHandler(NoteAddedEvent, value);
            remove => RemoveHandler(NoteAddedEvent, value);
        }

        /// <summary>
        /// Occurs when a note is updated.
        /// </summary>
        public event RoutedEventHandler NoteUpdated
        {
            add => AddHandler(NoteUpdatedEvent, value);
            remove => RemoveHandler(NoteUpdatedEvent, value);
        }

        /// <summary>
        /// Occurs when a note is deleted.
        /// </summary>
        public event RoutedEventHandler NoteDeleted
        {
            add => AddHandler(NoteDeletedEvent, value);
            remove => RemoveHandler(NoteDeletedEvent, value);
        }

        /// <summary>
        /// Occurs when an existing label suggestion is selected.
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

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion Public events

        public NoteControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private string OriginalNoteText { get; set; }

        public Visibility TimestampVisibility { get; set; } = Visibility.Visible;
        public Visibility ButtonVisibility { get; set; } = Visibility.Hidden;




    }
}
