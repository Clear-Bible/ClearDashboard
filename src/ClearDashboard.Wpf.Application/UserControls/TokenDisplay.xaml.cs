﻿using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control for displaying a single <see cref="Token"/> alongside a possible <see cref="Translation"/>
    /// and possible note indicator.
    /// </summary>
    public partial class TokenDisplay : IHandle<SelectionUpdatedMessage>
    {
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the CompositeIndicatorComputedColor dependency property.
        /// </summary>
        public static readonly DependencyProperty CompositeIndicatorComputedColorProperty = DependencyProperty.Register(
            nameof(CompositeIndicatorComputedColor), typeof(Brush), typeof(TokenDisplay),
            new PropertyMetadata(Brushes.LightGray));

        /// <summary>
        /// Identifies the CompositeIndicatorHeight dependency property.
        /// </summary>
        public static readonly DependencyProperty CompositeIndicatorHeightProperty = DependencyProperty.Register(
            nameof(CompositeIndicatorHeight), typeof(double), typeof(TokenDisplay),
            new PropertyMetadata(3d, OnLayoutChanged));

        /// <summary>
        /// Identifies the CompositeIndicatorMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty CompositeIndicatorMarginProperty = DependencyProperty.Register(
            nameof(CompositeIndicatorMargin), typeof(Thickness), typeof(TokenDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the CompositeIndicatorVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty CompositeIndicatorVisibilityProperty = DependencyProperty.Register(
            nameof(CompositeIndicatorVisibility), typeof(Visibility), typeof(TokenDisplay),
            new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Identifies the ExtendedProperties dependency property.
        /// </summary>
        public static readonly DependencyProperty ExtendedPropertiesProperty =
            DependencyProperty.Register(nameof(ExtendedProperties), typeof(string), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the HighlightedTokenBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty HighlightedTokenBackgroundProperty = DependencyProperty.Register(
            nameof(HighlightedTokenBackground), typeof(Brush), typeof(TokenDisplay),
            new PropertyMetadata(Brushes.Aquamarine));

        /// <summary>
        /// Identifies the HorizontalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register(
            nameof(HorizontalSpacing), typeof(double), typeof(TokenDisplay),
            new PropertyMetadata(5d, OnLayoutChanged));

        /// <summary>
        /// Identifies the NoteIndicatorColor dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorColorProperty = DependencyProperty.Register(
            nameof(NoteIndicatorColor), typeof(Brush), typeof(TokenDisplay),
            new PropertyMetadata(Brushes.LightGray));

        /// <summary>
        /// Identifies the NoteIndicatorComputedColor dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorComputedColorProperty = DependencyProperty.Register(
            nameof(NoteIndicatorComputedColor), typeof(Brush), typeof(TokenDisplay),
            new PropertyMetadata(Brushes.LightGray));

        /// <summary>
        /// Identifies the NoteIndicatorHeight dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorHeightProperty = DependencyProperty.Register(
            nameof(NoteIndicatorHeight), typeof(double), typeof(TokenDisplay),
            new PropertyMetadata(5d, OnLayoutChanged));

        /// <summary>
        /// Identifies the NoteIndicatorMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorMarginProperty = DependencyProperty.Register(
            nameof(NoteIndicatorMargin), typeof(Thickness), typeof(TokenDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the NoteIndicatorVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorVisibilityProperty = DependencyProperty.Register(
            nameof(NoteIndicatorVisibility), typeof(Visibility), typeof(TokenDisplay),
            new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation),
            typeof(Orientation), typeof(TokenDisplay),
            new PropertyMetadata(Orientation.Horizontal, OnLayoutChanged));

        /// <summary>
        /// Identifies the SelectedTokenBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedTokenBackgroundProperty = DependencyProperty.Register(
            nameof(SelectedTokenBackground), typeof(Brush), typeof(TokenDisplay),
            new PropertyMetadata(Brushes.LightSteelBlue));

        /// <summary>
        /// Identifies the ShowNoteIndicator dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowNoteIndicatorProperty = DependencyProperty.Register(
            nameof(ShowNoteIndicator), typeof(bool), typeof(TokenDisplay),
            new PropertyMetadata(true, OnLayoutChanged));

        /// <summary>
        /// Identifies the ShowTranslation dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowTranslationProperty = DependencyProperty.Register(
            nameof(ShowTranslation), typeof(bool), typeof(TokenDisplay),
            new PropertyMetadata(true, OnLayoutChanged));

        /// <summary>
        /// Identifies the SurfaceText dependency property.
        /// </summary>
        public static readonly DependencyProperty SurfaceTextProperty =
            DependencyProperty.Register(nameof(SurfaceText), typeof(string), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TokenBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenBackgroundProperty = DependencyProperty.Register(
            nameof(TokenBackground), typeof(Brush), typeof(TokenDisplay),
            new PropertyMetadata(Brushes.Transparent));

        /// <summary>
        /// Identifies the TokenFlowDirection dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenFlowDirectionProperty = DependencyProperty.Register(
            nameof(TokenFlowDirection), typeof(FlowDirection), typeof(TokenDisplay),
            new PropertyMetadata(FlowDirection.LeftToRight));

        /// <summary>
        /// Identifies the TokenFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenFontFamilyProperty = DependencyProperty.Register(
            nameof(TokenFontFamily), typeof(FontFamily), typeof(TokenDisplay),
            new PropertyMetadata(new FontFamily(
                new Uri(
                    "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"),
                ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the TokenFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenFontSizeProperty = DependencyProperty.Register(
            nameof(TokenFontSize), typeof(double), typeof(TokenDisplay),
            new PropertyMetadata(18d));

        /// <summary>
        /// Identifies the TokenFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenFontStyleProperty = DependencyProperty.Register(
            nameof(TokenFontStyle), typeof(FontStyle), typeof(TokenDisplay),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the TokenFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenFontWeightProperty = DependencyProperty.Register(
            nameof(TokenFontWeight), typeof(FontWeight), typeof(TokenDisplay),
            new PropertyMetadata(FontWeights.SemiBold));

        /// <summary>
        /// Identifies the TokenMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenMarginProperty = DependencyProperty.Register(nameof(TokenMargin),
            typeof(Thickness), typeof(TokenDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the TokenVerticalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenVerticalSpacingProperty = DependencyProperty.Register(
            nameof(TokenVerticalSpacing), typeof(double), typeof(TokenDisplay),
            new PropertyMetadata(4d, OnLayoutChanged));

        /// <summary>
        /// Identifies the TranslationAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationAlignmentProperty = DependencyProperty.Register(
            nameof(TranslationAlignment), typeof(HorizontalAlignment), typeof(TokenDisplay),
            new PropertyMetadata(HorizontalAlignment.Center, OnLayoutChanged));

        /// <summary>
        /// Identifies the TranslationBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationBackgroundProperty = DependencyProperty.Register(
            nameof(TranslationBackground), typeof(Brush), typeof(TokenDisplay),
            new PropertyMetadata(Brushes.Transparent));

        /// <summary>
        /// Identifies the TranslationColor dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationColorProperty =
            DependencyProperty.Register(nameof(TranslationColor), typeof(Brush), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TranslationFlowDirection dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFlowDirectionProperty = DependencyProperty.Register(
            nameof(TranslationFlowDirection), typeof(FlowDirection), typeof(TokenDisplay),
            new PropertyMetadata(FlowDirection.LeftToRight));

        /// <summary>
        /// Identifies the TranslationFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontFamilyProperty = DependencyProperty.Register(
            nameof(TranslationFontFamily), typeof(FontFamily), typeof(TokenDisplay),
            new PropertyMetadata(new FontFamily(
                new Uri(
                    "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"),
                ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the TranslationFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontSizeProperty = DependencyProperty.Register(
            nameof(TranslationFontSize), typeof(double), typeof(TokenDisplay),
            new PropertyMetadata(16d));

        /// <summary>
        /// Identifies the TranslationFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontStyleProperty = DependencyProperty.Register(
            nameof(TranslationFontStyle), typeof(FontStyle), typeof(TokenDisplay),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the TranslationFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontWeightProperty = DependencyProperty.Register(
            nameof(TranslationFontWeight), typeof(FontWeight), typeof(TokenDisplay),
            new PropertyMetadata(FontWeights.SemiBold));

        /// <summary>
        /// Identifies the TranslationMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationMarginProperty = DependencyProperty.Register(
            nameof(TranslationMargin), typeof(Thickness), typeof(TokenDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the TranslationText dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationTextProperty =
            DependencyProperty.Register(nameof(TranslationText), typeof(string), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TranslationVerticalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationVerticalSpacingProperty = DependencyProperty.Register(
            nameof(TranslationVerticalSpacing), typeof(double), typeof(TokenDisplay),
            new PropertyMetadata(10d, OnLayoutChanged));

        /// <summary>
        /// Identifies the TranslationVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationVisibilityProperty = DependencyProperty.Register(
            nameof(TranslationVisibility), typeof(Visibility), typeof(TokenDisplay),
            new PropertyMetadata(Visibility.Visible));

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
        /// Identifies the TokenJoinEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenJoinEvent = EventManager.RegisterRoutedEvent
            ("TokenJoin", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TokenJoinLanguagePairEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenJoinLanguagePairEvent = EventManager.RegisterRoutedEvent
            ("TokenJoinLanguagePair", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TokenUnjoinEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenUnjoinEvent = EventManager.RegisterRoutedEvent
            ("TokenUnjoin", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

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
        /// Identifies the NoteIndicatorLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteIndicatorLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteIndicatorLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorLeftButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteIndicatorRightButtonDownEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorRightButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteIndicatorRightButtonUpEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorRightButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteIndicatorMouseEnterEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorMouseEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteIndicatorMouseLeaveEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorMouseLeave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteIndicatorMouseWheelEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorMouseWheel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the NoteCreateEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteCreateEvent = EventManager.RegisterRoutedEvent
            ("NoteCreate", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the FilterPinsEvent routed event.
        /// </summary>
        public static readonly RoutedEvent FilterPinsEvent = EventManager.RegisterRoutedEvent
            ("FilterPins", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the CopyEvent routed event.
        /// </summary>
        public static readonly RoutedEvent CopyEvent = EventManager.RegisterRoutedEvent
            ("Copy", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

        /// <summary>
        /// Identifies the TranslateQuickEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslateQuickEvent = EventManager.RegisterRoutedEvent
            ("TranslateQuick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TokenDisplay));

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
        /// Occurs when the user requests to join multiple tokens into a composite token.
        /// </summary>
        public event RoutedEventHandler TokenJoin
        {
            add => AddHandler(TokenJoinEvent, value);
            remove => RemoveHandler(TokenJoinEvent, value);
        }

        /// <summary>
        /// Occurs when the user requests to join multiple tokens into a composite token for a language pair.
        /// </summary>
        public event RoutedEventHandler TokenJoinLanguagePair
        {
            add => AddHandler(TokenJoinLanguagePairEvent, value);
            remove => RemoveHandler(TokenJoinLanguagePairEvent, value);
        }

        /// <summary>
        /// Occurs when the user requests to unjoin a composite token.
        /// </summary>
        public event RoutedEventHandler TokenUnjoin
        {
            add => AddHandler(TokenUnjoinEvent, value);
            remove => RemoveHandler(TokenUnjoinEvent, value);
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
        public event RoutedEventHandler NoteIndicatorLeftButtonDown
        {
            add => AddHandler(NoteIndicatorLeftButtonDownEvent, value);
            remove => RemoveHandler(NoteIndicatorLeftButtonDownEvent, value);
        }

        /// <summary>
        /// Occurs when the left mouse button is released while the mouse pointer is over a note indicator.
        /// </summary>
        public event RoutedEventHandler NoteIndicatorLeftButtonUp
        {
            add => AddHandler(NoteIndicatorLeftButtonUpEvent, value);
            remove => RemoveHandler(NoteIndicatorLeftButtonUpEvent, value);
        }

        /// <summary>
        /// Occurs when the right mouse button is pressed while the mouse pointer is over a note indicator.
        /// </summary>
        public event RoutedEventHandler NoteIndicatorRightButtonDown
        {
            add => AddHandler(NoteIndicatorRightButtonDownEvent, value);
            remove => RemoveHandler(NoteIndicatorRightButtonDownEvent, value);
        }

        /// <summary>
        /// Occurs when the right mouse button is released while the mouse pointer is over a note indicator.
        /// </summary>
        public event RoutedEventHandler NoteIndicatorRightButtonUp
        {
            add => AddHandler(NoteIndicatorRightButtonUpEvent, value);
            remove => RemoveHandler(NoteIndicatorRightButtonUpEvent, value);
        }

        /// <summary>
        /// Occurs when the mouse pointer enters the bounds of a note indicator.
        /// </summary>
        public event RoutedEventHandler NoteIndicatorMouseEnter
        {
            add => AddHandler(NoteIndicatorMouseEnterEvent, value);
            remove => RemoveHandler(NoteIndicatorMouseEnterEvent, value);
        }

        /// <summary>
        /// Occurs when the mouse pointer leaves the bounds of a note indicator.
        /// </summary>
        public event RoutedEventHandler NoteIndicatorMouseLeave
        {
            add => AddHandler(NoteIndicatorMouseLeaveEvent, value);
            remove => RemoveHandler(NoteIndicatorMouseLeaveEvent, value);
        }

        /// <summary>
        /// Occurs when the user rotates the mouse wheel while the mouse pointer is over a note indicator.
        /// </summary>
        public event RoutedEventHandler NoteIndicatorMouseWheel
        {
            add => AddHandler(NoteIndicatorMouseWheelEvent, value);
            remove => RemoveHandler(NoteIndicatorMouseWheelEvent, value);
        }

        /// <summary>
        /// Occurs when the user requests to create a new note.
        /// </summary>
        public event RoutedEventHandler NoteCreate
        {
            add => AddHandler(NoteCreateEvent, value);
            remove => RemoveHandler(NoteCreateEvent, value);
        }

        /// <summary>
        /// Occurs when the user requests to filter pins.
        /// </summary>
        public event RoutedEventHandler FilterPins
        {
            add => AddHandler(FilterPinsEvent, value);
            remove => RemoveHandler(FilterPinsEvent, value);
        }

        /// <summary>
        /// Occurs when the user requests to copy.
        /// </summary>
        public event RoutedEventHandler Copy
        {
            add => AddHandler(CopyEvent, value);
            remove => RemoveHandler(CopyEvent, value);
        }

        /// <summary>
        /// Occurs when the user requests to translate quick.
        /// </summary>
        public event RoutedEventHandler TranslateQuick
        {
            add => AddHandler(TranslateQuickEvent, value);
            remove => RemoveHandler(TranslateQuickEvent, value);
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
            TokenDisplayViewModel.PropertyChanged += TokenDisplayViewModelPropertyChanged;
            CalculateLayout();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            TokenDisplayViewModel.PropertyChanged -= TokenDisplayViewModelPropertyChanged;
        }

        private void TokenDisplayViewModelPropertyChanged(object? sender,
            System.ComponentModel.PropertyChangedEventArgs e)
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
                TokenDisplay = tokenDisplay!,
                ModifierKeys = Keyboard.Modifiers
            });
        }

        private void OnTokenClicked(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenClickedEvent, e);
        }

        private void OnTokenContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            JoinTokensMenuItem.Visibility = AllSelectedTokens.CanJoinTokens ? Visibility.Visible : Visibility.Collapsed;
            JoinTokensLanguagePairMenuItem.Visibility = AllSelectedTokens.CanJoinTokens ? Visibility.Visible : Visibility.Collapsed;
            UnjoinTokenMenuItem.Visibility = AllSelectedTokens.CanUnjoinToken ? Visibility.Visible : Visibility.Collapsed;
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

        private void OnToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ExtendedProperties))
            {
                e.Handled = true;
            }
        }

        private void RaiseTranslationEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            var control = e.Source as FrameworkElement;
            var tokenDisplay = control?.DataContext as TokenDisplayViewModel;
            RaiseEvent(new TranslationEventArgs
            {
                RoutedEvent = routedEvent,
                TokenDisplay = tokenDisplay!,
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
                TokenDisplayViewModel = tokenDisplay!
            });
        }

        private void OnNoteLeftButtonDown(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteIndicatorLeftButtonDownEvent, e);
        }

        private void OnNoteLeftButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteIndicatorRightButtonUpEvent, e);
        }

        private void OnNoteRightButtonDown(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteIndicatorRightButtonDownEvent, e);
        }

        private void OnNoteRightButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteIndicatorRightButtonUpEvent, e);
        }

        private void OnNoteMouseEnter(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteIndicatorMouseEnterEvent, e);
        }

        private void OnNoteMouseLeave(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteIndicatorMouseLeaveEvent, e);
        }

        private void OnNoteMouseWheel(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteIndicatorMouseWheelEvent, e);
        }

        private void OnCreateNote(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(NoteCreateEvent, e);
        }

        private void OnTokenJoin(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenJoinEvent, e);
        }

        private void OnTokenJoinLanguagePair(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenJoinLanguagePairEvent, e);
        }

        private void OnTokenUnjoin(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenUnjoinEvent, e);
        }

        private void OnFilterPins(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(FilterPinsEvent, e);
        }

        private void OnCopy(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(CopyEvent, e);
        }

        private void OnTranslateQuick(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(TranslateQuickEvent, e); //Rename RaiseNoteEvent
        }

        public async Task HandleAsync(SelectionUpdatedMessage message, CancellationToken cancellationToken)
        {
            AllSelectedTokens = message.SelectedTokens;
            await Task.CompletedTask;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the <see cref="EventAggregator"/> to be used for participating in the Caliburn Micro eventing system.
        /// </summary>
        public static IEventAggregator? EventAggregator { get; set; }

        /// <summary>
        /// Gets or sets the collection of tokens selected across all displays.
        /// </summary>
        private TokenDisplayViewModelCollection AllSelectedTokens { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the composite token indicator.
        /// </summary>
        /// <remarks>
        /// This should not be set explicitly; it is computed based on whether the token is part of a composite token.
        /// </remarks>
        public Brush CompositeIndicatorComputedColor
        {
            get => (Brush)GetValue(CompositeIndicatorComputedColorProperty);
            private set => SetValue(CompositeIndicatorComputedColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the height of the composite indicator.
        /// </summary>
        public double CompositeIndicatorHeight
        {
            get => (double)GetValue(CompositeIndicatorHeightProperty);
            set => SetValue(CompositeIndicatorHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin around the composite indicator.
        /// </summary>
        /// <remarks>
        /// This property should not be set explicitly; it is computed from the token horizontal and vertical spacing.
        /// </remarks>
        public Thickness CompositeIndicatorMargin
        {
            get => (Thickness)GetValue(CompositeIndicatorMarginProperty);
            private set => SetValue(CompositeIndicatorMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Visibility"/> of the composite indicator.
        /// </summary>
        /// <remarks>
        /// This should  not be set explicitly; it is computed based on whether the token is part of a composite token.
        /// </remarks>
        public Visibility CompositeIndicatorVisibility
        {
            get => (Visibility)GetValue(CompositeIndicatorVisibilityProperty);
            private set => SetValue(CompositeIndicatorVisibilityProperty, value);
        }

        /// <summary>
        /// Gets or sets the extendedProperties to be displayed.
        /// </summary>
        public string? ExtendedProperties
        {
            get => (string)GetValue(ExtendedPropertiesProperty);
            set => SetValue(ExtendedPropertiesProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the token background when it is highlighted.
        /// </summary>
        public Brush HighlightedTokenBackground
        {
            get => (Brush)GetValue(HighlightedTokenBackgroundProperty);
            set => SetValue(HighlightedTokenBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the horizontal spacing between translations.
        /// </summary>
        /// <remarks>
        /// This is a relative factor that will ultimately depend on the token's PaddingBefore and PaddingAfter values.
        /// </remarks>
        public double HorizontalSpacing
        {
            get => (double)GetValue(HorizontalSpacingProperty);
            set => SetValue(HorizontalSpacingProperty, value);
        }

        /// <summary>
        /// Gets or sets the default <see cref="Brush"/> used to draw the note indicator.
        /// </summary>
        public Brush NoteIndicatorColor
        {
            get => (Brush)GetValue(NoteIndicatorColorProperty);
            set => SetValue(NoteIndicatorColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the note indicator, depending on the note's hover status.
        /// </summary>
        /// <remarks>
        /// This should not be set explicitly; it is computed from the note's hover status.
        /// </remarks>
        public Brush NoteIndicatorComputedColor
        {
            get => (Brush)GetValue(NoteIndicatorComputedColorProperty);
            private set => SetValue(NoteIndicatorComputedColorProperty, value);
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
        /// This property should not be set explicitly; it is computed from the token horizontal and vertical spacing.
        /// </remarks>
        public Thickness NoteIndicatorMargin
        {
            get => (Thickness)GetValue(NoteIndicatorMarginProperty);
            private set => SetValue(NoteIndicatorMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Visibility"/> of the note indicator.
        /// </summary>
        /// <remarks>This should not be set explicitly; it is computed based on the <see cref="ShowNoteIndicator"/> value.</remarks>
        public Visibility NoteIndicatorVisibility
        {
            get => (Visibility)GetValue(NoteIndicatorVisibilityProperty);
            private set => SetValue(NoteIndicatorVisibilityProperty, value);
        }

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
        /// Gets or sets the <see cref="Brush"/> used to draw the token background when it is selected.
        /// </summary>
        public Brush SelectedTokenBackground
        {
            get => (Brush)GetValue(SelectedTokenBackgroundProperty);
            set => SetValue(SelectedTokenBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the whether to show the note indicator.
        /// </summary>
        public bool ShowNoteIndicator
        {
            get => (bool)GetValue(ShowNoteIndicatorProperty);
            set => SetValue(ShowNoteIndicatorProperty, value);
        }

        /// <summary>
        /// Gets or sets the whether to showTranslation the translation.
        /// </summary>
        public bool ShowTranslation
        {
            get => (bool)GetValue(ShowTranslationProperty);
            set => SetValue(ShowTranslationProperty, value);
        }

        /// <summary>
        /// Gets or sets the surface text to be displayed.
        /// </summary>
        /// <remarks>
        /// This should not be set directly; it is computed based on the orientation of the display.
        /// </remarks>
        public string SurfaceText
        {
            get => (string)GetValue(SurfaceTextProperty);
            private set => SetValue(SurfaceTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the token background.
        /// </summary>
        /// <remarks>
        /// This property should not be set explicitly; it is computed from the token's selection status.
        /// </remarks>
        public Brush TokenBackground
        {
            get => (Brush)GetValue(TokenBackgroundProperty);
            private set => SetValue(TokenBackgroundProperty, value);
        }

        /// <summary>
        /// Gets the strongly-typed <see cref="TokenDisplayViewModel"/> data source for this control.
        /// </summary>
        public TokenDisplayViewModel TokenDisplayViewModel => (TokenDisplayViewModel)DataContext;

        /// <summary>
        /// Gets or sets the <see cref="FlowDirection"/> to use for displaying the tokens.
        /// </summary>
        public FlowDirection TokenFlowDirection
        {
            get => (FlowDirection)GetValue(TokenFlowDirectionProperty);
            set => SetValue(TokenFlowDirectionProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="FontFamily"/> to use for displaying the token.
        /// </summary>
        public FontFamily TokenFontFamily
        {
            get => (FontFamily)GetValue(TokenFontFamilyProperty);
            set => SetValue(TokenFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for the token.
        /// </summary>
        public double TokenFontSize
        {
            get => (double)GetValue(TokenFontSizeProperty);
            set => SetValue(TokenFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for the token.
        /// </summary>
        public FontStyle TokenFontStyle
        {
            get => (FontStyle)GetValue(TokenFontStyleProperty);
            set => SetValue(TokenFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for the token.
        /// </summary>
        public FontWeight TokenFontWeight
        {
            get => (FontWeight)GetValue(TokenFontWeightProperty);
            set => SetValue(TokenFontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin around each token for display.
        /// </summary>
        /// <remarks>
        /// This property should not be set explicitly; it is computed from the token horizontal and vertical spacing.
        /// </remarks>
        public Thickness TokenMargin
        {
            get => (Thickness) GetValue(TokenMarginProperty);
            private set => SetValue(TokenMarginProperty, value);
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
        /// Gets or sets the <see cref="HorizontalAlignment"/> for the token and translation.
        /// </summary>
        public HorizontalAlignment TranslationAlignment
        {
            get => (HorizontalAlignment)GetValue(TranslationAlignmentProperty);
            set => SetValue(TranslationAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the translation background.
        /// </summary>
        /// <remarks>
        /// This property should normally not be set explicitly; it is computed from the token's selection status.
        /// </remarks>
        public Brush TranslationBackground
        {
            get => (Brush)GetValue(TranslationBackgroundProperty);
            set => SetValue(TranslationBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to use for displaying the translation, based on its TranslationState.
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
        /// Gets or sets the <see cref="FlowDirection"/> to use for displaying the translations.
        /// </summary>
        public FlowDirection TranslationFlowDirection
        {
            get => (FlowDirection)GetValue(TranslationFlowDirectionProperty);
            set => SetValue(TranslationFlowDirectionProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="FontFamily"/> to use for displaying the translations.
        /// </summary>
        public FontFamily TranslationFontFamily
        {
            get => (FontFamily)GetValue(TranslationFontFamilyProperty);
            set => SetValue(TranslationFontFamilyProperty, value);
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
        /// Gets or sets the font style for the translation.
        /// </summary>
        public FontStyle TranslationFontStyle
        {
            get => (FontStyle)GetValue(TranslationFontStyleProperty);
            set => SetValue(TranslationFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for the translation.
        /// </summary>
        public FontWeight TranslationFontWeight
        {
            get => (FontWeight)GetValue(TranslationFontWeightProperty);
            set => SetValue(TranslationFontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the horizontal spacing between translations.
        /// </summary>
        /// <remarks>
        /// This property should not be set explicitly; it is computed from the translation horizontal and vertical spacing.
        /// </remarks>
        public Thickness TranslationMargin
        {
            get => (Thickness) GetValue(TranslationMarginProperty);
            private set => SetValue(TranslationMarginProperty, value);
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
        /// Gets or sets the <see cref="Visibility"/> of the translation.
        /// </summary>
        /// <remarks>This should not be set explicitly; it is computed based on the <see cref="ShowTranslation"/> value.</remarks>
        public Visibility TranslationVisibility
        {
            get => (Visibility) GetValue(TranslationVisibilityProperty);
            private set => SetValue(TranslationVisibilityProperty, value);
        }

        /// <summary>
        /// Gets or sets the translation target text to be displayed.
        /// </summary>
        /// <remarks>
        /// This should normally not be called directly; it is computed based on the display properties.
        /// </remarks>
        public string TranslationText
        {
            get => (string) GetValue(TranslationTextProperty);
            set => SetValue(TranslationTextProperty, value);
        }
        #endregion Public Properties

        private void CalculateLayout()
        {
            var tokenLeftMargin = Orientation == Orientation.Horizontal ? TokenDisplayViewModel.PaddingBefore.Length * HorizontalSpacing : 0;
            var tokenRightMargin = Orientation == Orientation.Horizontal ? TokenDisplayViewModel.PaddingAfter.Length * HorizontalSpacing : 0;
            var translationLeftMargin = Orientation == Orientation.Horizontal ? Math.Max(tokenLeftMargin, HorizontalSpacing / 2) : 0;
            var translationRightMargin = Orientation == Orientation.Horizontal ? Math.Max(tokenRightMargin, HorizontalSpacing / 2) : 0;

            CompositeIndicatorMargin = new Thickness(tokenLeftMargin, 0, 0, 1);
            CompositeIndicatorVisibility = TokenDisplayViewModel.IsCompositeTokenMember ? Visibility.Visible : Visibility.Hidden;
            CompositeIndicatorComputedColor = TokenDisplayViewModel.CompositeIndicatorColor;
            
            TokenBackground = TokenDisplayViewModel.IsHighlighted ? HighlightedTokenBackground
                : TokenDisplayViewModel.IsTokenSelected ? SelectedTokenBackground
                : Brushes.Transparent;
            TokenMargin = new Thickness(tokenLeftMargin, 0, tokenRightMargin, 0);
            SurfaceText = Orientation == Orientation.Horizontal ? TokenDisplayViewModel.SurfaceText : TokenDisplayViewModel.SurfaceText.Trim();
            ExtendedProperties = TokenDisplayViewModel.ExtendedProperties;

            NoteIndicatorMargin = new Thickness(tokenLeftMargin, 1, 0, TokenVerticalSpacing);
            NoteIndicatorVisibility = (ShowNoteIndicator && TokenDisplayViewModel.HasNote) ? Visibility.Visible : Visibility.Hidden;
            NoteIndicatorComputedColor = TokenDisplayViewModel.IsNoteHovered ? Brushes.BlueViolet : NoteIndicatorColor;

            TranslationMargin = new Thickness(translationLeftMargin, 0, translationRightMargin, TranslationVerticalSpacing);
            TranslationVisibility = (ShowTranslation && TokenDisplayViewModel.Translation != null) ? Visibility.Visible : Visibility.Collapsed;
            TranslationBackground = TokenDisplayViewModel.IsTranslationSelected ? SelectedTokenBackground : Brushes.Transparent; 
            TranslationText = TokenDisplayViewModel.TargetTranslationText;
            TranslationColor = TokenDisplayViewModel.TranslationState switch
            {
                Translation.OriginatedFromValues.FromTranslationModel => Brushes.Red,
                Translation.OriginatedFromValues.FromAlignmentModel => Brushes.Red,
                Translation.OriginatedFromValues.None => Brushes.Red,
                Translation.OriginatedFromValues.FromOther => Brushes.Blue,
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
            Unloaded += OnUnloaded;

            EventAggregator?.SubscribeOnUIThread(this);
        }

        ~TokenDisplay()
        {
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;

            EventAggregator?.Unsubscribe(this);
        }

        private void OnHorizontalAlignmentChanged(object? sender, EventArgs args)
        {
            CalculateLayout();
        }
    }
}
