using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control for displaying a verse, as represented by an IEnumerable of <see cref="TokenDisplayViewModel" /> instances.
    /// </summary>
    public partial class VerseDisplay : UserControl
    {
        #region Static RoutedEvents
        /// <summary>
        /// Identifies the TokenClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenClickedEvent = EventManager.RegisterRoutedEvent
            ("TokenClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenDoubleClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenDoubleClickedEvent = EventManager.RegisterRoutedEvent
            ("TokenDoubleClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TokenLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TokenLeftButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenRightButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TokenRightButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenRightButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TokenRightButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseEnterEvent = EventManager.RegisterRoutedEvent
            ("TokenMouseEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseLeaveEvent = EventManager.RegisterRoutedEvent
            ("TokenMouseLeave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseWheelEvent = EventManager.RegisterRoutedEvent
            ("TokenMouseWheel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationClickedEvent = EventManager.RegisterRoutedEvent
            ("TranslationClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationDoubleClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationDoubleClickedEvent = EventManager.RegisterRoutedEvent
            ("TranslationDoubleClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TranslationLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TranslationLeftButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationRightButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TranslationRightButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationRightButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TranslationRightButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationMouseEnterEvent = EventManager.RegisterRoutedEvent
            ("TranslationMouseEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationMouseLeaveEvent = EventManager.RegisterRoutedEvent
            ("TranslationMouseLeave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationMouseWheelEvent = EventManager.RegisterRoutedEvent
            ("TranslationMouseWheel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            ("NoteLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            ("NoteLeftButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteRightButtonDownEvent = EventManager.RegisterRoutedEvent
            ("NoteRightButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteRightButtonUpEvent = EventManager.RegisterRoutedEvent
            ("NoteRightButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseEnterEvent = EventManager.RegisterRoutedEvent
            ("NoteMouseEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseLeaveEvent = EventManager.RegisterRoutedEvent
            ("NoteMouseLeave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseWheelEvent = EventManager.RegisterRoutedEvent
            ("NoteMouseWheel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));        
        
        /// <summary>
        /// Identifies the NoteCreateEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteCreateEvent = EventManager.RegisterRoutedEvent
            ("NoteCreate", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));
        #endregion
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the Title Visibiilty dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleVisibilityProperty = DependencyProperty.Register("TitleVisibility", typeof(Visibility), typeof(VerseDisplay), new PropertyMetadata(Visibility.Visible));


        /// <summary>
        /// Identifies the TitlePadding dependency property.
        /// </summary>
        public static readonly DependencyProperty TitlePaddingProperty = DependencyProperty.Register("TitlePadding", typeof(Thickness), typeof(VerseDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the TitleMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleMarginProperty = DependencyProperty.Register("TitleMargin", typeof(Thickness), typeof(VerseDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the TitleFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register("TitleFontSize", typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(16d));

        /// <summary>
        /// Identifies the TitleHorizontalAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleHorizontalAlignmentProperty = DependencyProperty.Register("TitleHorizontalAlignment", typeof(HorizontalAlignment), typeof(VerseDisplay),
            new PropertyMetadata(HorizontalAlignment.Left));

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the Wrap dependency property.
        /// </summary>
        public static readonly DependencyProperty WrapProperty = DependencyProperty.Register("Wrap", typeof(bool), typeof(VerseDisplay),
            new PropertyMetadata(true, new PropertyChangedCallback(OnWrapChanged)));

        /// <summary>
        /// Callback handler for the Wrap dependency property: when the Wrap value changes, update the <see cref="ItemsPanelTemplate"/>.
        /// </summary>
        /// <param name="obj">The object whose TranslationVerticalSpacing has changed.</param>
        /// <param name="args">Event args containing the new value.</param>
        private static void OnWrapChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (VerseDisplay)obj;
            control.CalculateItemsPanelTemplate((bool) args.NewValue);
        }

        /// <summary>
        /// Identifies the ItemsPanelTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsPanelTemplateProperty = DependencyProperty.Register("ItemsPanelTemplate", typeof(ItemsPanelTemplate), typeof(VerseDisplay));
        
        /// <summary>
        /// Identifies the Tokens dependency property.
        /// </summary>
        public static readonly DependencyProperty TokensProperty = DependencyProperty.Register("Tokens", typeof(IEnumerable), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the HorizontalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register("HorizontalSpacing", typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(10d));

        /// <summary>
        /// Identifies the TokenVerticalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenVerticalSpacingProperty = DependencyProperty.Register("TokenVerticalSpacing", typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(4d));

        /// <summary>
        /// Identifies the TranslationFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontSizeProperty = DependencyProperty.Register("TranslationFontSize", typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(16d));

        /// <summary>
        /// Identifies the TranslationAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationAlignmentProperty = DependencyProperty.Register("TranslationAlignment", typeof(HorizontalAlignment), typeof(VerseDisplay),
            new PropertyMetadata(HorizontalAlignment.Center));

        /// <summary>
        /// Identifies the TranslationVerticalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationVerticalSpacingProperty = DependencyProperty.Register("TranslationVerticalSpacing", typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(10d));

        /// <summary>
        /// Identifies the NoteIndicatorHeight dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorHeightProperty = DependencyProperty.Register("NoteIndicatorHeight", typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(3d));

        /// <summary>
        /// Identifies the NoteIndicatorColor dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorColorProperty = DependencyProperty.Register("NoteIndicatorColor", typeof(Brush), typeof(VerseDisplay),
            new PropertyMetadata(Brushes.LightGray));

        /// <summary>
        /// Identifies the ShowTranslations dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowTranslationsProperty = DependencyProperty.Register("ShowTranslations", typeof(bool), typeof(VerseDisplay),
            new PropertyMetadata(true));

        /// <summary>
        /// Identifies the ShowNoteIndicators dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowNoteIndicatorsProperty = DependencyProperty.Register("ShowNoteIndicators", typeof(bool), typeof(VerseDisplay),
            new PropertyMetadata(true));

        #endregion Static DependencyProperties
        #region Private event handlers

        private void CalculateItemsPanelTemplate(bool wrap)
        {
            ItemsPanelTemplate = (ItemsPanelTemplate)FindResource(wrap ? "WrapPanelTemplate" : "StackPanelTemplate");
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CalculateItemsPanelTemplate(Wrap);
        }

        private void RaiseTokenEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            var control = e.Source as FrameworkElement;
            var tokenDisplay = control?.DataContext as TokenDisplayViewModel;
            RaiseEvent(new TokenEventArgs
            {
                RoutedEvent = routedEvent,
                TokenDisplayViewModel = tokenDisplay
            });
        }

        private void OnTokenClicked(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenClickedEvent, e);
        }

        private void OnTokenDoubleClicked(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenDoubleClickedEvent, e);
        }

        private void OnTokenLeftButtonDown(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenLeftButtonDownEvent, e);
        }

        private void OnTokenLeftButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenRightButtonUpEvent, e);
        }
        private void OnTokenRightButtonDown(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenRightButtonDownEvent, e);
        }

        private void OnTokenRightButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenRightButtonUpEvent, e);
        }

        private void OnTokenMouseEnter(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenMouseEnterEvent, e);
        }

        private void OnTokenMouseLeave(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenMouseLeaveEvent, e);
        }

        private void OnTokenMouseWheel(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenMouseWheelEvent, e);
        }

        private void RaiseTranslationEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            var control = e.Source as TokenDisplay;
            RaiseEvent(new TranslationEventArgs
            {
                RoutedEvent = routedEvent,
                TokenDisplayViewModel = control?.TokenDisplayViewModel,
                Translation = control?.TokenDisplayViewModel?.Translation
            });
        }

        private void OnTranslationClicked(object sender, RoutedEventArgs e)
        {
            RaiseTranslationEvent(TranslationClickedEvent, e);
        }

        private void OnTranslationDoubleClicked(object sender, RoutedEventArgs e)
        {
            RaiseTranslationEvent(TranslationDoubleClickedEvent, e);
        }

        private void OnTranslationLeftButtonDown(object sender, RoutedEventArgs e)
        {
            RaiseTranslationEvent(TranslationLeftButtonDownEvent, e);
        }

        private void OnTranslationLeftButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseTranslationEvent(TranslationRightButtonUpEvent, e);
        }
        private void OnTranslationRightButtonDown(object sender, RoutedEventArgs e)
        {
            RaiseTranslationEvent(TranslationRightButtonDownEvent, e);
        }

        private void OnTranslationRightButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseTranslationEvent(TranslationRightButtonUpEvent, e);
        }

        private void OnTranslationMouseEnter(object sender, RoutedEventArgs e)
        {
            RaiseTranslationEvent(TranslationMouseEnterEvent, e);
        }

        private void OnTranslationMouseLeave(object sender, RoutedEventArgs e)
        {
            RaiseTranslationEvent(TranslationMouseLeaveEvent, e);
        }

        private void OnTranslationMouseWheel(object sender, RoutedEventArgs e)
        {
            RaiseTranslationEvent(TranslationMouseWheelEvent, e);
        }

        private void RaiseNoteEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            var control = e.Source as TokenDisplay;
            RaiseEvent(new NoteEventArgs
            {
                RoutedEvent = routedEvent,
                //TokenDisplayViewModel = control?.TokenDisplayViewModel
            });
        }

        private void OnNoteLeftButtonDown(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteLeftButtonDownEvent, e);
        }

        private void OnNoteLeftButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteRightButtonUpEvent, e);
        }
        private void OnNoteRightButtonDown(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteRightButtonDownEvent, e);
        }

        private void OnNoteRightButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteRightButtonUpEvent, e);
        }

        private void OnNoteMouseEnter(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteMouseEnterEvent, e);
        }

        private void OnNoteMouseLeave(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteMouseLeaveEvent, e);
        }

        private void OnNoteMouseWheel(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteMouseWheelEvent, e);
        }

        private void OnNoteCreate(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteCreateEvent, e);
        }

        #endregion
        #region Public events

        /// <summary>
        /// Occurs when an individual token is clicked.
        /// </summary>
        public event RoutedEventHandler TokenClicked
        {
            add => AddHandler(TokenClickedEvent, value);
            remove => RemoveHandler(TokenClickedEvent, value);
        }

        /// <summary>
        /// Occurs when an individual token is clicked two or more times.
        /// </summary>
        public event RoutedEventHandler TokenDoubleClicked
        {
            add => AddHandler(TokenDoubleClickedEvent, value);
            remove => RemoveHandler(TokenDoubleClickedEvent, value);
        }

        /// <summary>
        /// Occurs when the left mouse button is pressed while the mouse pointer is over a token.
        /// </summary>
        public event RoutedEventHandler TokenLeftButtonDown
        {
            add => AddHandler(TokenLeftButtonDownEvent, value);
            remove => RemoveHandler(TokenLeftButtonDownEvent, value);
        }

        /// <summary>
        /// Occurs when the left mouse button is released while the mouse pointer is over a token.
        /// </summary>
        public event RoutedEventHandler TokenLeftButtonUp
        {
            add => AddHandler(TokenLeftButtonUpEvent, value);
            remove => RemoveHandler(TokenLeftButtonUpEvent, value);
        }

        /// <summary>
        /// Occurs when the right mouse button is pressed while the mouse pointer is over a token.
        /// </summary>
        public event RoutedEventHandler TokenRightButtonDown
        {
            add => AddHandler(TokenRightButtonDownEvent, value);
            remove => RemoveHandler(TokenRightButtonDownEvent, value);
        }

        /// <summary>
        /// Occurs when the right mouse button is released while the mouse pointer is over a token.
        /// </summary>
        public event RoutedEventHandler TokenRightButtonUp
        {
            add => AddHandler(TokenRightButtonUpEvent, value);
            remove => RemoveHandler(TokenRightButtonUpEvent, value);
        }

        /// <summary>
        /// Occurs when the mouse pointer enters the bounds of a token.
        /// </summary>
        public event RoutedEventHandler TokenMouseEnter
        {
            add => AddHandler(TokenMouseEnterEvent, value);
            remove => RemoveHandler(TokenMouseEnterEvent, value);
        }

        /// <summary>
        /// Occurs when the mouse pointer leaves the bounds of a token.
        /// </summary>
        public event RoutedEventHandler TokenMouseLeave
        {
            add => AddHandler(TokenMouseLeaveEvent, value);
            remove => RemoveHandler(TokenMouseLeaveEvent, value);
        }

        /// <summary>
        /// Occurs when the user rotates the mouse wheel while the mouse pointer is over a token.
        /// </summary>
        public event RoutedEventHandler TokenMouseWheel
        {
            add => AddHandler(TokenMouseWheelEvent, value);
            remove => RemoveHandler(TokenMouseWheelEvent, value);
        }

        /// <summary>
        /// Occurs when an individual translation is clicked.
        /// </summary>
        public event RoutedEventHandler TranslationClicked
        {
            add => AddHandler(TranslationClickedEvent, value);
            remove => RemoveHandler(TranslationClickedEvent, value);
        }

        /// <summary>
        /// Occurs when an individual translation is clicked two or more times.
        /// </summary>
        public event RoutedEventHandler TranslationDoubleClicked
        {
            add => AddHandler(TranslationDoubleClickedEvent, value);
            remove => RemoveHandler(TranslationDoubleClickedEvent, value);
        }

        /// <summary>
        /// Occurs when the left mouse button is pressed while the mouse pointer is over a translation.
        /// </summary>
        public event RoutedEventHandler TranslationLeftButtonDown
        {
            add => AddHandler(TranslationLeftButtonDownEvent, value);
            remove => RemoveHandler(TranslationLeftButtonDownEvent, value);
        }

        /// <summary>
        /// Occurs when the left mouse button is released while the mouse pointer is over a translation.
        /// </summary>
        public event RoutedEventHandler TranslationLeftButtonUp
        {
            add => AddHandler(TranslationLeftButtonUpEvent, value);
            remove => RemoveHandler(TranslationLeftButtonUpEvent, value);
        }

        /// <summary>
        /// Occurs when the right mouse button is pressed while the mouse pointer is over a translation.
        /// </summary>
        public event RoutedEventHandler TranslationRightButtonDown
        {
            add => AddHandler(TranslationRightButtonDownEvent, value);
            remove => RemoveHandler(TranslationRightButtonDownEvent, value);
        }

        /// <summary>
        /// Occurs when the right mouse button is released while the mouse pointer is over a translation.
        /// </summary>
        public event RoutedEventHandler TranslationRightButtonUp
        {
            add => AddHandler(TranslationRightButtonUpEvent, value);
            remove => RemoveHandler(TranslationRightButtonUpEvent, value);
        }

        /// <summary>
        /// Occurs when the mouse pointer enters the bounds of a translation.
        /// </summary>
        public event RoutedEventHandler TranslationMouseEnter
        {
            add => AddHandler(TranslationMouseEnterEvent, value);
            remove => RemoveHandler(TranslationMouseEnterEvent, value);
        }

        /// <summary>
        /// Occurs when the mouse pointer leaves the bounds of a translation.
        /// </summary>
        public event RoutedEventHandler TranslationMouseLeave
        {
            add => AddHandler(TranslationMouseLeaveEvent, value);
            remove => RemoveHandler(TranslationMouseLeaveEvent, value);
        }

        /// <summary>
        /// Occurs when the user rotates the mouse wheel while the mouse pointer is over a translation.
        /// </summary>
        public event RoutedEventHandler TranslationMouseWheel
        {
            add => AddHandler(TranslationMouseWheelEvent, value);
            remove => RemoveHandler(TranslationMouseWheelEvent, value);
        }

        /// <summary>
        /// Occurs when the left mouse button is pressed while the mouse pointer is over a note indicator.
        /// </summary>
        public event RoutedEventHandler NoteLeftButtonDown
        {
            add => AddHandler(NoteLeftButtonDownEvent, value);
            remove => RemoveHandler(NoteLeftButtonDownEvent, value);
        }

        /// <summary>
        /// Occurs when the left mouse button is released while the mouse pointer is over a note indicator.
        /// </summary>
        public event RoutedEventHandler NoteLeftButtonUp
        {
            add => AddHandler(NoteLeftButtonUpEvent, value);
            remove => RemoveHandler(NoteLeftButtonUpEvent, value);
        }

        /// <summary>
        /// Occurs when the right mouse button is pressed while the mouse pointer is over a note indicator.
        /// </summary>
        public event RoutedEventHandler NoteRightButtonDown
        {
            add => AddHandler(NoteRightButtonDownEvent, value);
            remove => RemoveHandler(NoteRightButtonDownEvent, value);
        }

        /// <summary>
        /// Occurs when the right mouse button is released while the mouse pointer is over a note indicator.
        /// </summary>
        public event RoutedEventHandler NoteRightButtonUp
        {
            add => AddHandler(NoteRightButtonUpEvent, value);
            remove => RemoveHandler(NoteRightButtonUpEvent, value);
        }

        /// <summary>
        /// Occurs when the mouse pointer enters the bounds of a note indicator.
        /// </summary>
        public event RoutedEventHandler NoteMouseEnter
        {
            add => AddHandler(NoteMouseEnterEvent, value);
            remove => RemoveHandler(NoteMouseEnterEvent, value);
        }

        /// <summary>
        /// Occurs when the mouse pointer leaves the bounds of a note indicator.
        /// </summary>
        public event RoutedEventHandler NoteMouseLeave
        {
            add => AddHandler(NoteMouseLeaveEvent, value);
            remove => RemoveHandler(NoteMouseLeaveEvent, value);
        }

        /// <summary>
        /// Occurs when the user rotates the mouse wheel while the mouse pointer is over a note indicator.
        /// </summary>
        public event RoutedEventHandler NoteMouseWheel
        {
            add => AddHandler(NoteMouseWheelEvent, value);
            remove => RemoveHandler(NoteMouseWheelEvent, value);
        }

        /// <summary>
        /// Occurs when the user requests to create a new note.
        /// </summary>
        public event RoutedEventHandler NoteCreate
        {
            add => AddHandler(NoteCreateEvent, value);
            remove => RemoveHandler(NoteCreateEvent, value);
        }

        #endregion
        #region Public properties
        /// <summary>
        /// Gets or sets the title to be displayed for the verse.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Gets or sets the title to be displayed for the verse.
        /// </summary>
        public Visibility TitleVisibility
        {
            get => (Visibility)GetValue(TitleVisibilityProperty);
            set => SetValue(TitleVisibilityProperty, value);
        }


        /// <summary>
        /// Gets or sets the padding of the title to be displayed for the verse.
        /// </summary>
        public Thickness TitlePadding
        {
            get => (Thickness)GetValue(TitlePaddingProperty);
            set => SetValue(TitlePaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin of the title to be displayed for the verse.
        /// </summary>
        public Thickness TitleMargin
        {
            get => (Thickness)GetValue(TitleMarginProperty);
            set => SetValue(TitleMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for the title.
        /// </summary>
        public double TitleFontSize
        {
            get => (double)GetValue(TitleFontSizeProperty);
            set => SetValue(TitleFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the horizontal alignment for the title.
        /// </summary>
        public HorizontalAlignment TitleHorizontalAlignment
        {
            get => (HorizontalAlignment)GetValue(TitleHorizontalAlignmentProperty);
            set => SetValue(TitleHorizontalAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the orientation for displaying the tokens.
        /// </summary>
        /// <remarks>
        /// This controls the layout of the tokens to be displayed; regardless of this setting, any translation will be displayed vertically below the token.
        /// </remarks>
        public Orientation Orientation
        {
            get => (Orientation) GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the tokens should wrap in the control.
        /// </summary>
        public bool Wrap
        {
            get => (bool)GetValue(WrapProperty);
            set => SetValue(WrapProperty, value);
        }           

        /// <summary>
        /// Gets or sets whether the <see cref="ItemsPanelTemplate"/> to use when rendering the control.
        /// </summary>
        /// <remarks>This should normally not be set directly, as it is determined by the value of the <see cref="Wrap"/> property.</remarks>
        private ItemsPanelTemplate ItemsPanelTemplate
        {
            get => (ItemsPanelTemplate)GetValue(ItemsPanelTemplateProperty);
            set => SetValue(ItemsPanelTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="TokenDisplayViewModel"/> objects to display in the control.
        /// </summary>
        public IEnumerable Tokens
        {
            get => (IEnumerable)GetValue(TokensProperty);
            set => SetValue(TokensProperty, value);
        }

        /// <summary>
        /// Gets or sets the horizontal spacing between translations.
        /// </summary>
        /// <remarks>
        /// This is a relative factor that will ultimately depend on the token's <see cref="TokenDisplayViewModel.PaddingBefore"/> and <see cref="TokenDisplayViewModel.PaddingAfter"/> values.
        /// </remarks>
        public double HorizontalSpacing
        {
            get => (double)GetValue(HorizontalSpacingProperty);
            set => SetValue(HorizontalSpacingProperty, value);
        }

        /// <summary>
        /// Gets or sets the vertical spacing below the token.
        /// </summary>
        public double TokenVerticalSpacing
        {
            get => (double)GetValue(TokenVerticalSpacingProperty);
            set => SetValue(TokenVerticalSpacingProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for the translation.
        /// </summary>
        public double TranslationFontSize
        {
            get => (double)GetValue(TranslationFontSizeProperty);
            set => SetValue(TranslationFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="HorizontalAlignment"/> for the token and translation.
        /// </summary>
        public HorizontalAlignment TranslationAlignment
        {
            get => (HorizontalAlignment)GetValue(HorizontalAlignmentProperty);
            set => SetValue(HorizontalAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the vertical spacing below the translation.
        /// </summary>
        public double TranslationVerticalSpacing
        {
            get => (double)GetValue(TranslationVerticalSpacingProperty);
            set => SetValue(TranslationVerticalSpacingProperty, value);
        }

        /// <summary>
        /// Gets or sets whether to display token translations.
        /// </summary>
        public bool ShowTranslations
        {
            get => (bool)GetValue(ShowTranslationsProperty);
            set => SetValue(ShowTranslationsProperty, value);
        }

        /// <summary>
        /// Gets or sets the height of the note indicator.
        /// </summary>
        public double NoteIndicatorHeight
        {
            get => (double)GetValue(NoteIndicatorHeightProperty);
            set => SetValue(NoteIndicatorHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the note indicator.
        /// </summary>
        public Brush NoteIndicatorColor
        {
            get => (Brush)GetValue(NoteIndicatorColorProperty);
            set => SetValue(NoteIndicatorColorProperty, value);
        }

        /// <summary>
        /// Gets or sets whether to display note indicators.
        /// </summary>
        public bool ShowNoteIndicators
        {
            get => (bool)GetValue(ShowNoteIndicatorsProperty);
            set => SetValue(ShowNoteIndicatorsProperty, value);
        }
        #endregion Public properties

        public VerseDisplay()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }
    }
}
