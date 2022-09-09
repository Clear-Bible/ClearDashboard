using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.Wpf.Application.ViewModels.Display;
using SIL.Machine.Tokenization;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control for displaying a verse, as represented by an IEnumerable of <see cref="TokenDisplay" /> instances.
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
        #endregion
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the Wrap dependency property.
        /// </summary>
        public static readonly DependencyProperty WrapProperty = DependencyProperty.Register("Wrap", typeof(bool), typeof(VerseDisplay));
        
        public static readonly DependencyProperty TokensTextRowProperty = DependencyProperty.Register("TokensTextRow", typeof(TokensTextRow), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the Tokens dependency property.
        /// </summary>
        public static readonly DependencyProperty TokensProperty = DependencyProperty.Register("Tokens", typeof(IEnumerable), typeof(VerseDisplay));
        public static readonly DependencyProperty TranslationsProperty = DependencyProperty.Register("Translations", typeof(IEnumerable), typeof(VerseDisplay));

        public static readonly DependencyProperty TranslationFontSizeProperty = DependencyProperty.Register("TranslationFontSize", typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(16d));

        public static readonly DependencyProperty TranslationVerticalSpacingProperty = DependencyProperty.Register("TranslationVerticalSpacing", typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(10d));

        #endregion Static DependencyProperties

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

        #endregion
        #region Event handlers

        private void RaiseTokenEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            var control = e.Source as FrameworkElement;
            var tokenDisplay = control?.DataContext as TokenDisplay;
            RaiseEvent(new TokenEventArgs
            {
                RoutedEvent = routedEvent,
                TokenDisplay = tokenDisplay
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

        #endregion

        /// <summary>
        /// Gets or sets the orientation for displaying the tokens.
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation) GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the words should wrap in the control.
        /// </summary>
        public bool Wrap
        {
            get => (bool)GetValue(WrapProperty);
            set => SetValue(WrapProperty, value);
        }           
        
        public TokensTextRow TokensTextRow
        {
            get => (TokensTextRow)GetValue(TokensTextRowProperty);
            set => SetValue(TokensTextRowProperty, value);
        }        

        public ItemsPanelTemplate ItemsPanelTemplate => (ItemsPanelTemplate) FindResource(Wrap ? "WrapPanelTemplate" : "StackPanelTemplate");

        //public Brush WordBorderBrush => ParagraphMode ? Brushes.Transparent : (Brush) FindResource("MaterialDesignBody");

        //private Thickness ParagraphMargin = new(0,0,0,0);
        //private Thickness StackMargin = new(6,2,6,2);
        //public Thickness InnerMargin => ParagraphMode ? ParagraphMargin : StackMargin;        

        //private Thickness ParagraphPadding = new(5,0,5,0);
        //private Thickness StackPadding = new(10,2,10,2);
        //public Thickness InnerPadding => ParagraphMode ? ParagraphPadding : StackPadding;


        public VerseDisplay()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets a collection to display in the control.
        /// </summary>
        //public IEnumerable ItemsSource
        //{
        //    get => (IEnumerable)GetValue(ItemsSourceProperty);
        //    set
        //    {
        //        SetValue(ItemsSourceProperty, value);
        //        Tokens = Detokenize(value);
        //    }
        //}

        /// <summary>
        /// Gets or sets a collection of <see cref="TokenDisplay"/> objects to display in the control.
        /// </summary>
        public IEnumerable Tokens
        {
            get => (IEnumerable)GetValue(TokensProperty);
            set => SetValue(TokensProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection to display in the control.
        /// </summary>
        public IEnumerable Translations
        {
            get => (IEnumerable)GetValue(TranslationsProperty);
            set => SetValue(TranslationsProperty, value);
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
        /// Gets or sets the vertical spacing between the token and its translation.
        /// </summary>
        public double TranslationVerticalSpacing
        {
            get => (double)GetValue(TranslationVerticalSpacingProperty);
            set => SetValue(TranslationVerticalSpacingProperty, value);
        }

        public IEnumerable Detokenize(IEnumerable row)
        {
            var tokensTextRow = row as TokensTextRow;
            if (tokensTextRow != null)
            {
                var detokenizer = new EngineStringDetokenizer(new LatinWordDetokenizer());
                var tokensWithPadding = detokenizer.Detokenize(tokensTextRow.Tokens);
                return tokensWithPadding;
            }

            return null;
        }


    }
}
