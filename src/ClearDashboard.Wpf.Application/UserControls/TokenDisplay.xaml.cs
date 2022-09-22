using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control for displaying a single <see cref="Token"/> alongside a possible <see cref="Translation"/>
    /// and possible note indicator.
    /// </summary>
    public partial class TokenDisplay
    {
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(TokenDisplay),
            new PropertyMetadata(Orientation.Horizontal, OnLayoutChanged));
        
        /// <summary>
        /// Identifies the TokenMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenMarginProperty = DependencyProperty.Register("TokenMargin", typeof(Thickness), typeof(TokenDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the HorizontalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register("HorizontalSpacing", typeof(double), typeof(TokenDisplay),
            new PropertyMetadata(5d, OnLayoutChanged));

        /// <summary>
        /// Identifies the TokenVerticalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenVerticalSpacingProperty = DependencyProperty.Register("TokenVerticalSpacing", typeof(double), typeof(TokenDisplay),
            new PropertyMetadata(4d, OnLayoutChanged));

        /// <summary>
        /// Identifies the NoteIndicatorMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorMarginProperty = DependencyProperty.Register("NoteIndicatorMargin", typeof(Thickness), typeof(TokenDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the NoteIndicatorHeight dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorHeightProperty = DependencyProperty.Register("NoteIndicatorHeight", typeof(double), typeof(TokenDisplay),
            new PropertyMetadata(3d, OnLayoutChanged));
        
        /// <summary>
        /// Identifies the NoteIndicatorColor dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorColorProperty = DependencyProperty.Register("NoteIndicatorColor", typeof(Brush), typeof(TokenDisplay),
            new PropertyMetadata(Brushes.LightGray));

        /// <summary>
        /// Identifies the TranslationMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationMarginProperty = DependencyProperty.Register("TranslationMargin", typeof(Thickness), typeof(TokenDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the TranslationVerticalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationVerticalSpacingProperty = DependencyProperty.Register("TranslationVerticalSpacing", typeof(double), typeof(TokenDisplay),
            new PropertyMetadata(10d, OnLayoutChanged));

        /// <summary>
        /// Identifies the TranslationAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationAlignmentProperty = DependencyProperty.Register("TranslationAlignment", typeof(HorizontalAlignment), typeof(TokenDisplay),
            new PropertyMetadata(HorizontalAlignment.Center, OnLayoutChanged));

        /// <summary>
        /// Identifies the TranslationFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontSizeProperty = DependencyProperty.Register("TranslationFontSize", typeof(double), typeof(TokenDisplay),
            new PropertyMetadata(16d));

        /// <summary>
        /// Identifies the ShowTranslation dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowTranslationProperty = DependencyProperty.Register("ShowTranslation", typeof(bool), typeof(TokenDisplay),
            new PropertyMetadata(true, OnLayoutChanged));

        /// <summary>
        /// Identifies the TranslationVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationVisibilityProperty = DependencyProperty.Register("TranslationVisibility", typeof(Visibility), typeof(TokenDisplay),
            new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Identifies the ShowNoteIndicator dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowNoteIndicatorProperty = DependencyProperty.Register("ShowNoteIndicator", typeof(bool), typeof(TokenDisplay),
            new PropertyMetadata(true, OnLayoutChanged));

        /// <summary>
        /// Identifies the NoteIndicatorVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorVisibilityProperty = DependencyProperty.Register("NoteIndicatorVisibility", typeof(Visibility), typeof(TokenDisplay),
            new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Identifies the SurfaceText dependency property.
        /// </summary>
        public static readonly DependencyProperty SurfaceTextProperty = DependencyProperty.Register("SurfaceText", typeof(string), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TargetTranslationText dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetTranslationTextProperty = DependencyProperty.Register("TargetTranslationText", typeof(string), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TranslationColor dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationColorProperty = DependencyProperty.Register("TranslationColor", typeof(Brush), typeof(TokenDisplay));

        #endregion Static DependencyProperties
        #region Static RoutedEvents
        /// <summary>
        /// Identifies the TokenClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenClickedEvent = EventManager.RegisterRoutedEvent
            ("TokenClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TokenDoubleClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenDoubleClickedEvent = EventManager.RegisterRoutedEvent
            ("TokenDoubleClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TokenLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TokenLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TokenLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TokenLeftButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TokenRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenRightButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TokenRightButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TokenRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenRightButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TokenRightButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TokenMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseEnterEvent = EventManager.RegisterRoutedEvent
            ("TokenMouseEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TokenMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseLeaveEvent = EventManager.RegisterRoutedEvent
            ("TokenMouseLeave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TokenMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseWheelEvent = EventManager.RegisterRoutedEvent
            ("TokenMouseWheel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TokenClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationClickedEvent = EventManager.RegisterRoutedEvent
            ("TranslationClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TranslationDoubleClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationDoubleClickedEvent = EventManager.RegisterRoutedEvent
            ("TranslationDoubleClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TranslationLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TranslationLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TranslationLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TranslationLeftButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TranslationRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationRightButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TranslationRightButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TranslationRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationRightButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TranslationRightButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TranslationMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationMouseEnterEvent = EventManager.RegisterRoutedEvent
            ("TranslationMouseEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TranslationMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationMouseLeaveEvent = EventManager.RegisterRoutedEvent
            ("TranslationMouseLeave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TranslationMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationMouseWheelEvent = EventManager.RegisterRoutedEvent
            ("TranslationMouseWheel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            ("NoteLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            ("NoteLeftButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteRightButtonDownEvent = EventManager.RegisterRoutedEvent
            ("NoteRightButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteRightButtonUpEvent = EventManager.RegisterRoutedEvent
            ("NoteRightButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseEnterEvent = EventManager.RegisterRoutedEvent
            ("NoteMouseEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseLeaveEvent = EventManager.RegisterRoutedEvent
            ("NoteMouseLeave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseWheelEvent = EventManager.RegisterRoutedEvent
            ("NoteMouseWheel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteCreateEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteCreateEvent = EventManager.RegisterRoutedEvent
            ("NoteCreate", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        #endregion Static RoutedEvents
        #region Public Events
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
        #region Private Event Handlers
        /// <summary>
        /// Callback handler for changes to the dependency properties that affect the layout.
        /// </summary>
        /// <param name="obj">The object whose layout has changed.</param>
        /// <param name="args">Event args containing the new value.</param>
        private static void OnLayoutChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (TokenDisplay)obj;
            control.CalculateLayout();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CalculateLayout();
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
            var control = e.Source as FrameworkElement;
            var tokenDisplay = control?.DataContext as TokenDisplayViewModel;
            RaiseEvent(new TranslationEventArgs
            {
                RoutedEvent = routedEvent,
                TokenDisplayViewModel = tokenDisplay,
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
            var tokenDisplay = control?.DataContext as TokenDisplayViewModel;
            RaiseEvent(new NoteEventArgs
            {
                RoutedEvent = routedEvent,
                //TokenDisplayViewModel = tokenDisplayViewModel
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

        private void OnCreateNote(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteCreateEvent, e);

        }

        #endregion

        /// <summary>
        /// Gets or sets the orientation for displaying the token.
        /// </summary>
        /// <remarks>
        /// Regardless of the value of this property, the token and its translation are still displayed in a vertical orientation.
        /// This is only relevant for determining whether leading whitespace should be trimmed prior to display.
        /// </remarks>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin around each token for display.
        /// </summary>
        /// <remarks>
        /// This property should normally not be set explicitly; it is computed from the token horizontal and vertical spacing.
        /// </remarks>
        public Thickness TokenMargin
        {
            get => (Thickness) GetValue(TokenMarginProperty);
            set => SetValue(TokenMarginProperty, value);
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
        /// Gets or sets the margin around the note indicator.
        /// </summary>
        /// <remarks>
        /// This property should normally not be set explicitly; it is computed from the token horizontal and vertical spacing.
        /// </remarks>
        public Thickness NoteIndicatorMargin
        {
            get => (Thickness) GetValue(NoteIndicatorMarginProperty);
            set => SetValue(NoteIndicatorMarginProperty, value);
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
            get => (double) GetValue(TranslationFontSizeProperty);
            set => SetValue(TranslationFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="HorizontalAlignment"/> for the token and translation.
        /// </summary>
        public HorizontalAlignment TranslationAlignment
        {
            get => (HorizontalAlignment) GetValue(HorizontalAlignmentProperty);
            set => SetValue(HorizontalAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the horizontal spacing between translations.
        /// </summary>
        /// <remarks>
        /// This property should normally not be set explicitly; it is computed from the translation horizontal and vertical spacing.
        /// </remarks>
        public Thickness TranslationMargin
        {
            get => (Thickness) GetValue(TranslationMarginProperty);
            set => SetValue(TranslationMarginProperty, value);
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
        /// Gets or sets the whether to showTranslation the translation.
        /// </summary>
        public bool ShowTranslation
        {
            get => (bool) GetValue(ShowTranslationProperty);
            set => SetValue(ShowTranslationProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Visibility"/> of the translation.
        /// </summary>
        /// <remarks>This should normally not be called; it is computed based on the <see cref="ShowTranslation"/> value.</remarks>
        public Visibility TranslationVisibility
        {
            get => (Visibility) GetValue(TranslationVisibilityProperty);
            set => SetValue(TranslationVisibilityProperty, value);
        }

        /// <summary>
        /// Gets or sets the whether to showTranslation the note indicator.
        /// </summary>
        public bool ShowNoteIndicator
        {
            get => (bool) GetValue(ShowNoteIndicatorProperty);
            set => SetValue(ShowNoteIndicatorProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Visibility"/> of the note indicator.
        /// </summary>
        /// <remarks>This should normally not be called directly; it is computed based on the <see cref="ShowNoteIndicator"/> value.</remarks>
        public Visibility NoteIndicatorVisibility
        {
            get => (Visibility) GetValue(NoteIndicatorVisibilityProperty);
            set => SetValue(NoteIndicatorVisibilityProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the note indicator.
        /// </summary>
        public Brush NoteIndicatorColor
        {
            get => (Brush) GetValue(NoteIndicatorColorProperty);
            set => SetValue(NoteIndicatorColorProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="TokenDisplayViewModel"/> data source for this control.
        /// </summary>
        public TokenDisplayViewModel TokenDisplayViewModel => (TokenDisplayViewModel) DataContext;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to use for displaying the translation, based on its <see cref="DataAccessLayer.Models.TranslationState"./>
        /// </summary>
        /// <remarks>
        /// This should normally not be set directly; the value is based on the TranslationState:
        ///   * Words in red come from a translation model generated from SMT, e.g. IBM4.
        ///   * Words in blue were set by the same word being set elsewhere, with "Change all unset occurrences" selected.
        ///   * Words in black were set by the user.
        /// </remarks>
        public Brush TranslationColor
        {
            get => (Brush)GetValue(TranslationColorProperty);
            set => SetValue(TranslationColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the surface text to be displayed.
        /// </summary>
        /// <remarks>
        /// This should normally not be called directly; it is computed based on the orientation of the display.
        /// </remarks>
        public string SurfaceText
        {
            get => (string) GetValue(SurfaceTextProperty);
            set => SetValue(SurfaceTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the translation target text to be displayed.
        /// </summary>
        /// <remarks>
        /// This should normally not be called directly; it is computed based on the display properties.
        /// </remarks>
        public string TargetTranslationText
        {
            get => (string) GetValue(TargetTranslationTextProperty);
            set => SetValue(TargetTranslationTextProperty, value);
        }

        private void CalculateLayout()
        {
            var leftMargin = Orientation == Orientation.Horizontal ? TokenDisplayViewModel.PaddingBefore.Length * HorizontalSpacing : 0;
            var rightMargin = Orientation == Orientation.Horizontal ? TokenDisplayViewModel.PaddingAfter.Length * HorizontalSpacing : 0;

            TokenMargin = new Thickness(leftMargin, 0, rightMargin, 0);
            NoteIndicatorMargin = new Thickness(leftMargin, 0, 0, TokenVerticalSpacing);
            TranslationMargin = new Thickness(leftMargin, 0, rightMargin, TranslationVerticalSpacing);
            TranslationVisibility = (ShowTranslation && TokenDisplayViewModel.Translation != null) ? Visibility.Visible : Visibility.Collapsed;
            NoteIndicatorVisibility = (ShowNoteIndicator && TokenDisplayViewModel.HasNote) ? Visibility.Visible : Visibility.Hidden;

            SurfaceText = Orientation == Orientation.Horizontal ? TokenDisplayViewModel.SurfaceText : TokenDisplayViewModel.SurfaceText.Trim();
            TargetTranslationText = TokenDisplayViewModel.TargetTranslationText;
            TranslationColor = TokenDisplayViewModel.TranslationState switch
            {
                "FromTranslationModel" => Brushes.Red,
                "FromOther" => Brushes.Blue,
                _ => Brushes.Black
            };
        }

        public TokenDisplay()
        {
            InitializeComponent();

            HorizontalContentAlignment = HorizontalAlignment.Center;
            var horizontalAlignmentProperty = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(Control.HorizontalContentAlignmentProperty, typeof(Control));
            horizontalAlignmentProperty.AddValueChanged(this, OnHorizontalAlignmentChanged);
            
            Loaded += OnLoaded;
        }

        private void OnHorizontalAlignmentChanged(object? sender, EventArgs args)
        {
            CalculateLayout();
        }

    }
}
