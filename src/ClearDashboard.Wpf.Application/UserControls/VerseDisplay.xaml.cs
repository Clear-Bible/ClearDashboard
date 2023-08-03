using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using SIL.Extensions;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearApplicationFoundation.Framework.Input;
using ClearDashboard.Wpf.Application.Events.Notes;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control for displaying a verse, as represented by an IEnumerable of <see cref="TokenDisplayViewModel" /> instances.
    /// </summary>
    public partial class VerseDisplay : INotifyPropertyChanged,
        IHandle<SelectionUpdatedMessage>,
        IHandle<TokensUpdatedMessage>
    {
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
        /// Identifies the TokenJoinEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenJoinEvent = EventManager.RegisterRoutedEvent
            ("TokenJoin", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenJoinEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenJoinLanguagePairEvent = EventManager.RegisterRoutedEvent
            ("TokenJoinLanguagePair", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenUnjoinEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TokenUnjoinEvent = EventManager.RegisterRoutedEvent
            ("TokenUnjoin", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

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
        /// Identifies the NoteIndicatorLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorLeftButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteRightButtonDownEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorRightButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteRightButtonUpEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorRightButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseEnterEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorMouseEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseLeaveEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorMouseLeave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteIndicatorMouseWheelEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteMouseWheelEvent = EventManager.RegisterRoutedEvent
            ("NoteIndicatorMouseWheel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the NoteCreateEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteCreateEvent = EventManager.RegisterRoutedEvent
            ("NoteCreate", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the FilterPinsEvent routed event.
        /// </summary>
        public static readonly RoutedEvent FilterPinsEvent = EventManager.RegisterRoutedEvent
            ("FilterPins", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the FilterPinsByBiblicalTermsEvent routed event.
        /// </summary>
        public static readonly RoutedEvent FilterPinsByBiblicalTermsEvent = EventManager.RegisterRoutedEvent
            ("FilterPinsByBiblicalTerms", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the CopyEvent routed event.
        /// </summary>
        public static readonly RoutedEvent CopyEvent = EventManager.RegisterRoutedEvent
            ("Copy", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenCreateAlignment routed event.
        /// </summary>
        public static readonly RoutedEvent TokenCreateAlignmentEvent = EventManager.RegisterRoutedEvent
            ("TokenCreateAlignment", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TokenDeleteAlignment routed event.
        /// </summary>
        public static readonly RoutedEvent TokenDeleteAlignmentEvent = EventManager.RegisterRoutedEvent
            ("TokenDeleteAlignment", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslateQuickEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslateQuickEvent = EventManager.RegisterRoutedEvent
            ("TranslateQuick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));
        #endregion

        #region Static DependencyProperties

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
        /// Identifies the SourceFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceFontFamilyProperty = DependencyProperty.Register(nameof(SourceFontFamily), typeof(FontFamily), typeof(VerseDisplay),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the SourceFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceFontSizeProperty = DependencyProperty.Register(nameof(SourceFontSize), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(18d));

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
        /// Identifies the TargetFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetFontFamilyProperty = DependencyProperty.Register(nameof(TargetFontFamily), typeof(FontFamily), typeof(VerseDisplay),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the TargetFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetFontSizeProperty = DependencyProperty.Register(nameof(TargetFontSize), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(16d));

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
        /// Identifies the TokenVerticalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenVerticalSpacingProperty = DependencyProperty.Register(nameof(TokenVerticalSpacing), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(4d));

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
        /// Identifies the VerseBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty VerseBackgroundProperty = DependencyProperty.Register(nameof(VerseBackground), typeof(Brush), typeof(VerseDisplay),
            new PropertyMetadata(Brushes.AliceBlue));

        /// <summary>
        /// Identifies the VerseBorderBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty VerseBorderBrushProperty = DependencyProperty.Register(nameof(VerseBorderBrush), typeof(Brush), typeof(VerseDisplay),
            new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// Identifies the VerseBorderThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty VerseBorderThicknessProperty = DependencyProperty.Register(nameof(VerseBorderThickness), typeof(Thickness), typeof(VerseDisplay),
            new PropertyMetadata(new Thickness(1)));

        /// <summary>
        /// Identifies the VerseMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty VerseMarginProperty = DependencyProperty.Register(nameof(VerseMargin), typeof(Thickness), typeof(VerseDisplay),
            new PropertyMetadata(new Thickness(0, 10, 0, 10)));

        /// <summary>
        /// Identifies the VersePadding dependency property.
        /// </summary>
        public static readonly DependencyProperty VersePaddingProperty = DependencyProperty.Register(nameof(VersePadding), typeof(Thickness), typeof(VerseDisplay),
            new PropertyMetadata(new Thickness(10)));

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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CalculateItemsPanelTemplate(Wrap);
        }

        private void RaiseTokenEvent(RoutedEvent routedEvent, TokenEventArgs args)
        {
            RaiseEvent(new TokenEventArgs
            {
                RoutedEvent = routedEvent,
                TokenDisplay = args.TokenDisplay,
                SelectedTokens = VerseSelectedTokens,
                ModifierKeys = args.ModifierKeys,
            });
        }

        private void RaiseTokenEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            RaiseTokenEvent(routedEvent, (TokenEventArgs)e);
        }

        private void OnTokenClicked(object sender, RoutedEventArgs e)
        {
            if (e is not TokenEventArgs args || args is { TokenDisplay: null } )
            {
                return;
            }

            // If shift is pressed, then leave any selected tokens selected.
            if (!args.IsShiftPressed)
            {
                UpdateVerseSelection(args.TokenDisplay, args.IsControlPressed);
            }
           

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

        private void UpdateVerseSelection(TokenDisplayViewModel? token, bool addToSelection)
        {
            if (token == null)
            {
                return;
            }
            var tokenIsSelected = token.IsTokenSelected;
            if (!addToSelection)
            {
                foreach (var selectedToken in VerseSelectedTokens)
                {
                    selectedToken.IsTokenSelected = false;
                }
                VerseSelectedTokens.Clear();
            }

            token.IsTokenSelected = !tokenIsSelected;
            if (token.IsTokenSelected)
            {
                if (token.IsCompositeTokenMember)
                {
                    VerseDisplayViewModel.MatchingTokenAction(token.CompositeTokenMembers.TokenIds, t => t.IsTokenSelected = true);
                    VerseSelectedTokens.AddRange(VerseDisplayViewModel.SourceTokenDisplayViewModels.MatchingTokens(token.CompositeTokenMembers.TokenIds));
                }
                else
                {
                    VerseSelectedTokens.Add(token);
                }
            }
            else
            {
                if (token.IsCompositeTokenMember)
                {
                    var selectedCompositeTokenMembers = VerseSelectedTokens.MatchingTokens(token.CompositeTokenMembers.TokenIds);
                    VerseSelectedTokens.RemoveAll(t=>selectedCompositeTokenMembers.Contains(t));
                }
                else
                {
                    VerseSelectedTokens.Remove(token);
                }
            }
        }

        public async Task HandleAsync(SelectionUpdatedMessage message, CancellationToken cancellationToken)
        {
            VerseSelectedTokens.RemoveAll(t => !message.SelectedTokens.Contains(t));
            await Task.CompletedTask;
        }

        public async Task HandleAsync(TokensUpdatedMessage message, CancellationToken cancellationToken)
        {
            OnPropertyChanged(nameof(SourceTokens));
            await Task.CompletedTask;
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
            RaiseTokenEvent(TokenLeftButtonDownEvent, e);
        }

        private void OnTokenLeftButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenRightButtonUpEvent, e);
        }

        private void OnTokenRightButtonDown(object sender, RoutedEventArgs e)
        {
            //var control = e.Source as FrameworkElement;
            //if (control?.DataContext is TokenDisplayViewModel { IsTokenSelected: false } tokenDisplay)
            //{
            //    UpdateVerseSelection(tokenDisplay, false);
            //}

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

        private void OnTokenUnjoin(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenUnjoinEvent, e);
        }

        private void RaiseTranslationEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            var control = (TokenDisplay)e.Source;
            RaiseEvent(new TranslationEventArgs
            {
                RoutedEvent = routedEvent,
                TokenDisplay = control.TokenDisplayViewModel,
                InterlinearDisplay = VerseDisplayViewModel as InterlinearDisplayViewModel,
                Translation = control.TokenDisplayViewModel.Translation!,
            }) ;
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
            RaiseNoteEvent(FilterPinsEvent, e);
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
        /// Occurs when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

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


        public static IEventAggregator? EventAggregator { get; set; }

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
        /// Gets or sets the <see cref="FontFamily"/> to use for displaying the token.
        /// </summary>
        public FontFamily SourceFontFamily
        {
            get => (FontFamily)GetValue(SourceFontFamilyProperty);
            set => SetValue(SourceFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for the token.
        /// </summary>
        public double SourceFontSize
        {
            get => (double)GetValue(SourceFontSizeProperty);
            set => SetValue(SourceFontSizeProperty, value);
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
        /// Gets the collection of <see cref="TokenDisplayViewModel"/> source objects to display in the control.
        /// </summary>
        public IEnumerable SourceTokens => VerseDisplayViewModel?.SourceTokenDisplayViewModels;

        /// <summary>
        /// Gets or sets the <see cref="FontFamily"/> to use for displaying the target tokens.
        /// </summary>
        public FontFamily TargetFontFamily
        {
            get => (FontFamily)GetValue(TargetFontFamilyProperty);
            set => SetValue(TargetFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for the target tokens.
        /// </summary>
        public double TargetFontSize
        {
            get => (double)GetValue(TargetFontSizeProperty);
            set => SetValue(TargetFontSizeProperty, value);
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
        /// Gets the collection of <see cref="TokenDisplayViewModel"/> target objects to display in the control.
        /// </summary>
        public TokenDisplayViewModelCollection TargetTokens => VerseDisplayViewModel.TargetTokenDisplayViewModels;

        /// <summary>
        /// Gets or sets the visibility of the target (alignment) verse.
        /// </summary>
        public Visibility TargetVisibility => TargetTokens.Any() ? Visibility.Visible : Visibility.Collapsed;

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
        /// Gets or sets the <see cref="Brush"/> used to draw the background of the tokens list.
        /// </summary>
        public Brush VerseBackground
        {
            get => (Brush)GetValue(VerseBackgroundProperty);
            set => SetValue(VerseBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the border around the tokens list.
        /// </summary>
        public Brush VerseBorderBrush
        {
            get => (Brush)GetValue(VerseBorderBrushProperty);
            set => SetValue(VerseBorderBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the border thickness for the tokens list.
        /// </summary>
        public Thickness VerseBorderThickness
        {
            get => (Thickness)GetValue(VerseBorderThicknessProperty);
            set => SetValue(VerseBorderThicknessProperty, value);
        }

        /// <summary>
        /// Gets the strongly-typed VerseDisplayViewModel bound to this control.
        /// </summary>
        public VerseDisplayViewModel VerseDisplayViewModel => (DataContext.GetType().Name != "NamedObject" ? DataContext as VerseDisplayViewModel : null)!;

        /// <summary>
        /// Gets or sets the margin for the tokens list.
        /// </summary>
        public Thickness VerseMargin
        {
            get => (Thickness) GetValue(VerseMarginProperty);
            set => SetValue(VerseMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="TokenDisplayViewModel"/> objects that are selected in the UI for this verse.
        /// </summary>
        public TokenDisplayViewModelCollection VerseSelectedTokens { get; set; } = new();

        /// <summary>
        /// Gets or sets the padding for the tokens list.
        /// </summary>
        public Thickness VersePadding
        {
            get => (Thickness) GetValue(VersePaddingProperty);
            set => SetValue(VersePaddingProperty, value);
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

        public VerseDisplay()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            if (EventAggregator != null)
            {
                EventAggregator.SubscribeOnUIThread(this);
            }
        }
    }
}
