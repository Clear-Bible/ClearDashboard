using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.Display;
using SIL.Extensions;
using ClearDashboard.Wpf.Application.ViewModels.Display.Messages;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control for displaying a verse, as represented by an IEnumerable of <see cref="TokenDisplayViewModel" /> instances.
    /// </summary>
    public partial class VerseDisplay : IHandle<SelectionUpdatedMessage>
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

        /// <summary>
        /// Identifies the NoteCreateEvent routed event.
        /// </summary>
        public static readonly RoutedEvent FilterPinsEvent = EventManager.RegisterRoutedEvent
            ("FilterPins", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the TranslateQuickEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslateQuickEvent = EventManager.RegisterRoutedEvent
            ("TranslateQuick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VerseDisplay));
        #endregion
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the Title Visibility dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleVisibilityProperty = DependencyProperty.Register(nameof(TitleVisibility), typeof(Visibility), typeof(VerseDisplay), new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Identifies the TitlePadding dependency property.
        /// </summary>
        public static readonly DependencyProperty TitlePaddingProperty = DependencyProperty.Register(nameof(TitlePadding), typeof(Thickness), typeof(VerseDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the TitleMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleMarginProperty = DependencyProperty.Register(nameof(TitleMargin), typeof(Thickness), typeof(VerseDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

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
        /// Identifies the TokensMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TokensMarginProperty = DependencyProperty.Register(nameof(TokensMargin), typeof(Thickness), typeof(VerseDisplay),
            new PropertyMetadata(new Thickness(0, 10, 0, 10)));

        /// <summary>
        /// Identifies the TokensPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty TokensPaddingProperty = DependencyProperty.Register(nameof(TokensPadding), typeof(Thickness), typeof(VerseDisplay),
            new PropertyMetadata(new Thickness(10)));

        /// <summary>
        /// Identifies the TokensBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty TokensBackgroundProperty = DependencyProperty.Register(nameof(TokensBackground), typeof(Brush), typeof(VerseDisplay),
            new PropertyMetadata(Brushes.AliceBlue));

        /// <summary>
        /// Identifies the TokensBorderBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty TokensBorderBrushProperty = DependencyProperty.Register(nameof(TokensBorderBrush), typeof(Brush), typeof(VerseDisplay),
            new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// Identifies the TokensBorderThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty TokensBorderThicknessProperty = DependencyProperty.Register(nameof(TokensBorderThickness), typeof(Thickness), typeof(VerseDisplay),
            new PropertyMetadata(new Thickness(1)));

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the Wrap dependency property.
        /// </summary>
        public static readonly DependencyProperty WrapProperty = DependencyProperty.Register(nameof(Wrap), typeof(bool), typeof(VerseDisplay),
            new PropertyMetadata(true, OnWrapChanged));

        /// <summary>
        /// Identifies the ItemsPanelTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsPanelTemplateProperty = DependencyProperty.Register(nameof(ItemsPanelTemplate), typeof(ItemsPanelTemplate), typeof(VerseDisplay));

        /// <summary>
        /// Identifies the Tokens dependency property.
        /// </summary>
        public static readonly DependencyProperty TokensProperty = DependencyProperty.Register(nameof(Tokens), typeof(IEnumerable), typeof(VerseDisplay));


        public static readonly DependencyProperty TargetTokensProperty = DependencyProperty.Register(nameof(TargetTokens), typeof(IEnumerable), typeof(VerseDisplay),
            new PropertyMetadata(null, OnTargetTokensChanged));

        public static void OnTargetTokensChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var verseDisplay = (VerseDisplay)d;

            if (verseDisplay.TargetTokens != null)
            {
                var tokenDisplayViewModelCollection = (TokenDisplayViewModelCollection)verseDisplay.TargetTokens!;
                verseDisplay.TargetVisibility = tokenDisplayViewModelCollection.Any() ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                verseDisplay.TargetVisibility = Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty TargetVisibilityProperty = DependencyProperty.Register(nameof(TargetVisibility), typeof(Visibility), typeof(VerseDisplay),
            new PropertyMetadata(Visibility.Collapsed));


        /// <summary>
        /// Identifies the HorizontalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(10d));

        /// <summary>
        /// Identifies the TokenVerticalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenVerticalSpacingProperty = DependencyProperty.Register(nameof(TokenVerticalSpacing), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(4d));

        /// <summary>
        /// Identifies the TranslationFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontSizeProperty = DependencyProperty.Register(nameof(TranslationFontSize), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(16d));

        /// <summary>
        /// Identifies the TranslationFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontFamilyProperty = DependencyProperty.Register(nameof(TranslationFontFamily), typeof(FontFamily), typeof(VerseDisplay),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the TranslationFlowDirection dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFlowDirectionProperty = DependencyProperty.Register(nameof(TranslationFlowDirection), typeof(FlowDirection), typeof(VerseDisplay),
            new PropertyMetadata(FlowDirection.LeftToRight));


        /// <summary>
        /// Identifies the TranslationAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationAlignmentProperty = DependencyProperty.Register(nameof(TranslationAlignment), typeof(HorizontalAlignment), typeof(VerseDisplay),
            new PropertyMetadata(HorizontalAlignment.Center));

        /// <summary>
        /// Identifies the TranslationVerticalSpacing dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationVerticalSpacingProperty = DependencyProperty.Register(nameof(TranslationVerticalSpacing), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(10d));

        /// <summary>
        /// Identifies the NoteIndicatorHeight dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorHeightProperty = DependencyProperty.Register(nameof(NoteIndicatorHeight), typeof(double), typeof(VerseDisplay),
            new PropertyMetadata(3d));

        /// <summary>
        /// Identifies the NoteIndicatorColor dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteIndicatorColorProperty = DependencyProperty.Register(nameof(NoteIndicatorColor), typeof(Brush), typeof(VerseDisplay),
            new PropertyMetadata(Brushes.LightGray));

        /// <summary>
        /// Identifies the SelectedTokenBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedTokenBackgroundProperty = DependencyProperty.Register(nameof(SelectedTokenBackground), typeof(Brush), typeof(VerseDisplay),
            new PropertyMetadata(Brushes.LightSteelBlue));

        /// <summary>
        /// Identifies the ShowTranslations dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowTranslationsProperty = DependencyProperty.Register(nameof(ShowTranslations), typeof(bool), typeof(VerseDisplay),
            new PropertyMetadata(true));

        /// <summary>
        /// Identifies the ShowNoteIndicators dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowNoteIndicatorsProperty = DependencyProperty.Register(nameof(ShowNoteIndicators), typeof(bool), typeof(VerseDisplay),
            new PropertyMetadata(true));

        #endregion Static DependencyProperties
        #region Private event handlers

        /// <summary>
        /// Callback handler for the Wrap dependency property: when the Wrap value changes, update the <see cref="ItemsPanelTemplate"/>.
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
            ItemsPanelTemplate = (ItemsPanelTemplate)FindResource(wrap ? "WrapPanelTemplate" : "StackPanelTemplate");
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CalculateItemsPanelTemplate(Wrap);
        }

        private void RaiseTokenEvent(RoutedEvent routedEvent, TokenDisplayViewModel tokenDisplay)
        {
            RaiseEvent(new TokenEventArgs
            {
                RoutedEvent = routedEvent,
                TokenDisplayViewModel = tokenDisplay,
                SelectedTokens = SelectedTokens,
                ModifierKeys = Keyboard.Modifiers,
            });
        }

        private void RaiseTokenEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            var control = e.Source as FrameworkElement;
            var tokenDisplay = control?.DataContext as TokenDisplayViewModel;
            RaiseTokenEvent(routedEvent, tokenDisplay!);
        }

        private void OnTokenClicked(object sender, RoutedEventArgs e)
        {
            var control = e.Source as FrameworkElement;
            var tokenDisplay = control?.DataContext as TokenDisplayViewModel;
            var ctrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            UpdateSelection(tokenDisplay!, ctrlPressed);

            RaiseTokenEvent(TokenClickedEvent, e);
        }

        private void UpdateSelection(TokenDisplayViewModel token, bool addToSelection)
        {
            var tokenIsSelected = token.IsSelected;
            if (!addToSelection)
            {
                foreach (var selectedToken in SelectedTokens)
                {
                    selectedToken.IsSelected = false;
                }
                SelectedTokens.Clear();
            }

            token.IsSelected = !tokenIsSelected;
            if (token.IsSelected)
            {
                SelectedTokens.Add(token);
            }
        }

        public async Task HandleAsync(SelectionUpdatedMessage message, CancellationToken cancellationToken)
        {
            SelectedTokens.RemoveAll(t => !message.SelectedTokens.Contains(t));
            await Task.CompletedTask;
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
            if (!SelectedTokens.Any())
            {
                var control = e.Source as FrameworkElement;
                var tokenDisplay = control?.DataContext as TokenDisplayViewModel;
                UpdateSelection(tokenDisplay!, false);
            }
            RaiseTokenEvent(TokenRightButtonDownEvent, e);
        }

        private void OnTokenRightButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseTokenEvent(TokenRightButtonUpEvent, e);
        }

        private void OnTokenMouseEnter(object sender, RoutedEventArgs e)
        {
            var control = e.Source as FrameworkElement;
            var tokenDisplayViewModel = control?.DataContext as TokenDisplayViewModel;

            if (DataContext is VerseDisplayViewModel verseDisplayViewModel)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0)
                {
                    if (tokenDisplayViewModel != null && verseDisplayViewModel.Alignments != null)
                    {
                        IEnumerable<Token> sourceTokens;
                        IEnumerable<Token> targetTokens;
                        if (tokenDisplayViewModel.IsSource)
                        {
                            targetTokens = verseDisplayViewModel.Alignments
                                .Where(a => a.AlignedTokenPair.SourceToken.TokenId.Equals(tokenDisplayViewModel.Token
                                    .TokenId))
                                .SelectMany(a =>
                                {
                                    if (a.AlignedTokenPair.TargetToken is not CompositeToken)
                                    {
                                        return new List<Token>() { a.AlignedTokenPair.TargetToken };
                                    }
                                    else
                                    {
                                        return ((CompositeToken)a.AlignedTokenPair.TargetToken).Tokens;
                                    }
                                });
                            ;
                            sourceTokens = verseDisplayViewModel.Alignments
                                .Where(a => a.AlignedTokenPair.SourceToken.TokenId.Equals(tokenDisplayViewModel.Token
                                    .TokenId))
                                .SelectMany(a =>
                                {
                                    if (a.AlignedTokenPair.SourceToken is not CompositeToken)
                                    {
                                        return new List<Token>() { a.AlignedTokenPair.SourceToken };
                                    }
                                    else
                                    {
                                        return ((CompositeToken)a.AlignedTokenPair.SourceToken).Tokens;
                                    }
                                });
                        }
                        else
                        {
                            sourceTokens = verseDisplayViewModel.Alignments
                                .Where(a => a.AlignedTokenPair.TargetToken.TokenId.Equals(tokenDisplayViewModel.Token
                                    .TokenId))
                                .SelectMany(a =>
                                {
                                    if (a.AlignedTokenPair.SourceToken is not CompositeToken)
                                    {
                                        return new List<Token>() { a.AlignedTokenPair.SourceToken };
                                    }
                                    else
                                    {
                                        return ((CompositeToken)a.AlignedTokenPair.SourceToken).Tokens;
                                    }
                                });
                            ;
                            targetTokens = verseDisplayViewModel.Alignments
                                .Where(a => a.AlignedTokenPair.TargetToken.TokenId.Equals(tokenDisplayViewModel.Token
                                    .TokenId))
                                .SelectMany(a =>
                                {
                                    if (a.AlignedTokenPair.TargetToken is not CompositeToken)
                                    {
                                        return new List<Token>() { a.AlignedTokenPair.TargetToken };
                                    }
                                    else
                                    {
                                        return ((CompositeToken)a.AlignedTokenPair.TargetToken).Tokens;
                                    }
                                });
                        }

                        verseDisplayViewModel.SourceTokenDisplayViewModels
                            .Select(tdm =>
                            {
                                if (sourceTokens
                                    .Select(t => t.TokenId)
                                    .Contains(tdm.Token.TokenId))
                                {
                                    tdm.IsSelected = true;
                                }
                                else
                                {
                                    tdm.IsSelected = false;
                                }

                                return tdm;
                            })
                            .ToList();
                        verseDisplayViewModel.TargetTokenDisplayViewModels
                            .Select(tdm =>
                            {
                                if (targetTokens
                                    .Select(t => t.TokenId)
                                    .Contains(tdm.Token.TokenId))
                                {
                                    tdm.IsSelected = true;
                                }
                                else
                                {
                                    tdm.IsSelected = false;
                                }

                                return tdm;
                            })
                            .ToList();
                    }
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Alt) > 0)
                {
                    verseDisplayViewModel.SourceTokenDisplayViewModels
                        .Select(tdm =>
                        {
                            tdm.IsSelected = false;
                            return tdm;
                        })
                        .ToList();
                    verseDisplayViewModel.TargetTokenDisplayViewModels
                        .Select(tdm =>
                        {
                            tdm.IsSelected = false;
                            return tdm;
                        })
                        .ToList();

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

        private void RaiseTranslationEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            var control = e.Source as TokenDisplay;
            RaiseEvent(new TranslationEventArgs
            {
                RoutedEvent = routedEvent,
                TokenDisplayViewModel = control?.TokenDisplayViewModel,
                Translation = control?.TokenDisplayViewModel?.Translation,
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
                TokenDisplayViewModel = control?.TokenDisplayViewModel,
                SelectedTokens = SelectedTokens,
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

        private void OnTranslateQuick(object sender, RoutedEventArgs e)
        {
            RaiseNoteEvent(TranslateQuickEvent, e);
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

        /// <summary>
        /// Occurs when the user requests to filter pins.
        /// </summary>
        public event RoutedEventHandler FilterPins
        {
            add => AddHandler(FilterPinsEvent, value);
            remove => RemoveHandler(FilterPinsEvent, value);
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
        /// Gets or sets whether the tokens should wrap in the control.
        /// </summary>
        public bool Wrap
        {
            get => (bool)GetValue(WrapProperty);
            set => SetValue(WrapProperty, value);
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

        public IEnumerable? TargetTokens
        {
            get => (IEnumerable)GetValue(TargetTokensProperty);
            set => SetValue(TargetTokensProperty, value);
        }

        public Visibility TargetVisibility
        {
            get => (Visibility)GetValue(TargetVisibilityProperty);
            set => SetValue(TargetVisibilityProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="TokenDisplayViewModel"/> objects that are selected in the UI.
        /// </summary>
        public TokenDisplayViewModelCollection SelectedTokens { get; set; } = new();

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
        /// Gets or sets the margin for the tokens list.
        /// </summary>
        public Thickness TokensMargin
        {
            get => (Thickness) GetValue(TokensMarginProperty);
            set => SetValue(TokensMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for the tokens list.
        /// </summary>
        public Thickness TokensPadding
        {
            get => (Thickness) GetValue(TokensPaddingProperty);
            set => SetValue(TokensPaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the background of the tokens list.
        /// </summary>
        public Brush TokensBackground
        {
            get => (Brush)GetValue(TokensBackgroundProperty);
            set => SetValue(TokensBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to draw the border around the tokens list.
        /// </summary>
        public Brush TokensBorderBrush
        {
            get => (Brush)GetValue(TokensBorderBrushProperty);
            set => SetValue(TokensBorderBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the border thickness for the tokens list.
        /// </summary>
        public Thickness TokensBorderThickness
        {
            get => (Thickness)GetValue(TokensBorderThicknessProperty);
            set => SetValue(TokensBorderThicknessProperty, value);
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
            get => (HorizontalAlignment)GetValue(TranslationAlignmentProperty);
            set => SetValue(TranslationAlignmentProperty, value);
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
        /// Gets or sets the <see cref="FontFamily"/> to use for displaying the translations.
        /// </summary>
        public FontFamily TranslationFontFamily
        {
            get => (FontFamily)GetValue(TranslationFontFamilyProperty);
            set => SetValue(TranslationFontFamilyProperty, value);
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
        #endregion Public properties

        public VerseDisplay()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public IEventAggregator EventAggregator { get; set; }

        public VerseDisplay(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
        }
    }
}
