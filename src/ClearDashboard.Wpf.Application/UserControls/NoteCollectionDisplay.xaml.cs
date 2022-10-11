using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.Display;
using NotesLabel = ClearDashboard.DAL.Alignment.Notes.Label;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A user control that displays a collection of <see cref="Note"/> instances.
    /// </summary>
    public partial class NoteCollectionDisplay : UserControl, INotifyPropertyChanged
    {
        #region Static Routed Events
        /// <summary>
        /// Identifies the NoteApplied routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAddedEvent = EventManager.RegisterRoutedEvent
            ("NoteAdded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteCollectionDisplay));

        /// <summary>
        /// Identifies the NoteUpdated routed event.
        /// </summary>
        public static readonly RoutedEvent NoteUpdatedEvent = EventManager.RegisterRoutedEvent
            ("NoteUpdated", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteCollectionDisplay));

        /// <summary>
        /// Identifies the NoteDeleted routed event.
        /// </summary>
        public static readonly RoutedEvent NoteDeletedEvent = EventManager.RegisterRoutedEvent
            ("NoteDeleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteCollectionDisplay));

        /// <summary>
        /// Identifies the LabelSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelSelectedEvent = EventManager.RegisterRoutedEvent
            ("LabelSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteCollectionDisplay));

        /// <summary>
        /// Identifies the LabelAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelAddedEvent = EventManager.RegisterRoutedEvent
            ("LabelAdded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteCollectionDisplay));

        /// <summary>
        /// Identifies the CloseRequestedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent CloseRequestedEvent = EventManager.RegisterRoutedEvent
            ("CloseRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteCollectionDisplay));

        #endregion Static Routed Events
        #region Static Dependency Properties
  
        /// <summary>
        /// Identifies the EntityId dependency property.
        /// </summary>
        public static readonly DependencyProperty EntityIdProperty = DependencyProperty.Register("EntityId", typeof(IId), typeof(NoteCollectionDisplay));

        /// <summary>
        /// Identifies the EntityId dependency property.
        /// </summary>
        public static readonly DependencyProperty EntityIdsProperty = DependencyProperty.Register("EntityIds", typeof(EntityIdCollection), typeof(NoteCollectionDisplay));

        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(NoteCollectionDisplay));
        
        /// <summary>
        /// Identifies the Notes dependency property.
        /// </summary>
        public static readonly DependencyProperty NotesProperty = DependencyProperty.Register("Notes", typeof(ObservableCollection<Note>), typeof(NoteCollectionDisplay));

        /// <summary>
        /// Identifies the LabelBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelBackgroundProperty = DependencyProperty.Register("LabelBackground", typeof(SolidColorBrush), typeof(NoteCollectionDisplay),
            new PropertyMetadata(Brushes.BlanchedAlmond));

        /// <summary>
        /// Identifies the NoteFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteFontSizeProperty = DependencyProperty.Register("NoteFontSize", typeof(double), typeof(NoteCollectionDisplay),
            new PropertyMetadata(15d));

        /// <summary>
        /// Identifies the NoteMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteMarginProperty = DependencyProperty.Register("NoteMargin", typeof(Thickness), typeof(NoteCollectionDisplay),
            new PropertyMetadata(new Thickness(2, 2, 2, 2)));

        /// <summary>
        /// Identifies the InnerMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerMarginProperty = DependencyProperty.Register("InnerMargin", typeof(Thickness), typeof(NoteCollectionDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 10)));

        /// <summary>
        /// Identifies the UserMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty UserMarginProperty = DependencyProperty.Register("UserMargin", typeof(Thickness), typeof(NoteCollectionDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the UserFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty UserFontFamilyProperty = DependencyProperty.Register("UserFontFamily", typeof(FontFamily), typeof(NoteCollectionDisplay),
                new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the UserFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty UserFontSizeProperty = DependencyProperty.Register("UserFontSize", typeof(double), typeof(NoteCollectionDisplay),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the UserFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty UserFontWeightProperty = DependencyProperty.Register("UserFontWeight", typeof(FontWeight), typeof(NoteCollectionDisplay),
            new PropertyMetadata(FontWeights.SemiBold));

        /// <summary>
        /// Identifies the UserFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty UserFontStyleProperty = DependencyProperty.Register("UserFontStyle", typeof(FontStyle), typeof(NoteCollectionDisplay),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the TimestampFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampFontSizeProperty = DependencyProperty.Register("TimestampFontSize", typeof(double), typeof(NoteCollectionDisplay),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the TimestampMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampMarginProperty = DependencyProperty.Register("TimestampMargin", typeof(Thickness), typeof(NoteCollectionDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the LabelSuggestions dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelSuggestionsProperty = DependencyProperty.Register("LabelSuggestions", typeof(IEnumerable<NotesLabel>), typeof(NoteCollectionDisplay));

        /// <summary>
        /// Identifies the LabelMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelMarginProperty = DependencyProperty.Register("LabelMargin", typeof(Thickness), typeof(NoteCollectionDisplay),
            new PropertyMetadata(new Thickness(3, 0, 3, 0)));

        /// <summary>
        /// Identifies the LabelPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelPaddingProperty = DependencyProperty.Register("LabelPadding", typeof(Thickness), typeof(NoteCollectionDisplay),
            new PropertyMetadata(new Thickness(10, 5, 10, 5)));

        /// <summary>
        /// Identifies the LabelCornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelCornerRadiusProperty = DependencyProperty.Register("LabelCornerRadius", typeof(CornerRadius), typeof(NoteCollectionDisplay),
            new PropertyMetadata(new CornerRadius(10)));

        /// <summary>
        /// Identifies the LabelFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelFontSizeProperty = DependencyProperty.Register("LabelFontSize", typeof(double), typeof(NoteCollectionDisplay),
            new PropertyMetadata(11d));

        #endregion
        #region Private event handlers

        private void RaiseNoteEvent(RoutedEvent routedEvent, NoteEventArgs e)
        {
            RaiseEvent(new NoteEventArgs
            {
                RoutedEvent = routedEvent,
                Note = e.Note,
                EntityId = e.EntityId,
                EntityIds = e.EntityIds
            });
        }

        private void RaiseLabelEvent(RoutedEvent routedEvent, LabelEventArgs e)
        {
            RaiseEvent(new LabelEventArgs
            {
                RoutedEvent = routedEvent,
                Note = e.Note,
                Label = e.Label,
                EntityId = e.EntityId
            });
        }

        private void OnCloseRequested(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs { RoutedEvent = CloseRequestedEvent });
        }

        private void OnNoteAdded(object sender, RoutedEventArgs e)
        {
            var args = e as NoteEventArgs;
            if (Notes == null) Notes = new NoteCollection();
            Notes.Add(args.Note);
            NewNote = new Note();

            OnPropertyChanged(nameof(Notes));
            OnPropertyChanged(nameof(NewNote));

            RaiseNoteEvent(NoteAddedEvent, args);
        }

        private void OnNoteUpdated(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteUpdatedEvent, e as NoteEventArgs);
        }

        private void OnNoteDeleted(object sender, RoutedEventArgs e)
        {
            var args = e as NoteEventArgs;
            Notes.Remove(args.Note);

            RaiseNoteEvent(NoteDeletedEvent, e as NoteEventArgs);
        }

        private void OnLabelSelected(object sender, RoutedEventArgs e)
        {
            RaiseLabelEvent(LabelSelectedEvent, e as LabelEventArgs);
        }

        private void OnLabelAdded(object sender, RoutedEventArgs e)
        {
            RaiseLabelEvent(LabelAddedEvent, e as LabelEventArgs);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        #region Public Properties

        /// <summary>
        /// Gets or sets the <see cref="EntityId{T}"/> that this control is operating on..
        /// </summary>
        public IId? EntityId
        {
            get => (IId)GetValue(EntityIdProperty);
            set => SetValue(EntityIdProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="EntityIdCollection"/> that this control is operating on..
        /// </summary>
        public EntityIdCollection? EntityIds
        {
            get => (EntityIdCollection)GetValue(EntityIdsProperty);
            set => SetValue(EntityIdsProperty, value);
        }

        /// <summary>
        /// Gets or sets the title (entity description) that this control is operating on..
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="Note"/>s that this control is operating on..
        /// </summary>
        public ObservableCollection<Note> Notes
        {
            get => (ObservableCollection<Note>)GetValue(NotesProperty);
            set => SetValue(NotesProperty, value);
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
        /// Gets or sets the margin for individual note instances.
        /// </summary>
        public Thickness InnerMargin
        {
            get => (Thickness)GetValue(InnerMarginProperty);
            set => SetValue(InnerMarginProperty, value);
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
        /// Gets or sets the font family for displaying the user name below the note.
        /// </summary>
        public FontFamily UserFontFamily
        {
            get => (FontFamily)GetValue(UserFontSizeProperty);
            set => SetValue(UserFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for displaying the user name below the note.
        /// </summary>
        public double UserFontSize
        {
            get => (double)GetValue(UserFontSizeProperty);
            set => SetValue(UserFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for displaying the user name below the note.
        /// </summary>
        public FontStyle UserFontStyle
        {
            get => (FontStyle)GetValue(UserFontStyleProperty);
            set => SetValue(UserFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for displaying the user name below the note.
        /// </summary>
        public FontWeight UserFontWeight
        {
            get => (FontWeight)GetValue(UserFontWeightProperty);
            set => SetValue(UserFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for displaying the user below the note.
        /// </summary>
        public Thickness UserMargin
        {
            get => (Thickness)GetValue(UserMarginProperty);
            set => SetValue(UserMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for the timestamp.
        /// </summary>
        public Thickness TimestampMargin
        {
            get => (Thickness)GetValue(TimestampMarginProperty);
            set => SetValue(TimestampMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for the timestamp.
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
        /// Gets or sets a collection of <see cref="Label"/> objects for auto selection in the control.
        /// </summary>
        public IEnumerable<NotesLabel> LabelSuggestions
        {
            get => (IEnumerable<NotesLabel>)GetValue(LabelSuggestionsProperty);
            set => SetValue(LabelSuggestionsProperty, value);
        }

        public Note NewNote { get; set; } = new Note{Text = string.Empty};

        #endregion
        #region Public Events
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
        /// Occurs when the user requests to close the notes control.
        /// </summary>
        public event RoutedEventHandler CloseRequested
        {
            add => AddHandler(CloseRequestedEvent, value);
            remove => RemoveHandler(CloseRequestedEvent, value);
        }

        /// <summary>
        /// Occurs when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        public NoteCollectionDisplay()
        {
            InitializeComponent();
        }

    }
}
