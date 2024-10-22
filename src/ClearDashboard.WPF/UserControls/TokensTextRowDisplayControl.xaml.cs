﻿using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using SIL.Machine.Tokenization;

namespace ClearDashboard.Wpf.UserControls
{
    /// <summary>
    /// A control for displaying a <see cref="TokensTextRow"/>
    /// </summary>
    public partial class TokensTextRowDisplayControl : UserControl
    {
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(TokensTextRowDisplayControl));

        /// <summary>
        /// Identifies the Wrap dependency property.
        /// </summary>
        public static readonly DependencyProperty WrapProperty = DependencyProperty.Register("Wrap", typeof(bool), typeof(TokensTextRowDisplayControl));
        
        public static readonly DependencyProperty TokensTextRowProperty = DependencyProperty.Register("TokensTextRow", typeof(TokensTextRow), typeof(TokensTextRowDisplayControl));

        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(TokensTextRowDisplayControl));
        public static readonly DependencyProperty TokensProperty = DependencyProperty.Register("Tokens", typeof(IEnumerable), typeof(TokensTextRowDisplayControl));

        #endregion Static DependencyProperties

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

        ///// <summary>
        ///// Occurs when an individual token is clicked.
        ///// </summary>
        //public event RoutedEventHandler TokenClicked
        //{
        //    add => AddHandler(TokenClickedEvent, value);
        //    remove => RemoveHandler(TokenClickedEvent, value);
        //}

        ///// <summary>
        ///// Occurs when an individual token is clicked two or more times.
        ///// </summary>
        //public event RoutedEventHandler TokenDoubleClicked
        //{
        //    add => AddHandler(TokenDoubleClickedEvent, value);
        //    remove => RemoveHandler(TokenDoubleClickedEvent, value);
        //}

        ///// <summary>
        ///// Occurs when the left mouse button is pressed while the mouse pointer is over a token.
        ///// </summary>
        //public event RoutedEventHandler TokenLeftButtonDown
        //{
        //    add => AddHandler(TokenLeftButtonDownEvent, value);
        //    remove => RemoveHandler(TokenLeftButtonDownEvent, value);
        //}

        ///// <summary>
        ///// Occurs when the left mouse button is released while the mouse pointer is over a token.
        ///// </summary>
        //public event RoutedEventHandler TokenLeftButtonUp
        //{
        //    add => AddHandler(TokenLeftButtonUpEvent, value);
        //    remove => RemoveHandler(TokenLeftButtonUpEvent, value);
        //}

        ///// <summary>
        ///// Occurs when the right mouse button is pressed while the mouse pointer is over a token.
        ///// </summary>
        //public event RoutedEventHandler TokenRightButtonDown
        //{
        //    add => AddHandler(TokenRightButtonDownEvent, value);
        //    remove => RemoveHandler(TokenRightButtonDownEvent, value);
        //}

        ///// <summary>
        ///// Occurs when the right mouse button is released while the mouse pointer is over a token.
        ///// </summary>
        //public event RoutedEventHandler TokenRightButtonUp
        //{
        //    add => AddHandler(TokenRightButtonUpEvent, value);
        //    remove => RemoveHandler(TokenRightButtonUpEvent, value);
        //}

        ///// <summary>
        ///// Occurs when the mouse pointer enters the bounds of a token.
        ///// </summary>
        //public event RoutedEventHandler TokenMouseEnter
        //{
        //    add => AddHandler(TokenMouseEnterEvent, value);
        //    remove => RemoveHandler(TokenMouseEnterEvent, value);
        //}

        ///// <summary>
        ///// Occurs when the mouse pointer leaves the bounds of a token.
        ///// </summary>
        //public event RoutedEventHandler TokenMouseLeave
        //{
        //    add => AddHandler(TokenMouseLeaveEvent, value);
        //    remove => RemoveHandler(TokenMouseLeaveEvent, value);
        //}

        ///// <summary>
        ///// Occurs when the user rotates the mouse wheel while the mouse pointer is over a token.
        ///// </summary>
        //public event RoutedEventHandler TokenMouseWheel
        //{
        //    add => AddHandler(TokenMouseWheelEvent, value);
        //    remove => RemoveHandler(TokenMouseWheelEvent, value);
        //}

        public TokensTextRowDisplayControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets a collection to display in the control.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set
            {
                SetValue(ItemsSourceProperty, value);
                Tokens = Detokenize(value);
            }
        }

        /// <summary>
        /// Gets or sets a collection to display in the control.
        /// </summary>
        public IEnumerable Tokens
        {
            get => (IEnumerable)GetValue(TokensProperty);
            set => SetValue(TokensProperty, value);
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
//            RaiseTokenEvent(TokenClickedEvent, e);
        }        
        
        private void OnTokenDoubleClicked(object sender, RoutedEventArgs e)
        {
//            RaiseTokenEvent(TokenDoubleClickedEvent, e);
        }        
        
        private void OnTokenLeftButtonDown(object sender, RoutedEventArgs e)
        {
//            RaiseTokenEvent(TokenLeftButtonDownEvent, e);
        }        
        
        private void OnTokenLeftButtonUp(object sender, RoutedEventArgs e)
        {
//            RaiseTokenEvent(TokenRightButtonUpEvent, e);
        }        
        private void OnTokenRightButtonDown(object sender, RoutedEventArgs e)
        {
//            RaiseTokenEvent(TokenRightButtonDownEvent, e);
        }        
        
        private void OnTokenRightButtonUp(object sender, RoutedEventArgs e)
        {
//            RaiseTokenEvent(TokenRightButtonUpEvent, e);
        }        
        
        private void OnTokenMouseEnter(object sender, RoutedEventArgs e)
        {
//            RaiseTokenEvent(TokenMouseEnterEvent, e);
        }

        private void OnTokenMouseLeave(object sender, RoutedEventArgs e)
        {
//            RaiseTokenEvent(TokenMouseLeaveEvent, e);
        }

        private void OnTokenMouseWheel(object sender, RoutedEventArgs e)
        {
//            RaiseTokenEvent(TokenMouseWheelEvent, e);
        }
    }
}
