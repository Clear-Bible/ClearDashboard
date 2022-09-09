using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.ViewModels.Display;

//using Token = ClearDashboard.DataAccessLayer.Models.Token;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control for displaying a single <see cref="Translation"/>.
    /// </summary>
    public partial class TranslationDisplayControl : UserControl
    {
        #region Static RoutedEvents
        /// <summary>
        /// Identifies the TokenClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenClickedEvent = EventManager.RegisterRoutedEvent
            ("TokenClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplayControl));

        /// <summary>
        /// Identifies the TokenDoubleClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenDoubleClickedEvent = EventManager.RegisterRoutedEvent
            ("TokenDoubleClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplayControl));

        /// <summary>
        /// Identifies the TokenLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TokenLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplayControl));

        /// <summary>
        /// Identifies the TokenLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TokenLeftButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplayControl));

        /// <summary>
        /// Identifies the TokenRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenRightButtonDownEvent = EventManager.RegisterRoutedEvent
            ("TokenRightButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplayControl));

        /// <summary>
        /// Identifies the TokenRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenRightButtonUpEvent = EventManager.RegisterRoutedEvent
            ("TokenRightButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplayControl));

        /// <summary>
        /// Identifies the TokenMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseEnterEvent = EventManager.RegisterRoutedEvent
            ("TokenMouseEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplayControl));

        /// <summary>
        /// Identifies the TokenMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseLeaveEvent = EventManager.RegisterRoutedEvent
            ("TokenMouseLeave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplayControl));

        /// <summary>
        /// Identifies the TokenMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseWheelEvent = EventManager.RegisterRoutedEvent
            ("TokenMouseWheel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplayControl));

        #endregion Static RoutedEvents
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the InnerMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerMarginProperty = DependencyProperty.Register("InnerMargin", typeof(Thickness), typeof(TranslationDisplayControl),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        public static readonly DependencyProperty TranslationInnerPaddingProperty = DependencyProperty.Register("TranslationInnerPadding", typeof(Thickness), typeof(TranslationDisplayControl),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        public static readonly DependencyProperty TranslationFontSizeProperty = DependencyProperty.Register("TranslationFontSize", typeof(double), typeof(TranslationDisplayControl),
            new PropertyMetadata(16d));
        public static readonly DependencyProperty TranslationVerticalSpacingProperty = DependencyProperty.Register("TranslationVerticalSpacing", typeof(double), typeof(TranslationDisplayControl),
            new PropertyMetadata(10d, new PropertyChangedCallback(OnTransactionVerticalSpacingChanged)));

        private static void OnTransactionVerticalSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TranslationDisplayControl)d;
            control.CalculateTranslationInnerPadding((double) e.NewValue);
        }

        #endregion Static DependencyProperties

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

        private TokenDisplay TokenDisplay => (TokenDisplay) DataContext;

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

        public string SurfaceText
        {
            get
            {
                return TokenDisplay.SurfaceText;
            }
        }

        public string TargetTranslationText
        {
            get
            {
                return TokenDisplay.TargetTranslationText;
            }
        }

        public Visibility TranslationVisibility
        {
            get
            {
                return TokenDisplay.Translation != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility NoteVisibility
        {
            get
            {
                return !String.IsNullOrEmpty(TokenDisplay?.Note) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Thickness InnerPadding
        {
            get
            {
                return new Thickness(TokenDisplay.PaddingBefore.Length * 10, 
                    0,
                    TokenDisplay.PaddingAfter.Length * 10, 
                    0);
            }
        }        
        
        //public Thickness TranslationInnerPadding
        //{
        //    get
        //    {
        //        return new Thickness(TokenDisplay.PaddingBefore.Length * 10, 
        //            TranslationVerticalSpacing,
        //            0,
        //            TranslationVerticalSpacing);
        //    }
        //}

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

        private void RaiseTokenEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            RaiseEvent(new TokenEventArgs
            {
                RoutedEvent = routedEvent,
                SurfaceText = (e.Source as FrameworkElement)?.DataContext as string
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
    }
}
