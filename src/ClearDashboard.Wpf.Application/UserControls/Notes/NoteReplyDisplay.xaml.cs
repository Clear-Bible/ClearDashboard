using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Events.Notes;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using TimeZoneNames;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Windows.FontStyle;

namespace ClearDashboard.Wpf.Application.UserControls.Notes
{
    /// <summary>
    /// A control that displays the details of a single note reply.
    /// </summary>
    public partial class NoteReplyDisplay : INotifyPropertyChanged, IHandle<NoteUpdatedMessage>
    {
        #region Static Routed Events

        /// <summary>
        /// Identifies the NoteSeen routed event.
        /// </summary>
        public static readonly RoutedEvent NoteSeenEvent = EventManager.RegisterRoutedEvent
            (nameof(NoteSeen), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteReplyDisplay));

        #endregion Static Routed Events
        #region Static Dependency Properties

        /// <summary>
        /// Identifies the CurrentUserId dependency property.
        /// </summary>
        public static readonly DependencyProperty CurrentUserIdProperty = DependencyProperty.Register(nameof(CurrentUserId), typeof(Guid), typeof(NoteReplyDisplay));

        /// <summary>
        /// Identifies the ParentNote dependency property.
        /// </summary>
        public static readonly DependencyProperty ParentNoteProperty = DependencyProperty.Register(nameof(ParentNote), typeof(NoteViewModel), typeof(NoteReplyDisplay));

        /// <summary>
        /// Identifies the ReplyColor dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyColorProperty = DependencyProperty.Register(nameof(ReplyColor), typeof(SolidColorBrush), typeof(NoteReplyDisplay),
            new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        /// <summary>
        /// Identifies the ReplyFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyFontFamilyProperty = DependencyProperty.Register(nameof(ReplyFontFamily), typeof(FontFamily), typeof(NoteReplyDisplay),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the ReplyFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyFontSizeProperty = DependencyProperty.Register(nameof(ReplyFontSize), typeof(double), typeof(NoteReplyDisplay),
            new PropertyMetadata(15d));

        /// <summary>
        /// Identifies the ReplyFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyFontStyleProperty = DependencyProperty.Register(nameof(ReplyFontStyle), typeof(FontStyle), typeof(NoteReplyDisplay),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the ReplyFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyFontWeightProperty = DependencyProperty.Register(nameof(ReplyFontWeight), typeof(FontWeight), typeof(NoteReplyDisplay),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the ReplyMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyMarginProperty = DependencyProperty.Register(nameof(ReplyMargin), typeof(Thickness), typeof(NoteReplyDisplay),
            new PropertyMetadata(new Thickness(2, 2, 2, 0)));

        /// <summary>
        /// Identifies the ReplyMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty ReplyPaddingProperty = DependencyProperty.Register(nameof(ReplyPadding), typeof(Thickness), typeof(NoteReplyDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the SeenColor dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenColorProperty = DependencyProperty.Register(nameof(SeenColor), typeof(SolidColorBrush), typeof(NoteReplyDisplay),
            new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

        /// <summary>
        /// Identifies the SeenFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenFontFamilyProperty = DependencyProperty.Register(nameof(SeenFontFamily), typeof(FontFamily), typeof(NoteReplyDisplay),
                new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the SeenFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenFontSizeProperty = DependencyProperty.Register(nameof(SeenFontSize), typeof(double), typeof(NoteReplyDisplay),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the SeenFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenFontStyleProperty = DependencyProperty.Register(nameof(SeenFontStyle), typeof(FontStyle), typeof(NoteReplyDisplay),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the SeenFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenFontWeightProperty = DependencyProperty.Register(nameof(SeenFontWeight), typeof(FontWeight), typeof(NoteReplyDisplay),
            new PropertyMetadata(FontWeights.SemiBold));

        /// <summary>
        /// Identifies the SeenMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenMarginProperty = DependencyProperty.Register(nameof(SeenMargin), typeof(Thickness), typeof(NoteReplyDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the SeenPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty SeenPaddingProperty = DependencyProperty.Register(nameof(SeenPadding), typeof(Thickness), typeof(NoteReplyDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the TimestampColor dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampColorProperty = DependencyProperty.Register(nameof(TimestampColor), typeof(SolidColorBrush), typeof(NoteReplyDisplay),
            new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

        /// <summary>
        /// Identifies the TimestampFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampFontFamilyProperty = DependencyProperty.Register(nameof(TimestampFontFamily), typeof(FontFamily), typeof(NoteReplyDisplay),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the TimestampFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampFontSizeProperty = DependencyProperty.Register(nameof(TimestampFontSize), typeof(double), typeof(NoteReplyDisplay),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the TimestampFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampFontStyleProperty = DependencyProperty.Register(nameof(TimestampFontStyle), typeof(FontStyle), typeof(NoteReplyDisplay),
            new PropertyMetadata(FontStyles.Italic));

        /// <summary>
        /// Identifies the TimestampFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampFontWeightProperty = DependencyProperty.Register(nameof(TimestampFontWeight), typeof(FontWeight), typeof(NoteReplyDisplay),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the TimestampMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampMarginProperty = DependencyProperty.Register(nameof(TimestampMargin), typeof(Thickness), typeof(NoteReplyDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the TimestampOrientation dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampOrientationProperty = DependencyProperty.Register(nameof(TimestampOrientation), typeof(Orientation), typeof(NoteReplyDisplay),
            new PropertyMetadata(Orientation.Horizontal));

        /// <summary>
        /// Identifies the TimestampPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty TimestampPaddingProperty = DependencyProperty.Register(nameof(TimestampPadding), typeof(Thickness), typeof(NoteReplyDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        #endregion
        #region Private Event Handlers

        private void RaiseReplySeenEvent(bool seen)
        {
            RaiseEvent(new NoteSeenEventArgs
            {
                RoutedEvent = NoteSeenEvent,
                Seen = seen,
                NoteViewModel = Reply
            });
        }

        private void OnSeenCheckBoxClicked(object sender, RoutedEventArgs e)
        {
            RaiseReplySeenEvent(SeenCheckBox.IsChecked.Value);
        }

        private void OnSeenCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            RaiseReplySeenEvent(true);
        }

        private void OnSeenCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            RaiseReplySeenEvent(false);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Private Event Handlers
        #region Public Properties

        /// <summary>
        /// Gets or sets the <see cref="EventAggregator"/> to be used for participating in the Caliburn Micro eventing system.
        /// </summary>
        public static IEventAggregator? EventAggregator { get; set; }

        /// <summary>
        /// Gets or sets the ID of the current user for determining the value of the Seen flags.
        /// </summary>
        public Guid CurrentUserId
        {
            get => (Guid)GetValue(CurrentUserIdProperty);
            set => SetValue(CurrentUserIdProperty, value);
        }
        
        /// <summary>
        /// Gets or sets the parent <see cref="NoteViewModel"/> for the reply.
        /// </summary>
        public NoteViewModel ParentNote
        {
            get => (NoteViewModel)GetValue(ParentNoteProperty);
            set => SetValue(ParentNoteProperty, value);
        }

        public NoteViewModel? Reply => DataContext as NoteViewModel;
        public string ReplyText => Reply != null ? Reply.Text : string.Empty;
        public string ReplyAuthor => Reply != null ? Reply.ModifiedBy : string.Empty;

        public string ReplyTimestamp
        {
            get
            {
                var localTimeZone = TimeZoneInfo.Local;
                var localTimeZoneAbbreviations = TZNames.GetAbbreviationsForTimeZone(localTimeZone.Id, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
                var localTimeZoneAbbreviation = localTimeZone.IsDaylightSavingTime(DateTime.Now) ? localTimeZoneAbbreviations.Daylight : localTimeZoneAbbreviations.Standard;
                var localTime = Reply?.NoteId?.Created?.ToLocalTime() ?? null;

                return localTime != null ? $"{localTime.Value:d} {localTime.Value:hhmm} {localTimeZoneAbbreviation}" : string.Empty;
            }
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

        /// <summary>
        /// Gets or sets the foreground color for the "seen by me" checkbox.
        /// </summary>
        public SolidColorBrush SeenColor
        {
            get => (SolidColorBrush)GetValue(SeenColorProperty);
            set => SetValue(SeenColorProperty, value);
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
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion Public events

        public NoteReplyDisplay()
        {
            InitializeComponent();

            EventAggregator?.SubscribeOnUIThread(this);
        }

        public async Task HandleAsync(NoteUpdatedMessage message, CancellationToken cancellationToken)
        {
            if (message.NoteId == Reply?.NoteId)
            {
                //Reply.NoteSeenChanged();
            }
            await Task.CompletedTask;
        }

        ~NoteReplyDisplay()
        {
            EventAggregator?.Unsubscribe(this);
        }
    }
}
