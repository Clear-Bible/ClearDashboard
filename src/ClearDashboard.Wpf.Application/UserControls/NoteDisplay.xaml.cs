using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using NotesLabel = ClearDashboard.DAL.Alignment.Notes.Label;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control that displays the details of a single <see cref="Note"/>.
    /// </summary>
    public partial class NoteDisplay : UserControl, INotifyPropertyChanged
    {
        #region Static Routed Events
        /// <summary>
        /// Identifies the NoteAdded routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAddedEvent = EventManager.RegisterRoutedEvent
            ("NoteAdded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /// <summary>
        /// Identifies the NoteUpdated routed event.
        /// </summary>
        public static readonly RoutedEvent NoteUpdatedEvent = EventManager.RegisterRoutedEvent
            ("NoteUpdated", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /// <summary>
        /// Identifies the NoteDeleted routed event.
        /// </summary>
        public static readonly RoutedEvent NoteDeletedEvent = EventManager.RegisterRoutedEvent
            ("NoteDeleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /// <summary>
        /// Identifies the LabelSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelSelectedEvent = EventManager.RegisterRoutedEvent
            ("LabelSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /// <summary>
        /// Identifies the LabelAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelAddedEvent = EventManager.RegisterRoutedEvent
            ("LabelAdded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        #endregion Static Routed Events
        #region Static Dependency Properties
        /// <summary>
        /// Identifies the EntityId dependency property.
        /// </summary>
        public static readonly DependencyProperty EntityIdProperty = DependencyProperty.Register("EntityId", typeof(IId), typeof(NoteDisplay));

        /// <summary>
        /// Identifies the Note dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register("Note", typeof(Note), typeof(NoteDisplay));

        /// <summary>
        /// Identifies the LabelBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelBackgroundProperty = DependencyProperty.Register("LabelBackground", typeof(SolidColorBrush), typeof(NoteDisplay));

        /// <summary>
        /// Identifies the NoteFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteFontSizeProperty = DependencyProperty.Register("NoteFontSize", typeof(double), typeof(NoteDisplay),
            new PropertyMetadata(15d));

        /// <summary>
        /// Identifies the NoteMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteMarginProperty = DependencyProperty.Register("NoteMargin", typeof(Thickness), typeof(NoteDisplay),
            new PropertyMetadata(new Thickness(2, 2, 2, 2)));

        /// <summary>
        /// Identifies the TimestampFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampFontSizeProperty = DependencyProperty.Register("TimestampFontSize", typeof(double), typeof(NoteDisplay),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the TimestampMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampMarginProperty = DependencyProperty.Register("TimestampMargin", typeof(Thickness), typeof(NoteDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the LabelSuggestions dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelSuggestionsProperty = DependencyProperty.Register("LabelSuggestions", typeof(IEnumerable<NotesLabel>), typeof(NoteDisplay));

        /// <summary>
        /// Identifies the LabelMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelMarginProperty = DependencyProperty.Register("LabelMargin", typeof(Thickness), typeof(NoteDisplay),
            new PropertyMetadata(new Thickness(3, 0, 3, 0)));

        /// <summary>
        /// Identifies the LabelPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelPaddingProperty = DependencyProperty.Register("LabelPadding", typeof(Thickness), typeof(NoteDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the LabelCornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelCornerRadiusProperty = DependencyProperty.Register("LabelCornerRadius", typeof(CornerRadius), typeof(NoteDisplay),
            new PropertyMetadata(new CornerRadius(0)));
       
        /// <summary>
        /// Identifies the LabelFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelFontSizeProperty = DependencyProperty.Register("LabelFontSize", typeof(double), typeof(NoteDisplay),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the AddMode dependency property.
        /// </summary>
        public static readonly DependencyProperty AddModeProperty = DependencyProperty.Register("AddMode", typeof(bool), typeof(NoteDisplay),
            new PropertyMetadata(false, OnAddModeChanged));

        /// <summary>
        /// Identifies the Watermark dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(string), typeof(NoteDisplay));

        #endregion
        #region Private event handlers

        private static void OnAddModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = (NoteDisplay)obj;
            control.UpdateControlLayout();
            control.OnPropertyChanged(nameof(ApplyLabel));
        }

        private void UpdateControlLayout()
        {
            if (AddMode)
            {
                IsEditing = true;
            }
        }

        private void CloseEdit()
        {
            IsEditing = false;
            IsChanged = false;
        }

        private void ApplyNote(object sender, RoutedEventArgs e)
        {
            CloseEdit();
            RaiseEvent(new NoteEventArgs()
            {
                RoutedEvent = AddMode ? NoteAddedEvent : NoteUpdatedEvent,
                EntityId = EntityId,
                Note = Note
            });
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Note.Text = OriginalNoteText;
            OnPropertyChanged(nameof(NoteText));

            CloseEdit();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateControlLayout();
        }

        private void NoteLabelClick(object sender, MouseButtonEventArgs e)
        {
            IsEditing = true;

            NoteTextBox.Focus();
            NoteTextBox.Select(NoteTextBox.Text.Length, 0);
            
            OriginalNoteText = NoteText;
        }

        private void NoteTextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (NoteTextBoxVisibility == Visibility.Visible)
            {
                IsChanged = true;
            }
        }

        private void RaiseLabelEvent(RoutedEvent routedEvent, LabelEventArgs args)
        {
            RaiseEvent(new LabelEventArgs
            {
                RoutedEvent = routedEvent,
                EntityId = EntityId,
                Label = args?.Label,
                Note = Note
            });
        }

        private void OnLabelAdded(object sender, RoutedEventArgs e)
        {
            var labelEventArgs = e as LabelEventArgs;
            Note.Labels.Add(labelEventArgs.Label);
            OnPropertyChanged(nameof(NoteLabels));

            RaiseLabelEvent(LabelAddedEvent, labelEventArgs);
        }

        private void OnLabelSelected(object sender, RoutedEventArgs e)
        {
            var labelEventArgs = e as LabelEventArgs;
            Note.Labels.Add(labelEventArgs.Label);
            OnPropertyChanged(nameof(NoteLabels));

            RaiseLabelEvent(LabelSelectedEvent, labelEventArgs);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Private event handlers
        #region Public Properties

        /// <summary>
        /// Gets or sets whether the control is adding a new note or editing an existing one.
        /// </summary>
        public bool AddMode 
        {
            get => (bool)GetValue(AddModeProperty);
            set => SetValue(AddModeProperty, value);
        }

        // TODO: localize
        public string ApplyLabel => AddMode ? "Add Note" : "Update Note";

        private string OriginalNoteText { get; set; }

        private bool _isEditing = false;
        private bool _isChanged = false;

        private bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(NoteLabelVisibility));
                OnPropertyChanged(nameof(NoteTextBoxVisibility));
            }
        }

        private bool IsChanged
        {
            get => _isChanged;
            set
            {
                _isChanged = value;
                OnPropertyChanged(nameof(TimestampVisibility));
                OnPropertyChanged(nameof(ButtonVisibility));
            } 
        }

        public Visibility NoteLabelVisibility => IsEditing ? Visibility.Hidden : Visibility.Visible;
        public Visibility NoteTextBoxVisibility => IsEditing ? Visibility.Visible : Visibility.Hidden;
        public Visibility TimestampVisibility => IsChanged ? Visibility.Hidden : Visibility.Visible;
        public Visibility ButtonVisibility => IsChanged ? Visibility.Visible : Visibility.Hidden;

        /// <summary>
        /// Gets the text of the note to display.
        /// </summary>
        public string NoteText => Note?.Text;

        /// <summary>
        /// Gets the labels of the note to display.
        /// </summary>
        public ObservableCollection<NotesLabel> NoteLabels => Note?.Labels;

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
        /// Gets or sets the font size for the note text box.
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
        /// Gets or sets the watermark to display for new notes.
        /// </summary>
        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
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

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion Public events

        public NoteDisplay()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }
    }
}
