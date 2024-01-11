using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Events.Notes;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.UserControls.Notes
{
    /// <summary>
    /// A control that displays a collection of note replies.
    /// </summary>
    public partial class NoteRepliesDisplay : INotifyPropertyChanged
    {
        #region Static Routed Events

        /// <summary>
        /// Identifies the NoteSeen routed event.
        /// </summary>
        public static readonly RoutedEvent NoteSeenEvent = EventManager.RegisterRoutedEvent
            (nameof(NoteSeen), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteRepliesDisplay));

        /// <summary>
        /// Identifies the NoteReplyAdded routed event.
        /// </summary>
        public static readonly RoutedEvent NoteReplyAddedEvent = EventManager.RegisterRoutedEvent
            (nameof(NoteReplyAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteRepliesDisplay));

        #endregion Static Routed Events
        #region Static Dependency Properties

        /// <summary>
        /// Identifies the CurrentUserId dependency property.
        /// </summary>
        public static readonly DependencyProperty CurrentUserIdProperty = DependencyProperty.Register(nameof(CurrentUserId), typeof(Guid), typeof(NoteRepliesDisplay));
        
        /// <summary>
        /// Identifies the ReplyColor dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyColorProperty = DependencyProperty.Register(nameof(ReplyColor), typeof(SolidColorBrush), typeof(NoteRepliesDisplay),
            new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        /// <summary>
        /// Identifies the ReplyFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyFontFamilyProperty = DependencyProperty.Register(nameof(ReplyFontFamily), typeof(FontFamily), typeof(NoteRepliesDisplay),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the ReplyFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyFontSizeProperty = DependencyProperty.Register(nameof(ReplyFontSize), typeof(double), typeof(NoteRepliesDisplay),
            new PropertyMetadata(15d));

        /// <summary>
        /// Identifies the ReplyFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyFontStyleProperty = DependencyProperty.Register(nameof(ReplyFontStyle), typeof(FontStyle), typeof(NoteRepliesDisplay),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the ReplyFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyFontWeightProperty = DependencyProperty.Register(nameof(ReplyFontWeight), typeof(FontWeight), typeof(NoteRepliesDisplay),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the ReplyMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyMarginProperty = DependencyProperty.Register(nameof(ReplyMargin), typeof(Thickness), typeof(NoteRepliesDisplay),
            new PropertyMetadata(new Thickness(2, 2, 2, 0)));

        /// <summary>
        /// Identifies the ReplyMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyPaddingProperty = DependencyProperty.Register(nameof(ReplyPadding), typeof(Thickness), typeof(NoteRepliesDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the SeenColor dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenColorProperty = DependencyProperty.Register(nameof(SeenColor), typeof(SolidColorBrush), typeof(NoteRepliesDisplay),
            new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

        /// <summary>
        /// Identifies the SeenFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenFontFamilyProperty = DependencyProperty.Register(nameof(SeenFontFamily), typeof(FontFamily), typeof(NoteRepliesDisplay),
                new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the SeenFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenFontSizeProperty = DependencyProperty.Register(nameof(SeenFontSize), typeof(double), typeof(NoteRepliesDisplay),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the SeenFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenFontStyleProperty = DependencyProperty.Register(nameof(SeenFontStyle), typeof(FontStyle), typeof(NoteRepliesDisplay),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the SeenFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenFontWeightProperty = DependencyProperty.Register(nameof(SeenFontWeight), typeof(FontWeight), typeof(NoteRepliesDisplay),
            new PropertyMetadata(FontWeights.SemiBold));

        /// <summary>
        /// Identifies the SeenMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenMarginProperty = DependencyProperty.Register(nameof(SeenMargin), typeof(Thickness), typeof(NoteRepliesDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the SeenPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenPaddingProperty = DependencyProperty.Register(nameof(SeenPadding), typeof(Thickness), typeof(NoteRepliesDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the TimestampColor dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampColorProperty = DependencyProperty.Register(nameof(TimestampColor), typeof(SolidColorBrush), typeof(NoteRepliesDisplay),
            new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

        /// <summary>
        /// Identifies the TimestampFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampFontFamilyProperty = DependencyProperty.Register(nameof(TimestampFontFamily), typeof(FontFamily), typeof(NoteRepliesDisplay),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the TimestampFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampFontSizeProperty = DependencyProperty.Register(nameof(TimestampFontSize), typeof(double), typeof(NoteRepliesDisplay),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the TimestampFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampFontStyleProperty = DependencyProperty.Register(nameof(TimestampFontStyle), typeof(FontStyle), typeof(NoteRepliesDisplay),
            new PropertyMetadata(FontStyles.Italic));

        /// <summary>
        /// Identifies the TimestampFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampFontWeightProperty = DependencyProperty.Register(nameof(TimestampFontWeight), typeof(FontWeight), typeof(NoteRepliesDisplay),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the TimestampMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampMarginProperty = DependencyProperty.Register(nameof(TimestampMargin), typeof(Thickness), typeof(NoteRepliesDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the TimestampOrientation dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampOrientationProperty = DependencyProperty.Register(nameof(TimestampOrientation), typeof(Orientation), typeof(NoteRepliesDisplay),
            new PropertyMetadata(Orientation.Horizontal));

        /// <summary>
        /// Identifies the TimestampPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampPaddingProperty = DependencyProperty.Register(nameof(TimestampPadding), typeof(Thickness), typeof(NoteRepliesDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        #endregion
        #region Private Event Handlers

        private void RaiseReplySeenEvent(RoutedEvent routedEvent, LabelEventArgs args)
        {
            //RaiseEvent(new LabelEventArgs
            //{
            //    RoutedEvent = routedEvent,
            //    Label = args.Label,
            //    Note = Note
            //});
        }

        private void OnSeenCheckBoxChecked(object sender, RoutedEventArgs e)
        {
        }

        private void OnSeenCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
        }

        private void RaiseReplySeenEvent(NoteSeenEventArgs args)
        {
            RaiseEvent(new NoteSeenEventArgs
            {
                RoutedEvent = NoteSeenEvent,
                Seen = args.Seen,
                NoteViewModel = args.NoteViewModel
            });
        }


        private void OnNoteSeen(object sender, RoutedEventArgs e)
        {
            RaiseReplySeenEvent(e as NoteSeenEventArgs);
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Private Event Handlers
        #region Public Properties

        /// <summary>
        /// Gets or sets the ID of the current user for determining the value of the Seen flags.
        /// </summary>
        public Guid CurrentUserId
        {
            get => (Guid)GetValue(CurrentUserIdProperty);
            set => SetValue(CurrentUserIdProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground color for the reply.
        /// </summary>
        public SolidColorBrush ReplyColor
        {
            get => (SolidColorBrush)GetValue(ReplyColorProperty);
            set => SetValue(ReplyColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family for the reply.
        /// </summary>
        public FontFamily ReplyFontFamily
        {
            get => (FontFamily)GetValue(ReplyFontFamilyProperty);
            set => SetValue(ReplyFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for the reply.
        /// </summary>
        public double ReplyFontSize
        {
            get => (double)GetValue(ReplyFontSizeProperty);
            set => SetValue(ReplyFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for the reply.
        /// </summary>
        public FontStyle ReplyFontStyle
        {
            get => (FontStyle)GetValue(ReplyFontStyleProperty);
            set => SetValue(ReplyFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for the reply.
        /// </summary>
        public FontWeight ReplyFontWeight
        {
            get => (FontWeight)GetValue(ReplyFontWeightProperty);
            set => SetValue(ReplyFontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for the reply.
        /// </summary>
        public Thickness ReplyMargin
        {
            get => (Thickness)GetValue(ReplyMarginProperty);
            set => SetValue(ReplyMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for the reply.
        /// </summary>
        public Thickness ReplyPadding
        {
            get => (Thickness)GetValue(ReplyPaddingProperty);
            set => SetValue(ReplyPaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family for displaying the "seen by me" checkbox.
        /// </summary>
        public FontFamily SeenFontFamily
        {
            get => (FontFamily)GetValue(SeenFontFamilyProperty);
            set => SetValue(SeenFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for displaying the "seen by me" checkbox.
        /// </summary>
        public double SeenFontSize
        {
            get => (double)GetValue(SeenFontSizeProperty);
            set => SetValue(SeenFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for displaying the "seen by me" checkbox.
        /// </summary>
        public FontStyle SeenFontStyle
        {
            get => (FontStyle)GetValue(SeenFontStyleProperty);
            set => SetValue(SeenFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for displaying the "seen by me" checkbox.
        /// </summary>
        public FontWeight SeenFontWeight
        {
            get => (FontWeight)GetValue(SeenFontWeightProperty);
            set => SetValue(SeenFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground color for the "seen by me" checkbox.
        /// </summary>
        public SolidColorBrush SeenColor
        {
            get => (SolidColorBrush)GetValue(SeenColorProperty);
            set => SetValue(SeenColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for displaying the "seen by me" checkbox.
        /// </summary>
        public Thickness SeenMargin
        {
            get => (Thickness)GetValue(SeenMarginProperty);
            set => SetValue(SeenMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for displaying the "seen by me" checkbox.
        /// </summary>
        public Thickness SeenPadding
        {
            get => (Thickness)GetValue(SeenPaddingProperty);
            set => SetValue(SeenPaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground color for the timestamp.
        /// </summary>
        public SolidColorBrush TimestampColor
        {
            get => (SolidColorBrush)GetValue(TimestampColorProperty);
            set => SetValue(TimestampColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family for displaying the timestamp.
        /// </summary>
        public FontFamily TimestampFontFamily
        {
            get => (FontFamily)GetValue(TimestampFontFamilyProperty);
            set => SetValue(TimestampFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for displaying the timestamp.
        /// </summary>
        public double TimestampFontSize
        {
            get => (double)GetValue(TimestampFontSizeProperty);
            set => SetValue(TimestampFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for displaying the timestamp.
        /// </summary>
        public FontStyle TimestampFontStyle
        {
            get => (FontStyle)GetValue(TimestampFontStyleProperty);
            set => SetValue(TimestampFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for displaying the timestamp.
        /// </summary>
        public FontWeight TimestampFontWeight
        {
            get => (FontWeight)GetValue(TimestampFontWeightProperty);
            set => SetValue(TimestampFontStyleProperty, value);
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
        /// Gets or sets the orientation for the timestamp.
        /// </summary>
        public Orientation TimestampOrientation
        {
            get => (Orientation)GetValue(TimestampOrientationProperty);
            set => SetValue(TimestampOrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for the timestamp.
        /// </summary>
        public Thickness TimestampPadding
        {
            get => (Thickness)GetValue(TimestampPaddingProperty);
            set => SetValue(TimestampPaddingProperty, value);
        }

        #endregion
        #region Public Events

        /// <summary>
        /// Occurs when the "seen by me" checkbox is checked or unchecked.
        /// </summary>
        public event RoutedEventHandler NoteSeen
        {
            add => AddHandler(NoteSeenEvent, value);
            remove => RemoveHandler(NoteSeenEvent, value);
        }

        /// <summary>
        /// Occurs when a new reply is added to a note.
        /// </summary>
        public event RoutedEventHandler NoteReplyAdded
        {
            add => AddHandler(NoteReplyAddedEvent, value);
            remove => RemoveHandler(NoteReplyAddedEvent, value);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion Public events

        public NoteRepliesDisplay()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty NoteViewModelWithRepliesProperty = DependencyProperty.Register(nameof(NoteViewModelWithReplies), typeof(NoteViewModel), typeof(NoteRepliesDisplay));
        public NoteViewModel NoteViewModelWithReplies
        {
            get
            {
                return (NoteViewModel)GetValue(NoteViewModelWithRepliesProperty);
            }
            set
            {
                SetValue(NoteViewModelWithRepliesProperty, value);
                NoteViewModelWithRepliesCollectionView = CollectionViewSource.GetDefaultView(value.Replies);
                NoteViewModelWithRepliesCollectionView.SortDescriptions.Clear();
                NoteViewModelWithRepliesCollectionView.SortDescriptions.Add(new SortDescription("Created", ListSortDirection.Ascending));
            }
        }

        public static readonly DependencyProperty NoteViewModelWithRepliesCollectionViewProperty = DependencyProperty.Register(nameof(NoteViewModelWithRepliesCollectionView), typeof(ICollectionView), typeof(NoteRepliesDisplay));
        public ICollectionView NoteViewModelWithRepliesCollectionView
        {
            get => (ICollectionView)GetValue(NoteViewModelWithRepliesCollectionViewProperty);
            set => SetValue(NoteViewModelWithRepliesCollectionViewProperty, value);
        }

        private async void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = e.Source as CheckBox;
            var noteViewModel = checkBox?.DataContext as NoteViewModel;
            if (noteViewModel != null)
            {
                RaiseEvent(new NoteSeenEventArgs
                {
                    RoutedEvent = NoteSeenEvent,
                    Seen = true,
                    NoteViewModel = noteViewModel
                });
            }
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            var checkBox = e.Source as CheckBox;
            var noteViewModel = checkBox?.DataContext as NoteViewModel;
            if (noteViewModel != null)
            {
                RaiseEvent(new NoteSeenEventArgs
                {
                    RoutedEvent = NoteSeenEvent,
                    Seen = false,
                    NoteViewModel = noteViewModel
                });
            }
        }
        private void AddReplyText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                RaiseEvent(new NoteReplyAddEventArgs
                {
                    RoutedEvent = NoteReplyAddedEvent,
                    Text = ((TextBox)e.Source).Text,
                    NoteViewModelWithReplies = NoteViewModelWithReplies
                });

                NoteReplyTextBox.Text = string.Empty;
            }
        }

    }
}
