using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control for displaying a single <see cref="Token"/> alongside a <see cref="Translation"/>.
    /// </summary>
    public partial class TranslationDisplayControl : UserControl
    {
        #region Static RoutedEvents
        /// <summary>
        /// Identifies the TokenClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenClickedEvent = EventManager.RegisterRoutedEvent
            ("TokenClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TokenDoubleClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenDoubleClickedEvent = EventManager.RegisterRoutedEvent
            ("TokenDoubleClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TokenLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TokenLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TokenLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TokenLeftButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TokenRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenRightButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TokenRightButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TokenRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenRightButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TokenRightButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TokenMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseEnterEvent = EventManager.RegisterRoutedEvent
            ("TokenMouseEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TokenMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseLeaveEvent = EventManager.RegisterRoutedEvent
            ("TokenMouseLeave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TokenMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseWheelEvent = EventManager.RegisterRoutedEvent
            ("TokenMouseWheel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TokenClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationClickedEvent = EventManager.RegisterRoutedEvent
            ("TranslationClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TranslationDoubleClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationDoubleClickedEvent = EventManager.RegisterRoutedEvent
            ("TranslationDoubleClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TranslationLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TranslationLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TranslationLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TranslationLeftButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TranslationRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationRightButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TranslationRightButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TranslationRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationRightButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TranslationRightButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TranslationMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationMouseEnterEvent = EventManager.RegisterRoutedEvent
            ("TranslationMouseEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TranslationMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationMouseLeaveEvent = EventManager.RegisterRoutedEvent
            ("TranslationMouseLeave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the TranslationMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationMouseWheelEvent = EventManager.RegisterRoutedEvent
            ("TranslationMouseWheel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the NoteLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            ("NoteLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the NoteLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            ("NoteLeftButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the NoteRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteRightButtonDownEvent = EventManager.RegisterRoutedEvent
            ("NoteRightButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the NoteRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteRightButtonUpEvent = EventManager.RegisterRoutedEvent
            ("NoteRightButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the NoteMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseEnterEvent = EventManager.RegisterRoutedEvent
            ("NoteMouseEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the NoteMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseLeaveEvent = EventManager.RegisterRoutedEvent
            ("NoteMouseLeave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        /// <summary>
        /// Identifies the NoteMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseWheelEvent = EventManager.RegisterRoutedEvent
            ("NoteMouseWheel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationDisplayControl));

        #endregion Static RoutedEvents
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the InnerMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerMarginProperty = DependencyProperty.Register("InnerMargin", typeof(Thickness), typeof(TranslationDisplayControl),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the TranslationInnerPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationInnerPaddingProperty = DependencyProperty.Register("TranslationInnerPadding", typeof(Thickness), typeof(TranslationDisplayControl),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the TranslationFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontSizeProperty = DependencyProperty.Register("TranslationFontSize", typeof(double), typeof(TranslationDisplayControl),
            new PropertyMetadata(16d));

        /// <summary>
        /// Identifies the TranslationVerticalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationVerticalSpacingProperty = DependencyProperty.Register("TranslationVerticalSpacing", typeof(double), typeof(TranslationDisplayControl),
            new PropertyMetadata(10d, new PropertyChangedCallback(OnTransactionVerticalSpacingChanged)));

        /// <summary>
        /// Callback handler for changes to the TranslationVerticalSpacing dependency property: when this property changes, recalculate the inner padding for the translation.
        /// </summary>
        /// <param name="obj">The object whose TranslationVerticalSpacing has changed.</param>
        /// <param name="args">Event args containing the new value.</param>
        private static void OnTransactionVerticalSpacingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (TranslationDisplayControl)obj;
            control.CalculateTranslationInnerPadding((double) args.NewValue);
        }

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

        #endregion
        #region Event Handlers
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

        private void RaiseTranslationEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            var control = e.Source as FrameworkElement;
            var tokenDisplay = control?.DataContext as TokenDisplay;
            RaiseEvent(new TranslationEventArgs
            {
                RoutedEvent = routedEvent,
                Translation = tokenDisplay?.Translation
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
            var control = e.Source as FrameworkElement;
            var tokenDisplay = control?.DataContext as TokenDisplay;
            RaiseEvent(new NoteEventArgs
            {
                RoutedEvent = routedEvent,
                TokenDisplay = tokenDisplay
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
        #endregion
        
        /// <summary>
        /// Gets or sets the margin around each word for text display.
        /// </summary>
        public Thickness InnerMargin
        {
            get
            {
                var result = GetValue(InnerMarginProperty);
                return (Thickness) result;
            }
            set => SetValue(InnerMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for the translation.
        /// </summary>
        public double TranslationFontSize
        {
            get => (double) GetValue(TranslationFontSizeProperty);
            set => SetValue(TranslationFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the vertical spacing between the token and its translation.
        /// </summary>
        public double TranslationVerticalSpacing
        {
            get => (double) GetValue(TranslationVerticalSpacingProperty);
            set => SetValue(TranslationVerticalSpacingProperty, value);
        }

        /// <summary>
        /// Gets or sets the vertical spacing between the token and its translation.
        /// </summary>
        public Thickness TranslationInnerPadding
        {
            get => (Thickness) GetValue(TranslationInnerPaddingProperty);
            set => SetValue(TranslationInnerPaddingProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="TokenDisplay"/> data source for this control.
        /// </summary>
        public TokenDisplay TokenDisplay => (TokenDisplay) DataContext;

        /// <summary>
        /// Gets the <see cref="Brush"/> to use for displaying the translation, based on its <see cref="DataAccessLayer.Models.TranslationState"./>
        /// </summary>
        /// <remarks>
        /// Words in red come from a translation model generated from SMT, e.g. IBM4.
        /// Words in blue were set by the same word being set elsewhere, with "Change all unset occurrences" selected.
        /// Words in black were set by the user.
        /// </remarks>
        public Brush TranslationColor
        {
            get
            {
                return TokenDisplay.TranslationState switch
                {
                    "FromTranslationModel" => Brushes.Red,
                    "FromOther" => Brushes.Blue,
                    _ => Brushes.Black
                };
            }
        }

        /// <summary>
        /// Gets the surface text to be displayed.
        /// </summary>
        public string SurfaceText => TokenDisplay.SurfaceText;

        /// <summary>
        /// Gets the target translation text.
        /// </summary>
        public string TargetTranslationText => TokenDisplay.TargetTranslationText;

        /// <summary>
        /// Gets the visibility of the translation button.
        /// </summary>
        public Visibility TranslationVisibility => TokenDisplay.Translation != null ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Gets the visibility of the note indicator.
        /// </summary>
        public Visibility NoteVisibility => !String.IsNullOrEmpty(TokenDisplay?.Note) ? Visibility.Visible : Visibility.Hidden;

        /// <summary>
        /// Gets the inner padding of the source text button.
        /// </summary>
        public Thickness InnerPadding => new Thickness(TokenDisplay.PaddingBefore.Length * 10, 0, TokenDisplay.PaddingAfter.Length * 10, 0);

        private void CalculateTranslationInnerPadding(double spacing)
        {
            TranslationInnerPadding = new Thickness(TokenDisplay.PaddingBefore.Length * 10,
                spacing,
                0,
                spacing * 2);
        }


        public TranslationDisplayControl()
        {
            InitializeComponent();

            this.Loaded += TranslationDisplayControl_Loaded;
        }

        private void TranslationDisplayControl_Loaded(object sender, RoutedEventArgs e)
        {
            CalculateTranslationInnerPadding(TranslationVerticalSpacing);
        }

    }
}
