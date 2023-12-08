using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using SIL.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearApplicationFoundation.Framework.Input;
using System.Threading;
using System.Windows.Input;
using ClearDashboard.Wpf.Application.Events.Notes;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using System.Dynamic;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control for displaying a verse, as represented by <see cref="VerseDisplayViewModel"/> instance containing
    /// an IEnumerable of <see cref="TokenDisplayViewModel" /> instances.
    /// </summary>
    public partial class VerseDisplay : IHandle<SelectionUpdatedMessage>
    {
        #region Static RoutedEvents
        /// <summary>
        /// Identifies the AlignedTokenClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent AlignedTokenClickedEvent = EventManager.RegisterRoutedEvent
            (nameof(AlignedTokenClicked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the AlignedTokenDoubleClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent AlignedTokenDoubleClickedEvent = EventManager.RegisterRoutedEvent
            (nameof(AlignedTokenDoubleClicked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the AlignedTokenLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent AlignedTokenLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            (nameof(AlignedTokenLeftButtonDown), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the AlignedTokenLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent AlignedTokenLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            (nameof(AlignedTokenLeftButtonUp), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the AlignedTokenRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent AlignedTokenRightButtonDownEvent = EventManager.RegisterRoutedEvent
            (nameof(AlignedTokenRightButtonDown), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the AlignedTokenRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent AlignedTokenRightButtonUpEvent = EventManager.RegisterRoutedEvent
            (nameof(AlignedTokenRightButtonUp), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the AlignedTokenMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent AlignedTokenMouseEnterEvent = EventManager.RegisterRoutedEvent
            (nameof(AlignedTokenMouseEnter), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the AlignedTokenMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent AlignedTokenMouseLeaveEvent = EventManager.RegisterRoutedEvent
            (nameof(AlignedTokenMouseLeave), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the AlignedTokenMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent AlignedTokenMouseWheelEvent = EventManager.RegisterRoutedEvent
            (nameof(AlignedTokenMouseWheel), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenClickedEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenClicked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenDoubleClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenDoubleClickedEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenDoubleClicked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenLeftButtonDown), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenLeftButtonUp), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenRightButtonDownEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenRightButtonDown), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenRightButtonUpEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenRightButtonUp), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseEnterEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenMouseEnter), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseLeaveEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenMouseLeave), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenMouseWheelEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenMouseWheel), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenJoinEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenJoinEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenJoin), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenJoinEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenJoinLanguagePairEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenJoinLanguagePair), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenSplit routed event.
        /// </summary>
        public static readonly RoutedEvent TokenSplitEvent = EventManager.RegisterRoutedEvent
          (nameof(TokenSplit), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenUnjoinEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenUnjoinEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenUnjoin), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationClickedEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationClicked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationDoubleClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationDoubleClickedEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationDoubleClicked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationLeftButtonDown), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationLeftButtonUp), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationRightButtonDownEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationRightButtonDown), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationRightButtonUpEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationRightButtonUp), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationMouseEnterEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationMouseEnter), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationMouseLeaveEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationMouseLeave), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationMouseWheelEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationMouseWheel), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslationSetEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationSetEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationSet), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            (nameof(NoteLeftButtonDown), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            (nameof(NoteLeftButtonUp), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteRightButtonDownEvent = EventManager.RegisterRoutedEvent
            (nameof(NoteRightButtonDown), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteRightButtonUpEvent = EventManager.RegisterRoutedEvent
            (nameof(NoteRightButtonUp), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseEnterEvent = EventManager.RegisterRoutedEvent
            (nameof(NoteMouseEnter), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseLeaveEvent = EventManager.RegisterRoutedEvent
            (nameof(NoteMouseLeave), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseWheelEvent = EventManager.RegisterRoutedEvent
            (nameof(NoteMouseWheel), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteCreateEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteCreateEvent = EventManager.RegisterRoutedEvent
            (nameof(NoteCreate), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the FilterPinsEvent routed event.
        /// </summary>
        public static readonly RoutedEvent FilterPinsEvent = EventManager.RegisterRoutedEvent
            (nameof(FilterPins), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the FilterPinsTargetEvent routed event.
        /// </summary>
        public static readonly RoutedEvent FilterPinsTargetEvent = EventManager.RegisterRoutedEvent
            (nameof(FilterPinsTarget), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the FilterPinsByBiblicalTermsEvent routed event.
        /// </summary>
        public static readonly RoutedEvent FilterPinsByBiblicalTermsEvent = EventManager.RegisterRoutedEvent
            (nameof(FilterPinsByBiblicalTerms), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the Copy routed event.
        /// </summary>
        public static readonly RoutedEvent CopyEvent = EventManager.RegisterRoutedEvent
            (nameof(Copy), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenCreateAlignment routed event.
        /// </summary>
        public static readonly RoutedEvent TokenCreateAlignmentEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenCreateAlignment), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenDeleteAlignment routed event.
        /// </summary>
        public static readonly RoutedEvent TokenDeleteAlignmentEvent = EventManager.RegisterRoutedEvent
            (nameof(TokenDeleteAlignment), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslateQuickEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslateQuickEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslateQuick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));
        #endregion
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the AlignmentAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty AlignedTokenAlignmentProperty = DependencyProperty.Register(
            nameof(AlignedTokenAlignment), typeof(HorizontalAlignment), typeof(VerseDisplay),
            new PropertyMetadata(HorizontalAlignment.Center));

        /// <summary>
        /// Identifies the AlignedTokenColor dependency property.
        /// </summary>
        public static readonly DependencyProperty AlignedTokenColorProperty = DependencyProperty.Register(nameof(AlignedTokenColor), typeof(Brush), typeof(VerseDisplay),
                new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// Identifies the AlignedTokenFlowDirection dependency property.
        /// </summary>
        public static readonly DependencyProperty AlignedTokenFlowDirectionProperty = DependencyProperty.Register(
            nameof(AlignedTokenFlowDirection), typeof(FlowDirection), typeof(VerseDisplay),
            new PropertyMetadata(FlowDirection.LeftToRight));

        /// <summary>
        /// Identifies the AlignedTokenFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty AlignedTokenFontFamilyProperty = DependencyProperty.Register(
            nameof(AlignedTokenFontFamily), typeof(FontFamily), typeof(VerseDisplay),
            new PropertyMetadata(new FontFamily(
                new Uri(
                    "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"),
                ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the AlignedTokenFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty AlignedTokenFontSizeProperty = DependencyProperty.Register(
            nameof(AlignedTokenFontSize), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(16d));

        /// <summary>
        /// Identifies the AlignedTokenFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty AlignedTokenFontStyleProperty = DependencyProperty.Register(
            nameof(AlignedTokenFontStyle), typeof(FontStyle), typeof(VerseDisplay),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the AlignedTokenFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty AlignedTokenFontWeightProperty = DependencyProperty.Register(
            nameof(AlignedTokenFontWeight), typeof(FontWeight), typeof(VerseDisplay),
            new PropertyMetadata(FontWeights.SemiBold));

        /// <summary>
        /// Identifies the AlignedTokenPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty AlignedTokenPaddingProperty = DependencyProperty.Register(
            nameof(AlignedTokenPadding), typeof(Thickness), typeof(VerseDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the AlignedTokenVerticalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty AlignedTokenVerticalSpacingProperty = DependencyProperty.Register(
            nameof(AlignedTokenVerticalSpacing), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(10d));

        /// <summary>
        /// Identifies the HighlightedTokenBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty HighlightedTokenBackgroundProperty = DependencyProperty.Register(nameof(HighlightedTokenBackground), typeof(Brush), typeof(VerseDisplay),
            new PropertyMetadata(Brushes.Aquamarine));

        /// <summary>
        /// Identifies the HorizontalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(10d));

        /// <summary>
        /// Identifies the NoteIndicatorColor dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorColorProperty = DependencyProperty.Register(nameof(NoteIndicatorColor), typeof(Brush), typeof(VerseDisplay),
            new PropertyMetadata(Brushes.LightGray));

        /// <summary>
        /// Identifies the NoteIndicatorHeight dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorHeightProperty = DependencyProperty.Register(nameof(NoteIndicatorHeight), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(3d));
        
        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the SelectedTokenBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedTokenBackgroundProperty = DependencyProperty.Register(nameof(SelectedTokenBackground), typeof(Brush), typeof(VerseDisplay),
            new PropertyMetadata(Brushes.LightSteelBlue));

        /// <summary>
        /// Identifies the ShowAlignedTokens dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowAlignedTokensProperty = DependencyProperty.Register(nameof(ShowAlignedTokens), typeof(bool), typeof(VerseDisplay),
            new PropertyMetadata(true));

        /// <summary>
        /// Identifies the ShowNoteIndicators dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowNoteIndicatorsProperty = DependencyProperty.Register(nameof(ShowNoteIndicators), typeof(bool), typeof(VerseDisplay),
            new PropertyMetadata(true));

        /// <summary>
        /// Identifies the ShowTranslations dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowTranslationsProperty = DependencyProperty.Register(nameof(ShowTranslations), typeof(bool), typeof(VerseDisplay),
            new PropertyMetadata(true));

      
        /// <summary>
        /// Identifies the SourceFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceFontStyleProperty = DependencyProperty.Register(nameof(SourceFontStyle), typeof(FontStyle), typeof(VerseDisplay),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the SourceFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceFontWeightProperty = DependencyProperty.Register(nameof(SourceFontWeight), typeof(FontWeight), typeof(VerseDisplay),
            new PropertyMetadata(FontWeights.SemiBold));

        /// <summary>
        /// Identifies the SourceItemsPanelTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceItemsPanelTemplateProperty = DependencyProperty.Register(nameof(SourceItemsPanelTemplate), typeof(ItemsPanelTemplate), typeof(VerseDisplay));

      
        /// <summary>
        /// Identifies the TargetFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetFontStyleProperty = DependencyProperty.Register(nameof(TargetFontStyle), typeof(FontStyle), typeof(VerseDisplay),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the TargetFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetFontWeightProperty = DependencyProperty.Register(nameof(TargetFontWeight), typeof(FontWeight), typeof(VerseDisplay),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the TargetItemsPanelTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetItemsPanelTemplateProperty = DependencyProperty.Register(nameof(TargetItemsPanelTemplate), typeof(ItemsPanelTemplate), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TitleFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register(nameof(TitleFontSize), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(16d));

        /// <summary>
        /// Identifies the TitleHorizontalAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleHorizontalAlignmentProperty = DependencyProperty.Register(nameof(TitleHorizontalAlignment), typeof(HorizontalAlignment), typeof(VerseDisplay),
            new PropertyMetadata(HorizontalAlignment.Left));

        /// <summary>
        /// Identifies the TitlePadding dependency property.
        /// </summary>
        public static readonly DependencyProperty TitlePaddingProperty = DependencyProperty.Register(nameof(TitlePadding), typeof(Thickness), typeof(VerseDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TitleMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleMarginProperty = DependencyProperty.Register(nameof(TitleMargin), typeof(Thickness), typeof(VerseDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the Title Visibility dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleVisibilityProperty = DependencyProperty.Register(nameof(TitleVisibility), typeof(Visibility), typeof(VerseDisplay), new PropertyMetadata(Visibility.Visible));

      
        /// <summary>
        /// Identifies the TranslationAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationAlignmentProperty = DependencyProperty.Register(nameof(TranslationAlignment), typeof(HorizontalAlignment), typeof(VerseDisplay),
            new PropertyMetadata(HorizontalAlignment.Center));

        /// <summary>
        /// Identifies the TranslationFlowDirection dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFlowDirectionProperty = DependencyProperty.Register(nameof(TranslationFlowDirection), typeof(FlowDirection), typeof(VerseDisplay),
            new PropertyMetadata(FlowDirection.LeftToRight));

        /// <summary>
        /// Identifies the TranslationFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontFamilyProperty = DependencyProperty.Register(nameof(TranslationFontFamily), typeof(FontFamily), typeof(VerseDisplay),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the TranslationFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontSizeProperty = DependencyProperty.Register(nameof(TranslationFontSize), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(16d));

        /// <summary>
        /// Identifies the TranslationFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontStyleProperty = DependencyProperty.Register(nameof(TranslationFontStyle), typeof(FontStyle), typeof(VerseDisplay),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the TranslationFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontWeightProperty = DependencyProperty.Register(nameof(TranslationFontWeight), typeof(FontWeight), typeof(VerseDisplay),
            new PropertyMetadata(FontWeights.SemiBold));

        /// <summary>
        /// Identifies the TranslationVerticalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationVerticalSpacingProperty = DependencyProperty.Register(nameof(TranslationVerticalSpacing), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(10d));

    
        /// <summary>
        /// Identifies the Wrap dependency property.
        /// </summary>
        public static readonly DependencyProperty WrapProperty = DependencyProperty.Register(nameof(Wrap), typeof(bool), typeof(VerseDisplay),
            new PropertyMetadata(true, OnWrapChanged));


        #endregion Static DependencyProperties
        #region Private event handlers

        /// <summary>
        /// Callback handler for the Wrap dependency property: when the Wrap value changes, update the <see cref="SourceItemsPanelTemplate"/>.
        /// </summary>
        /// <param name="obj">The object whose TranslationVerticalSpacing has changed.</param>
        /// <param name="args">Event args containing the new value.</param>
        private static void OnWrapChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (VerseDisplay)obj;
            control.CalculateItemsPanelTemplate((bool)args.NewValue);
        }

        private void CalculateItemsPanelTemplate(bool wrap)
        {
            SourceItemsPanelTemplate = (ItemsPanelTemplate)FindResource(wrap ? "SourceWrapPanelTemplate" : "SourceStackPanelTemplate");
            TargetItemsPanelTemplate = (ItemsPanelTemplate)FindResource(wrap ? "TargetWrapPanelTemplate" : "TargetStackPanelTemplate");
        }

     

        private void RaiseTokenEvent(RoutedEvent routedEvent, TokenEventArgs args)
        {
            RaiseEvent(new TokenEventArgs
            {
                RoutedEvent = routedEvent,
                TokenDisplay = args.TokenDisplay,
                SelectedTokens = VerseSelectedTokens,
                ModifierKeys = args.ModifierKeys,
                MouseLeftButton = args.MouseLeftButton,
                MouseMiddleButton = args.MouseMiddleButton,
                MouseRightButton = args.MouseRightButton
            });
        }

        private void RaiseTokenEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            RaiseTokenEvent(routedEvent, (TokenEventArgs)e);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CalculateItemsPanelTemplate(Wrap);
            Loaded -= OnLoaded;
        }

        private void OnTokenClicked(object sender, RoutedEventArgs e)
        {
            if (e is not TokenEventArgs args || args is { TokenDisplay: null })
            {
                return;
            }

            ////If shift is pressed, then leave any selected tokens selected.
            //if (!args.IsShiftPressed)
            //{
            //    UpdateVerseSelection(args.TokenDisplay, args.IsControlPressed);
            //}


            RaiseTokenEvent(TokenClickedEvent, args);
        }
  
        private void OnTokenCreateAlignment(object sender, RoutedEventArgs e)
        {
            if (e is TokenEventArgs args)
            {
                RaiseTokenEvent(TokenCreateAlignmentEvent, args);
            }
        }

        private void OnTokenDeleteAlignment(object sender, RoutedEventArgs e)
        {
            if (e is TokenEventArgs args)
            {
                RaiseTokenEvent(TokenDeleteAlignmentEvent, args);
            }
        }
        private void UpdateVerseSelection(TokenDisplayViewModel? token, bool addToSelection, bool isTranslation = false)
        {
            if (token == null)
            {
                return;
            }

            var tokenIsSelected = token.IsTokenSelected;
            var translationIsSelected = token.IsTranslationSelected;

            var currentTokenShouldStaySelected =
                !addToSelection &&
                VerseSelectedTokens.Count(t => t.IsTokenSelected) + VerseSelectedTokens.Count(t => t.IsTranslationSelected) > 1 &&
                VerseSelectedTokens.Contains(token.Token.TokenId) &&
                (
                    (!isTranslation && tokenIsSelected) ||
                    (isTranslation && translationIsSelected)
                );

            if (!addToSelection)
            {
                foreach (var selectedToken in VerseSelectedTokens)
                {
                    if (!currentTokenShouldStaySelected || !selectedToken.Token.TokenId.IdEquals(token.Token.TokenId))
                    {
                        selectedToken.IsTokenSelected = false;
                        selectedToken.IsTranslationSelected = false;
                    }
                }
                VerseSelectedTokens.Clear();
            }

            if (!currentTokenShouldStaySelected)
            {
                if (!isTranslation)
                {
                    token.IsTokenSelected = !tokenIsSelected;
                }
                else
                {
                    token.IsTranslationSelected = !translationIsSelected;
                }
            }

            if (token.IsTokenSelected || token.IsTranslationSelected)
            {
                if (token.IsCompositeTokenMember)
                {
                    VerseDisplayViewModel.MatchingTokenAction(token.CompositeTokenMembers.TokenIds, t => { t.IsTokenSelected = token.IsTokenSelected; t.IsTranslationSelected = token.IsTranslationSelected; });
                    VerseSelectedTokens.AddRangeDistinct(VerseDisplayViewModel.SourceTokenDisplayViewModels.MatchingTokens(token.CompositeTokenMembers.TokenIds));
                }
                else
                {
                    VerseSelectedTokens.AddDistinct(token);
                }
            }
            else
            {
                if (token.IsCompositeTokenMember)
                {
                    var selectedCompositeTokenMembers = VerseSelectedTokens.MatchingTokens(token.CompositeTokenMembers.TokenIds);
                    VerseSelectedTokens.RemoveAll(t => selectedCompositeTokenMembers.Contains(t));
                }
                else
                {
                    VerseSelectedTokens.Remove(token);
                }
            }
        }

        private void AddTokensToSelection(TokenDisplayViewModelCollection tokens, bool checkForCompositeTokens = true)
        {
            tokens.SelectAllTokens();
            if (checkForCompositeTokens)
            {
                foreach (var token in tokens)
                {
                    if (token.IsCompositeTokenMember)
                    {
                        VerseDisplayViewModel.MatchingTokenAction(token.CompositeTokenMembers.TokenIds, t => { t.IsTokenSelected = true; });
                        VerseSelectedTokens.AddRangeDistinct(VerseDisplayViewModel.SourceTokenDisplayViewModels.MatchingTokens(token.CompositeTokenMembers.TokenIds));
                    }
                    else
                    {
                        VerseSelectedTokens.AddDistinct(token);
                    }
                }
            }
            else
            {
                VerseSelectedTokens.AddRangeDistinct(tokens);
            }
        }

        private void UpdateDragSelection(TokenDisplayViewModel token)
        {
            VerseSelectedTokens.DeselectAllTokens();
            VerseSelectedTokens.Clear();

            AddTokensToSelection(SelectionManager!.SelectedTokensBeforeDrag, false);
            AddTokensToSelection(SelectionManager!.GetDragSelection(token), true);
        }

        private void OnAlignedTokenClicked(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(AlignedTokenClickedEvent, e);
        }

        private void OnAlignedTokenDoubleClicked(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(AlignedTokenDoubleClickedEvent, e);
        }

        private void OnAlignedTokenLeftButtonDown(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(AlignedTokenLeftButtonDownEvent, e);
        }

        private void OnAlignedTokenLeftButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(AlignedTokenRightButtonUpEvent, e);
        }
        private void OnAlignedTokenRightButtonDown(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(AlignedTokenRightButtonDownEvent, e);
        }

        private void OnAlignedTokenRightButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(AlignedTokenRightButtonUpEvent, e);
        }

        private void OnAlignedTokenMouseEnter(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(AlignedTokenMouseEnterEvent, e);
        }

        private void OnAlignedTokenMouseLeave(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(AlignedTokenMouseLeaveEvent, e);
        }

        private void OnAlignedTokenMouseWheel(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(AlignedTokenMouseWheelEvent, e);
        }

        private void OnTokenDoubleClicked(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenDoubleClickedEvent, e);
        }

        private void OnTokenJoin(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenJoinEvent, e);
        }        
        
        private void OnTokenJoinLanguagePair(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenJoinLanguagePairEvent, e);
        }        
        
        private void OnTokenLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (e is not TokenEventArgs args || args is { TokenDisplay: null })
            {
                return;
            }

            // If shift is pressed, then leave any selected tokens selected.
            if (!args.IsShiftPressed)
            {
                UpdateVerseSelection(args.TokenDisplay, args.IsControlPressed);
            }

            RaiseTokenEvent(TokenLeftButtonDownEvent, e);
        }

        private void OnTokenLeftButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenRightButtonUpEvent, e);
        }

        private void OnTokenRightButtonDown(object sender, RoutedEventArgs e)
        {
            var control = e.Source as FrameworkElement;
            if (control?.DataContext is TokenDisplayViewModel { IsTokenSelected: false } tokenDisplay)
            {
                UpdateVerseSelection(tokenDisplay, false);
            }

            RaiseTokenEvent(TokenRightButtonDownEvent, e);
        }

        private void OnTokenRightButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenRightButtonUpEvent, e);
        }

        private async void OnTokenMouseEnter(object sender, RoutedEventArgs e)
        {
            var args = (TokenEventArgs)e;
            var tokenDisplayViewModel = args.TokenDisplay;

            if (DataContext is VerseDisplayViewModel verseDisplayViewModel)
            {
                if (args.IsShiftPressed)
                {
                    if (verseDisplayViewModel.AlignmentManager is { Alignments: { } })
                    {
                        await verseDisplayViewModel.HighlightTokens(tokenDisplayViewModel.IsSource, tokenDisplayViewModel.AlignmentToken.TokenId);
                        await Task.Delay(50);
                        var element = (UIElement)sender;
                        EnhancedFocusScope.SetFocusOnActiveElementInScope(element);
                    }
                }
                
                if (args.IsAltPressed)
                {
                    await verseDisplayViewModel.UnhighlightTokens();

                    await Task.Delay(50);
                    var element = (UIElement)sender;
                    EnhancedFocusScope.SetFocusOnActiveElementInScope(element);
                }

                if (args is { IsShiftPressed: false, IsAltPressed: false, IsMouseLeftButtonDown: true })
                {
                    if (SelectionManager!.IsDragInProcess)
                    {
                        UpdateDragSelection(args.TokenDisplay);
                    }
                }
            }

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

        private void OnTokenSplit(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenSplitEvent, e);
        }        
        
        private void OnTokenUnjoin(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenUnjoinEvent, e);
        }

        private void RaiseTranslationEvent(RoutedEvent routedEvent, TranslationEventArgs args)
        {
            RaiseEvent(new TranslationEventArgs
            {
                RoutedEvent = routedEvent,
                TokenDisplay = args.TokenDisplay,
                SelectedTokens = VerseSelectedTokens,
                InterlinearDisplay = VerseDisplayViewModel as InterlinearDisplayViewModel,
                Translation = args.TokenDisplay!.Translation!,
                ModifierKeys = args.ModifierKeys
            });
        }

        private void RaiseTranslationEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            RaiseTranslationEvent(routedEvent, (TranslationEventArgs)e);
        }

        private void OnTranslationClicked(object sender, RoutedEventArgs e)
        {
            if (e is not TranslationEventArgs args || args is { TokenDisplay: null })
            {
                return;
            }

            // If shift is pressed, then leave any selected tokens selected.
            if (!args.IsShiftPressed)
            {
                UpdateVerseSelection(args.TokenDisplay, args.IsControlPressed, true);
            }

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

        private void OnTranslationSet(object sender, RoutedEventArgs e)
        {
            RaiseTranslationEvent(TranslationSetEvent, e);
        }

        private void RaiseNoteEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            //4
            var control = e.Source as TokenDisplay;
            RaiseEvent(new NoteEventArgs
            {
                RoutedEvent = routedEvent,
                TokenDisplayViewModel = control?.TokenDisplayViewModel!,
                SelectedTokens = VerseSelectedTokens,
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

        private void OnFilterPins(object sender, RoutedEventArgs e)
        {
            //3
            RaiseNoteEvent(FilterPinsEvent, e);
        }

        private void OnFilterPinsTarget(object sender, RoutedEventArgs e)
        {
            //3
            RaiseNoteEvent(FilterPinsTargetEvent, e);
        }

        private void OnFilterPinsByBiblicalTerms(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(FilterPinsByBiblicalTermsEvent, e);
        }

        private void OnCopy(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(CopyEvent, e);
        }

        private void OnTranslateQuick(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(TranslateQuickEvent, e);
        }

        // ReSharper restore UnusedMember.Global

        #endregion
        #region Public events

        // ReSharper disable UnusedMember.Global

        /// <summary>
        /// Occurs when an aligned token is clicked.
        /// </summary>
        public event RoutedEventHandler AlignedTokenClicked
        {
            add => AddHandler(AlignedTokenClickedEvent, value);
            remove => RemoveHandler(AlignedTokenClickedEvent, value);
        }

        /// <summary>
        /// Occurs when an aligned token is clicked two or more times.
        /// </summary>
        public event RoutedEventHandler AlignedTokenDoubleClicked
        {
            add => AddHandler(AlignedTokenDoubleClickedEvent, value);
            remove => RemoveHandler(AlignedTokenDoubleClickedEvent, value);
        }

        /// <summary>
        /// Occurs when the left mouse button is pressed while the mouse pointer is over an aligned token.
        /// </summary>
        public event RoutedEventHandler AlignedTokenLeftButtonDown
        {
            add => AddHandler(AlignedTokenLeftButtonDownEvent, value);
            remove => RemoveHandler(AlignedTokenLeftButtonDownEvent, value);
        }

        /// <summary>
        /// Occurs when the left mouse button is released while the mouse pointer is over an aligned token.
        /// </summary>
        public event RoutedEventHandler AlignedTokenLeftButtonUp
        {
            add => AddHandler(AlignedTokenLeftButtonUpEvent, value);
            remove => RemoveHandler(AlignedTokenLeftButtonUpEvent, value);
        }

        /// <summary>
        /// Occurs when the right mouse button is pressed while the mouse pointer is over an aligned token.
        /// </summary>
        public event RoutedEventHandler AlignedTokenRightButtonDown
        {
            add => AddHandler(AlignedTokenRightButtonDownEvent, value);
            remove => RemoveHandler(AlignedTokenRightButtonDownEvent, value);
        }

        /// <summary>
        /// Occurs when the right mouse button is released while the mouse pointer is over an aligned token.
        /// </summary>
        public event RoutedEventHandler AlignedTokenRightButtonUp
        {
            add => AddHandler(AlignedTokenRightButtonUpEvent, value);
            remove => RemoveHandler(AlignedTokenRightButtonUpEvent, value);
        }

        /// <summary>
        /// Occurs when the mouse pointer enters the bounds of an aligned token.
        /// </summary>
        public event RoutedEventHandler AlignedTokenMouseEnter
        {
            add => AddHandler(AlignedTokenMouseEnterEvent, value);
            remove => RemoveHandler(AlignedTokenMouseEnterEvent, value);
        }

        /// <summary>
        /// Occurs when the mouse pointer leaves the bounds of an aligned token.
        /// </summary>
        public event RoutedEventHandler AlignedTokenMouseLeave
        {
            add => AddHandler(AlignedTokenMouseLeaveEvent, value);
            remove => RemoveHandler(AlignedTokenMouseLeaveEvent, value);
        }

        /// <summary>
        /// Occurs when the user rotates the mouse wheel while the mouse pointer is over an aligned token.
        /// </summary>
        public event RoutedEventHandler AlignedTokenMouseWheel
        {
            add => AddHandler(AlignedTokenMouseWheelEvent, value);
            remove => RemoveHandler(AlignedTokenMouseWheelEvent, value);
        }

      
        /// <summary>
        /// Occurs when an individual token is clicked.
        /// </summary>
        public event RoutedEventHandler TokenClicked
        {
            add => AddHandler(TokenClickedEvent, value);
            remove => RemoveHandler(TokenClickedEvent, value);
        }

        /// <summary>
        /// Occurs when an alignment is created.
        /// </summary>
        public event RoutedEventHandler TokenCreateAlignment
        {
            add => AddHandler(TokenCreateAlignmentEvent, value);
            remove => RemoveHandler(TokenCreateAlignmentEvent, value);
        }

        /// <summary>
        /// Occurs when an alignment is deleted.
        /// </summary>
        public event RoutedEventHandler TokenDeleteAlignment
        {
            add => AddHandler(TokenDeleteAlignmentEvent, value);
            remove => RemoveHandler(TokenDeleteAlignmentEvent, value);
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
        /// Occurs when the user requests to split a token.
        /// </summary>
        public event RoutedEventHandler TokenSplit
        {
          add => AddHandler(TokenSplitEvent, value);
          remove => RemoveHandler(TokenSplitEvent, value);
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
        /// Occurs when an individual translation is clicked.
        /// </summary>
        public event RoutedEventHandler TranslationSet
        {
            add => AddHandler(TranslationSetEvent, value);
            remove => RemoveHandler(TranslationSetEvent, value);
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

        /// <summary>
        /// Occurs when the user requests to filter pins.
        /// </summary>
        public event RoutedEventHandler FilterPins
        {
            add => AddHandler(FilterPinsEvent, value);
            remove => RemoveHandler(FilterPinsEvent, value);
        }


        /// <summary>
        /// Occurs when the user requests to filter pins.
        /// </summary>
        public event RoutedEventHandler FilterPinsTarget
        {
            add => AddHandler(FilterPinsTargetEvent, value);
            remove => RemoveHandler(FilterPinsTargetEvent, value);
        }

        /// <summary>
        /// Occurs when the user requests to filter pins by biblical terms.
        /// </summary>
        public event RoutedEventHandler FilterPinsByBiblicalTerms
        {
            add => AddHandler(FilterPinsByBiblicalTermsEvent, value);
            remove => RemoveHandler(FilterPinsByBiblicalTermsEvent, value);
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
        #region Public properties

        /// <summary>
        /// Gets or sets a collection of <see cref="TokenDisplayViewModel"/> objects that are selected across all displays.
        /// </summary>
        public TokenDisplayViewModelCollection AllSelectedTokens { get; set; } = new();

        /// <summary>
        /// Gets or sets the <see cref="HorizontalAlignment"/> for the aligned text.
        /// </summary>
        public HorizontalAlignment AlignedTokenAlignment
        {
            get => (HorizontalAlignment)GetValue(AlignedTokenAlignmentProperty);
            set => SetValue(AlignedTokenAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to use for displaying the aligned text.
        /// </summary>
        public Brush AlignedTokenColor
        {
            get => (Brush)GetValue(AlignedTokenColorProperty);
            set => SetValue(AlignedTokenColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="FlowDirection"/> to use for displaying the aligned text.
        /// </summary>
        public FlowDirection AlignedTokenFlowDirection
        {
            get => (FlowDirection)GetValue(AlignedTokenFlowDirectionProperty);
            set => SetValue(AlignedTokenFlowDirectionProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="FontFamily"/> to use for displaying the aligned text.
        /// </summary>
        public FontFamily AlignedTokenFontFamily
        {
            get => (FontFamily)GetValue(AlignedTokenFontFamilyProperty);
            set => SetValue(AlignedTokenFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for the aligned text.
        /// </summary>
        public double AlignedTokenFontSize
        {
            get => (double)GetValue(AlignedTokenFontSizeProperty);
            set => SetValue(AlignedTokenFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for the aligned text.
        /// </summary>
        public FontStyle AlignedTokenFontStyle
        {
            get => (FontStyle)GetValue(AlignedTokenFontStyleProperty);
            set => SetValue(AlignedTokenFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for the aligned text.
        /// </summary>
        public FontWeight AlignedTokenFontWeight
        {
            get => (FontWeight)GetValue(AlignedTokenFontWeightProperty);
            set => SetValue(AlignedTokenFontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for the aligned text.
        /// </summary>
        public Thickness AlignedTokenPadding
        {
            get => (Thickness)GetValue(AlignedTokenPaddingProperty);
            set => SetValue(AlignedTokenPaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets the vertical spacing below the aligned token.
        /// </summary>
        public double AlignedTokenVerticalSpacing
        {
            get => (double)GetValue(AlignedTokenVerticalSpacingProperty);
            set => SetValue(AlignedTokenVerticalSpacingProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the background of highlighted tokens.
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
        /// This is a relative factor that will ultimately depend on the token's <see cref="TokenDisplayViewModel.PaddingBefore"/> and <see cref="TokenDisplayViewModel.PaddingAfter"/> values.
        /// </remarks>
        public double HorizontalSpacing
        {
            get => (double)GetValue(HorizontalSpacingProperty);
            set => SetValue(HorizontalSpacingProperty, value);
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
        /// Gets or sets the height of the note indicator.
        /// </summary>
        public double NoteIndicatorHeight
        {
            get => (double)GetValue(NoteIndicatorHeightProperty);
            set => SetValue(NoteIndicatorHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the orientation for displaying the tokens.
        /// </summary>
        /// <remarks>
        /// This controls the layout of the tokens to be displayed; regardless of this setting, any translation will be displayed vertically below the token.
        /// </remarks>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the background of selected tokens.
        /// </summary>
        public Brush SelectedTokenBackground
        {
            get => (Brush)GetValue(SelectedTokenBackgroundProperty);
            set => SetValue(SelectedTokenBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets whether to display aligned tokens.
        /// </summary>
        public bool ShowAlignedTokens
        {
            get => (bool)GetValue(ShowAlignedTokensProperty);
            set => SetValue(ShowAlignedTokensProperty, value);
        }

        /// <summary>
        /// Gets or sets whether to display note indicators.
        /// </summary>
        public bool ShowNoteIndicators
        {
            get => (bool)GetValue(ShowNoteIndicatorsProperty);
            set => SetValue(ShowNoteIndicatorsProperty, value);
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
        /// Gets or sets the font style for the token.
        /// </summary>
        public FontStyle SourceFontStyle
        {
            get => (FontStyle)GetValue(SourceFontStyleProperty);
            set => SetValue(SourceFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for the token.
        /// </summary>
        public FontWeight SourceFontWeight
        {
            get => (FontWeight)GetValue(SourceFontWeightProperty);
            set => SetValue(SourceFontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="SourceItemsPanelTemplate"/> to use when rendering the control.
        /// </summary>
        /// <remarks>This should normally not be set directly, as it is determined by the value of the <see cref="Wrap"/> property.</remarks>
        private ItemsPanelTemplate SourceItemsPanelTemplate
        {
            get => (ItemsPanelTemplate)GetValue(SourceItemsPanelTemplateProperty);
            set => SetValue(SourceItemsPanelTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for the target tokens.
        /// </summary>
        public FontStyle TargetFontStyle
        {
            get => (FontStyle)GetValue(TargetFontStyleProperty);
            set => SetValue(TargetFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for the target.
        /// </summary>
        public FontWeight TargetFontWeight
        {
            get => (FontWeight)GetValue(TargetFontWeightProperty);
            set => SetValue(TargetFontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="TargetItemsPanelTemplate"/> to use when rendering the target tokens.
        /// </summary>
        /// <remarks>This should normally not be set directly, as it is determined by the value of the <see cref="Wrap"/> property.</remarks>
        private ItemsPanelTemplate TargetItemsPanelTemplate
        {
            get => (ItemsPanelTemplate)GetValue(TargetItemsPanelTemplateProperty);
            set => SetValue(TargetItemsPanelTemplateProperty, value);
        }

      
        /// <summary>
        /// Gets or sets the title to be displayed for the verse.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
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
        /// Gets or sets the margin of the title to be displayed for the verse.
        /// </summary>
        public Thickness TitleMargin
        {
            get => (Thickness)GetValue(TitleMarginProperty);
            set => SetValue(TitleMarginProperty, value);
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
        /// Gets or sets the title to be displayed for the verse.
        /// </summary>
        public Visibility TitleVisibility
        {
            get => (Visibility)GetValue(TitleVisibilityProperty);
            set => SetValue(TitleVisibilityProperty, value);
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
            get => (double)GetValue(TranslationFontSizeProperty);
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
        /// Gets or sets the vertical spacing below the translation.
        /// </summary>
        public double TranslationVerticalSpacing
        {
            get => (double)GetValue(TranslationVerticalSpacingProperty);
            set => SetValue(TranslationVerticalSpacingProperty, value);
        }

  



        /// <summary>
        /// Gets or sets whether the tokens should wrap in the control.
        /// </summary>
        public bool Wrap
        {
            get => (bool)GetValue(WrapProperty);
            set => SetValue(WrapProperty, value);
        }

        #endregion Public properties

        public async Task HandleAsync(SelectionUpdatedMessage message, CancellationToken cancellationToken)
        {
            VerseSelectedTokens.RemoveAll(t => !message.SelectedTokens.Contains(t));
            await Task.CompletedTask;
        }

        public VerseDisplay()
        {
            InitializeComponent();

            Loaded += OnLoaded;

        }

        private async void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            // get the mouse position relative to the icon
            var mousePosition = e.GetPosition(ExternalNotesIcon);

            // get current mouse position on the screen
            var screenPoint = ExternalNotesIcon.PointToScreen(mousePosition);


            dynamic settings = new ExpandoObject();
            settings.MinWidth = 500;
            settings.MinHeight = 500;
            settings.Height = 500;
            settings.MaxWidth = 800;
            settings.MaxHeight = 700;
            settings.Top = Mouse.GetPosition(this).Y + screenPoint.Y;
            settings.Left = Mouse.GetPosition(this).X + screenPoint.X;

            var viewModel = IoC.Get<ExternalNoteViewModel>();
            await viewModel.Initialize(VerseDisplayViewModel.ExternalNotes);

            IWindowManager manager = new WindowManager();
            manager.ShowWindowAsync(viewModel, null, settings);

        }
    }
}
