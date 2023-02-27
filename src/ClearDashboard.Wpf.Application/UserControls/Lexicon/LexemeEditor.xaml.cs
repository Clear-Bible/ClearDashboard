﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Events.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;

namespace ClearDashboard.Wpf.Application.UserControls.Lexicon
{
    /// <summary>
    /// A control that displays and edits the details of a <see cref="LexemeViewModel"/>.
    /// </summary>
    public partial class LexemeEditor : INotifyPropertyChanged
    {
        #region Static Routed Events

        /// <summary>
        /// Identifies the LexemeAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LexemeAddedEvent = EventManager.RegisterRoutedEvent
            (nameof(LexemeAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeEditor));

        /// <summary>
        /// Identifies the LemmaUpdatedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LemmaUpdatedEvent = EventManager.RegisterRoutedEvent
            (nameof(LemmaUpdated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeEditor));

        /// <summary>
        /// Identifies the LexemeFormAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LexemeFormAddedEvent = EventManager.RegisterRoutedEvent
            (nameof(LexemeFormAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeEditor));

        /// <summary>
        /// Identifies the LexemeFormRemovedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LexemeFormRemovedEvent = EventManager.RegisterRoutedEvent
            (nameof(LexemeFormRemoved), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeEditor));

        /// <summary>
        /// Identifies the MeaningAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent MeaningAddedEvent = EventManager.RegisterRoutedEvent
            (nameof(MeaningAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeEditor));

        /// <summary>
        /// Identifies the MeaningDeletedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent MeaningDeletedEvent = EventManager.RegisterRoutedEvent
            (nameof(MeaningDeleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeEditor));

        /// <summary>
        /// Identifies the MeaningUpdatedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent MeaningUpdatedEvent = EventManager.RegisterRoutedEvent
            (nameof(MeaningUpdated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeEditor));

        /// <summary>
        /// Identifies the SemanticDomainAdded routed event.
        /// </summary>
        public static readonly RoutedEvent SemanticDomainAddedEvent = EventManager.RegisterRoutedEvent
            (nameof(SemanticDomainAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeEditor));

        /// <summary>
        /// Identifies the SemanticDomainRemovedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent SemanticDomainRemovedEvent = EventManager.RegisterRoutedEvent
            (nameof(SemanticDomainRemoved), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeEditor));

        /// <summary>
        /// Identifies the SemanticDomainSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent SemanticDomainSelectedEvent = EventManager.RegisterRoutedEvent
            (nameof(SemanticDomainSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeEditor));

        /// <summary>
        /// Identifies the TranslationDeletedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationDeletedEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationDeleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeEditor));
        
        /// <summary>
        /// Identifies the TranslationDroppedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationDroppedEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationDropped), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeEditor));

        /// <summary>
        /// Identifies the TranslationSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationSelectedEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeEditor));


        #endregion Static Routed Events
        #region Static Dependency Properties

        /// <summary>
        /// Identifies the Lexeme dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeProperty = DependencyProperty.Register(nameof(Lexeme), typeof(LexemeViewModel), typeof(LexemeEditor));

        /// <summary>
        /// Identifies the LemmaFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty LemmaFontFamilyProperty = DependencyProperty.Register(nameof(LemmaFontFamily), typeof(FontFamily), typeof(LexemeEditor),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the LemmaFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty LemmaFontSizeProperty = DependencyProperty.Register(nameof(LemmaFontSize), typeof(double), typeof(LexemeEditor),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the LemmaFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty LemmaFontStyleProperty = DependencyProperty.Register(nameof(LemmaFontStyle), typeof(FontStyle), typeof(LexemeEditor),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the LemmaFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty LemmaFontWeightProperty = DependencyProperty.Register(nameof(LemmaFontWeight), typeof(FontWeight), typeof(LexemeEditor),
            new PropertyMetadata(FontWeights.Normal));
        
        /// <summary>
        /// Identifies the LemmaMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty LemmaMarginProperty = DependencyProperty.Register(nameof(LemmaMargin), typeof(Thickness), typeof(LexemeEditor),
            new PropertyMetadata(new Thickness(3, 0, 3, 0)));

        /// <summary>
        /// Identifies the LemmaPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty LemmaPaddingProperty = DependencyProperty.Register(nameof(LemmaPadding), typeof(Thickness), typeof(LexemeEditor),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));
        
        /// <summary>
        /// Identifies the LexemeFormBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeFormBackgroundProperty = DependencyProperty.Register(nameof(LexemeFormBackground), typeof(SolidColorBrush), typeof(LexemeEditor),
            new PropertyMetadata(Brushes.BlanchedAlmond));

        /// <summary>
        /// Identifies the LexemeFormCornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeFormCornerRadiusProperty = DependencyProperty.Register(nameof(LexemeFormCornerRadius), typeof(CornerRadius), typeof(LexemeEditor),
            new PropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// Identifies the LexemeFormFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeFormFontFamilyProperty = DependencyProperty.Register(nameof(LexemeFormFontFamily), typeof(FontFamily), typeof(LexemeEditor),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the LexemeFormFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeFormFontSizeProperty = DependencyProperty.Register(nameof(LexemeFormFontSize), typeof(double), typeof(LexemeEditor),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the LexemeFormFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeFormFontStyleProperty = DependencyProperty.Register(nameof(LexemeFormFontStyle), typeof(FontStyle), typeof(LexemeEditor),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the LexemeFormFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeFormFontWeightProperty = DependencyProperty.Register(nameof(LexemeFormFontWeight), typeof(FontWeight), typeof(LexemeEditor),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the LexemeFormMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeFormMarginProperty = DependencyProperty.Register(nameof(LexemeFormMargin), typeof(Thickness), typeof(LexemeEditor),
            new PropertyMetadata(new Thickness(3, 0, 3, 0)));

        /// <summary>
        /// Identifies the LexemeFormPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeFormPaddingProperty = DependencyProperty.Register(nameof(LexemeFormPadding), typeof(Thickness), typeof(LexemeEditor),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the MeaningTextFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningTextFontFamilyProperty = DependencyProperty.Register(nameof(MeaningTextFontFamily), typeof(FontFamily), typeof(LexemeEditor),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the MeaningTextFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningTextFontSizeProperty = DependencyProperty.Register(nameof(MeaningTextFontSize), typeof(double), typeof(LexemeEditor),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the MeaningTextFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningTextFontStyleProperty = DependencyProperty.Register(nameof(MeaningTextFontStyle), typeof(FontStyle), typeof(LexemeEditor),
            new PropertyMetadata(FontStyles.Italic));

        /// <summary>
        /// Identifies the MeaningTextFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningTextFontWeightProperty = DependencyProperty.Register(nameof(MeaningTextFontWeight), typeof(FontWeight), typeof(LexemeEditor),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the MeaningTextMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningTextMarginProperty = DependencyProperty.Register(nameof(MeaningTextMargin), typeof(Thickness), typeof(LexemeEditor),
            new PropertyMetadata(new Thickness(3, 0, 3, 0)));

        /// <summary>
        /// Identifies the MeaningTextPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningTextPaddingProperty = DependencyProperty.Register(nameof(MeaningTextPadding), typeof(Thickness), typeof(LexemeEditor),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the SemanticDomainBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainBackgroundProperty = DependencyProperty.Register(nameof(SemanticDomainBackground), typeof(SolidColorBrush), typeof(LexemeEditor),
            new PropertyMetadata(Brushes.BlanchedAlmond));

        /// <summary>
        /// Identifies the SemanticDomainCornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainCornerRadiusProperty = DependencyProperty.Register(nameof(SemanticDomainCornerRadius), typeof(CornerRadius), typeof(LexemeEditor),
            new PropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// Identifies the SemanticDomainFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainFontFamilyProperty = DependencyProperty.Register(nameof(SemanticDomainFontFamily), typeof(FontFamily), typeof(LexemeEditor),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the SemanticDomainFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainFontSizeProperty = DependencyProperty.Register(nameof(SemanticDomainFontSize), typeof(double), typeof(LexemeEditor),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the SemanticDomainFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainFontStyleProperty = DependencyProperty.Register(nameof(SemanticDomainFontStyle), typeof(FontStyle), typeof(LexemeEditor),
            new PropertyMetadata(FontStyles.Italic));

        /// <summary>
        /// Identifies the SemanticDomainFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainFontWeightProperty = DependencyProperty.Register(nameof(SemanticDomainFontWeight), typeof(FontWeight), typeof(LexemeEditor),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the SemanticDomainMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainMarginProperty = DependencyProperty.Register(nameof(SemanticDomainMargin), typeof(Thickness), typeof(LexemeEditor),
            new PropertyMetadata(new Thickness(3, 0, 3, 0)));

        /// <summary>
        /// Identifies the SemanticDomainPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainPaddingProperty = DependencyProperty.Register(nameof(SemanticDomainPadding), typeof(Thickness), typeof(LexemeEditor),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the SemanticDomainSuggestions dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainSuggestionsProperty = DependencyProperty.Register(nameof(SemanticDomainSuggestions), typeof(SemanticDomainCollection), typeof(LexemeEditor));

        /// <summary>
        /// Identifies the TranslationFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontFamilyProperty = DependencyProperty.Register(nameof(TranslationFontFamily), typeof(FontFamily), typeof(LexemeEditor),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the TranslationFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontSizeProperty = DependencyProperty.Register(nameof(TranslationFontSize), typeof(double), typeof(LexemeEditor),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the TranslationFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontStyleProperty = DependencyProperty.Register(nameof(TranslationFontStyle), typeof(FontStyle), typeof(LexemeEditor),
            new PropertyMetadata(FontStyles.Italic));

        /// <summary>
        /// Identifies the TranslationFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontWeightProperty = DependencyProperty.Register(nameof(TranslationFontWeight), typeof(FontWeight), typeof(LexemeEditor),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the TranslationMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationMarginProperty = DependencyProperty.Register(nameof(TranslationMargin), typeof(Thickness), typeof(LexemeEditor),
            new PropertyMetadata(new Thickness(3, 0, 3, 0)));

        /// <summary>
        /// Identifies the TranslationPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationPaddingProperty = DependencyProperty.Register(nameof(TranslationPadding), typeof(Thickness), typeof(LexemeEditor),
            new PropertyMetadata(new Thickness(3, 3, 3, 3)));


        #endregion
        #region Private Properties
        private string? OriginalLemmaText { get; set; } = string.Empty;

        private bool _isEditing;
        private bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(LemmaTextBlockVisibility));
                OnPropertyChanged(nameof(LemmaTextBoxVisibility));
            }
        }

        public Visibility AddLexemeVisibility => Lexeme != null ? Visibility.Collapsed : Visibility.Visible;
        public Visibility LexemeControlsVisibility => Lexeme != null ? Visibility.Visible : Visibility.Collapsed;

        public Visibility LemmaTextBlockVisibility => IsEditing ? Visibility.Hidden : Visibility.Visible;
        public Visibility LemmaTextBoxVisibility => IsEditing ? Visibility.Visible : Visibility.Hidden;

        #endregion
        #region Private Methods

        private void BeginEdit()
        {
            IsEditing = true;

            LemmaTextBox.SelectAll();
            LemmaTextBox.Focus();

            OriginalLemmaText = Lexeme?.Lemma;
        }

        private void CommitEdit()
        {
            if (LemmaTextBox.Text != OriginalLemmaText && Lexeme != null)
            {
                Lexeme.Lemma = LemmaTextBox.Text;
                RaiseLemmaEvent(LemmaUpdatedEvent);
            }

            IsEditing = false;
        }

        private void UndoEdit()
        {

        }

        private void RaiseLexemeEvent(RoutedEvent routedEvent)
        {
            RaiseEvent(new LexemeEventArgs
            {
                RoutedEvent = routedEvent,
                Lexeme = Lexeme!,
            });
        }

        private void RaiseLemmaEvent(RoutedEvent routedEvent)
        {
            RaiseEvent(new LemmaEventArgs
            {
                RoutedEvent = routedEvent,
                Lexeme = Lexeme!,
            });
        }

        private void RaiseLexemeFormEvent(RoutedEvent routedEvent, Form lexemeForm)
        {
            RaiseEvent(new LexemeFormEventArgs
            {
                RoutedEvent = routedEvent,
                Lexeme = Lexeme!,
                Form = lexemeForm
            });
        }

        private void RaiseMeaningEvent(RoutedEvent routedEvent, MeaningViewModel meaning)
        {
            RaiseEvent(new MeaningEventArgs
            {
                RoutedEvent = routedEvent,
                Lexeme = Lexeme!,
                Meaning = meaning
            });
        }

        private void RaiseSemanticDomainEvent(RoutedEvent routedEvent, SemanticDomainEventArgs args)
        {
            RaiseEvent(new SemanticDomainEventArgs
            {
                RoutedEvent = routedEvent,
                Meaning = args.Meaning,
                SemanticDomain = args.SemanticDomain
            });
        }

        private void RaiseTranslationEntryEvent(RoutedEvent routedEvent, TranslationEntryEventArgs args)
        {
            RaiseEvent(new TranslationEntryEventArgs()
            {
                RoutedEvent = routedEvent,
                Lexeme = args.Lexeme,
                Meaning = args.Meaning,
                Translation = args.Translation
            });
        }

        #endregion
        #region Private Event Handlers

        private void OnLemmaLabelClick(object sender, MouseButtonEventArgs e)
        {
            //BeginEdit();
        }

        private void OnLemmaTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitEdit();
            }

            if (e.Key == Key.Escape)
            {
                UndoEdit();
            }
        }

        private void OnLemmaTextBoxLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            CommitEdit();
        }

        private void OnAddLexemeClicked(object sender, RoutedEventArgs e)
        {
            Lexeme = new LexemeViewModel { Lemma = "Lemma" };
            RaiseLexemeEvent(LexemeAddedEvent);
            
            OnPropertyChanged(nameof(AddLexemeVisibility));
            OnPropertyChanged(nameof(LexemeControlsVisibility));

            //BeginEdit();
        }

        private void OnLexemeFormAdded(object sender, RoutedEventArgs e)
        {
            if (Lexeme != null && e is LexemeFormEventArgs args)
            {
                RaiseLexemeFormEvent(LexemeFormAddedEvent, args.Form);
                Lexeme.Forms.Add(args.Form);
            }
        }

        private void OnLexemeFormRemoved(object sender, RoutedEventArgs e)
        {
            if (Lexeme != null && e is LexemeFormEventArgs args)
            {
                Lexeme.Forms.Remove(args.Form);
                RaiseLexemeFormEvent(LexemeFormRemovedEvent, args.Form);
            }
        }

        private void OnMeaningDeleted(object sender, RoutedEventArgs e)
        {
            if (e is MeaningEventArgs args)
            {
                Lexeme?.Meanings.Remove(args.Meaning);
                RaiseMeaningEvent(MeaningDeletedEvent, args.Meaning);
            }
        }
        
        private void OnMeaningUpdated(object sender, RoutedEventArgs e)
        {
            if (e is MeaningEventArgs args)
            {
                RaiseMeaningEvent(MeaningUpdatedEvent, args.Meaning);
            }
        }

        private void OnSemanticDomainAdded(object sender, RoutedEventArgs e)
        {
            if (e is SemanticDomainEventArgs args)
            {
                RaiseSemanticDomainEvent(SemanticDomainAddedEvent, args);
            }
        }

        private void OnSemanticDomainSelected(object sender, RoutedEventArgs e)
        {
            if (e is SemanticDomainEventArgs args)
            {
                RaiseSemanticDomainEvent(SemanticDomainSelectedEvent, args);
            }
        }

        private void OnSemanticDomainRemoved(object sender, RoutedEventArgs e)
        {
            if (e is SemanticDomainEventArgs args)
            {
                RaiseSemanticDomainEvent(SemanticDomainRemovedEvent, args);
            }
        }
        private void AddMeaningClicked(object sender, RoutedEventArgs e)
        {
            var meaning = new MeaningViewModel { Text = "New Meaning" };
            Lexeme!.Meanings.Add(meaning);
            RaiseMeaningEvent(MeaningAddedEvent, meaning);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Lexeme?.Lemma == "Lemma")
            {
                BeginEdit();
                LemmaTextBox.SelectAll();
                LemmaTextBox.Focus();
            }


            OnPropertyChanged(nameof(AddLexemeVisibility));
            OnPropertyChanged(nameof(LexemeControlsVisibility));
            
            Loaded -= OnLoaded;
        }

        private void OnTranslationDeleted(object sender, RoutedEventArgs e)
        {
            if (e is TranslationEntryEventArgs args)
            {
                RaiseTranslationEntryEvent(TranslationDeletedEvent, args);
            }
        }

        private void OnTranslationDropped(object sender, RoutedEventArgs e)
        {
            if (e is TranslationEntryEventArgs args)
            RaiseTranslationEntryEvent(TranslationDroppedEvent, args);
        }

        private void OnTranslationSelected(object sender, RoutedEventArgs e)
        {
            if (e is TranslationEntryEventArgs args)
            {
                RaiseTranslationEntryEvent(TranslationSelectedEvent, args);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Private event handlers
        #region Public Properties

        /// <summary>
        /// Gets or sets the lexeme associated with the editor.
        /// </summary>
        public LexemeViewModel? Lexeme
        {
            get => (LexemeViewModel)GetValue(LexemeProperty);
            set => SetValue(LexemeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family for the lemma.
        /// </summary>
        public FontFamily LemmaFontFamily
        {
            get => (FontFamily)GetValue(LemmaFontFamilyProperty);
            set => SetValue(LemmaFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for the lemma.
        /// </summary>
        public double LemmaFontSize
        {
            get => (double)GetValue(LemmaFontSizeProperty);
            set => SetValue(LemmaFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for the lemma.
        /// </summary>
        public FontStyle LemmaFontStyle
        {
            get => (FontStyle)GetValue(LemmaFontStyleProperty);
            set => SetValue(LemmaFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for the lemma.
        /// </summary>
        public FontWeight LemmaFontWeight
        {
            get => (FontWeight)GetValue(LemmaFontWeightProperty);
            set => SetValue(LemmaFontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for the lemma.
        /// </summary>
        public Thickness LemmaMargin
        {
            get => (Thickness)GetValue(LemmaMarginProperty);
            set => SetValue(LemmaMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for the lemma.
        /// </summary>
        public Thickness LemmaPadding
        {
            get => (Thickness)GetValue(LemmaPaddingProperty);
            set => SetValue(LemmaPaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush for individual lexeme form boxes.
        /// </summary>
        public SolidColorBrush LexemeFormBackground
        {
            get => (SolidColorBrush)GetValue(LexemeFormBackgroundProperty);
            set => SetValue(LexemeFormBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the corner radius for individual lexeme form boxes.
        /// </summary>
        public CornerRadius LexemeFormCornerRadius
        {
            get => (CornerRadius)GetValue(LexemeFormCornerRadiusProperty);
            set => SetValue(LexemeFormCornerRadiusProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family for lexeme form boxes.
        /// </summary>
        public FontFamily LexemeFormFontFamily
        {
            get => (FontFamily)GetValue(LexemeFormFontFamilyProperty);
            set => SetValue(LexemeFormFontFamilyProperty, value);
        }
        /// <summary>
        /// Gets or sets the font size for individual lexeme form boxes.
        /// </summary>
        public double LexemeFormFontSize
        {
            get => (double)GetValue(LexemeFormFontSizeProperty);
            set => SetValue(LexemeFormFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for individual lexeme form boxes.
        /// </summary>
        public FontStyle LexemeFormFontStyle
        {
            get => (FontStyle)GetValue(LexemeFormFontStyleProperty);
            set => SetValue(LexemeFormFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for individual lexeme form boxes.
        /// </summary>
        public FontWeight LexemeFormFontWeight
        {
            get => (FontWeight)GetValue(LexemeFormFontWeightProperty);
            set => SetValue(LexemeFormFontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for individual lexeme form boxes.
        /// </summary>
        public Thickness LexemeFormMargin
        {
            get => (Thickness)GetValue(LexemeFormMarginProperty);
            set => SetValue(LexemeFormMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for individual lexeme form boxes.
        /// </summary>
        public Thickness LexemeFormPadding
        {
            get => (Thickness)GetValue(LexemeFormPaddingProperty);
            set => SetValue(LexemeFormPaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family for the meaning text.
        /// </summary>
        public FontFamily MeaningTextFontFamily
        {
            get => (FontFamily)GetValue(MeaningTextFontFamilyProperty);
            set => SetValue(MeaningTextFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for the meaning text.
        /// </summary>
        public double MeaningTextFontSize
        {
            get => (double)GetValue(MeaningTextFontSizeProperty);
            set => SetValue(MeaningTextFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for the meaning text.
        /// </summary>
        public FontStyle MeaningTextFontStyle
        {
            get => (FontStyle)GetValue(MeaningTextFontStyleProperty);
            set => SetValue(MeaningTextFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for the meaning text.
        /// </summary>
        public FontWeight MeaningTextFontWeight
        {
            get => (FontWeight)GetValue(MeaningTextFontWeightProperty);
            set => SetValue(MeaningTextFontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for the meaning text.
        /// </summary>
        public Thickness MeaningTextMargin
        {
            get => (Thickness)GetValue(MeaningTextMarginProperty);
            set => SetValue(MeaningTextMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for the meaning text.
        /// </summary>
        public Thickness MeaningTextPadding
        {
            get => (Thickness)GetValue(MeaningTextPaddingProperty);
            set => SetValue(MeaningTextPaddingProperty, value);
        }


        /// <summary>
        /// Gets or sets the background brush for individual semantic domain boxes.
        /// </summary>
        public SolidColorBrush SemanticDomainBackground
        {
            get => (SolidColorBrush)GetValue(SemanticDomainBackgroundProperty);
            set => SetValue(SemanticDomainBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the corner radius for individual semantic domain boxes.
        /// </summary>
        public CornerRadius SemanticDomainCornerRadius
        {
            get => (CornerRadius)GetValue(SemanticDomainCornerRadiusProperty);
            set => SetValue(SemanticDomainCornerRadiusProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family for semantic domain boxes.
        /// </summary>
        public FontFamily SemanticDomainFontFamily
        {
            get => (FontFamily)GetValue(SemanticDomainFontFamilyProperty);
            set => SetValue(SemanticDomainFontFamilyProperty, value);
        }
        /// <summary>
        /// Gets or sets the font size for individual semantic domain boxes.
        /// </summary>
        public double SemanticDomainFontSize
        {
            get => (double)GetValue(SemanticDomainFontSizeProperty);
            set => SetValue(SemanticDomainFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for the individual semantic domain boxes.
        /// </summary>
        public FontStyle SemanticDomainFontStyle
        {
            get => (FontStyle)GetValue(SemanticDomainFontStyleProperty);
            set => SetValue(SemanticDomainFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for individual semantic domain boxes.
        /// </summary>
        public FontWeight SemanticDomainFontWeight
        {
            get => (FontWeight)GetValue(SemanticDomainFontWeightProperty);
            set => SetValue(SemanticDomainFontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for individual semantic domain boxes.
        /// </summary>
        public Thickness SemanticDomainMargin
        {
            get => (Thickness)GetValue(SemanticDomainMarginProperty);
            set => SetValue(SemanticDomainMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for individual semantic domain boxes.
        /// </summary>
        public Thickness SemanticDomainPadding
        {
            get => (Thickness)GetValue(SemanticDomainPaddingProperty);
            set => SetValue(SemanticDomainPaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="SemanticDomain"/> objects for auto selection in the control.
        /// </summary>
        public SemanticDomainCollection SemanticDomainSuggestions
        {
            get => (SemanticDomainCollection)GetValue(SemanticDomainSuggestionsProperty);
            set => SetValue(SemanticDomainSuggestionsProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family for translations.
        /// </summary>
        public FontFamily TranslationFontFamily
        {
            get => (FontFamily)GetValue(TranslationFontFamilyProperty);
            set => SetValue(TranslationFontFamilyProperty, value);
        }
        /// <summary>
        /// Gets or sets the font size for translations.
        /// </summary>
        public double TranslationFontSize
        {
            get => (double)GetValue(TranslationFontSizeProperty);
            set => SetValue(TranslationFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for translations.
        /// </summary>
        public FontStyle TranslationFontStyle
        {
            get => (FontStyle)GetValue(TranslationFontStyleProperty);
            set => SetValue(MeaningTextFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for translations.
        /// </summary>
        public FontWeight TranslationFontWeight
        {
            get => (FontWeight)GetValue(TranslationFontWeightProperty);
            set => SetValue(TranslationFontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for translations.
        /// </summary>
        public Thickness TranslationMargin
        {
            get => (Thickness)GetValue(TranslationMarginProperty);
            set => SetValue(TranslationMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for translations.
        /// </summary>
        public Thickness TranslationPadding
        {
            get => (Thickness)GetValue(TranslationPaddingProperty);
            set => SetValue(TranslationPaddingProperty, value);
        }


        #endregion
        #region Public Events

        /// <summary>
        /// Occurs when a new lexeme is added.
        /// </summary>
        public event RoutedEventHandler LexemeAdded
        {
            add => AddHandler(LexemeAddedEvent, value);
            remove => RemoveHandler(LexemeAddedEvent, value);
        }

        /// <summary>
        /// Occurs when a lemma is updatd.
        /// </summary>
        public event RoutedEventHandler LemmaUpdated
        {
            add => AddHandler(LemmaUpdatedEvent, value);
            remove => RemoveHandler(LemmaUpdatedEvent, value);
        }

        /// <summary>
        /// Occurs when an new lexeme form is added.
        /// </summary>
        public event RoutedEventHandler LexemeFormAdded
        {
            add => AddHandler(LexemeFormAddedEvent, value);
            remove => RemoveHandler(LexemeFormAddedEvent, value);
        }

        /// <summary>
        /// Occurs when a lexeme form is removed.
        /// </summary>
        public event RoutedEventHandler LexemeFormRemoved
        {
            add => AddHandler(LexemeFormRemovedEvent, value);
            remove => RemoveHandler(LexemeFormRemovedEvent, value);
        }

        /// <summary>
        /// Occurs when an new meaning is added.
        /// </summary>
        public event RoutedEventHandler MeaningAdded
        {
            add => AddHandler(MeaningAddedEvent, value);
            remove => RemoveHandler(MeaningAddedEvent, value);
        }

        /// <summary>
        /// Occurs when a meaning is deleted.
        /// </summary>
        public event RoutedEventHandler MeaningDeleted
        {
            add => AddHandler(MeaningDeletedEvent, value);
            remove => RemoveHandler(MeaningDeletedEvent, value);
        }

        /// <summary>
        /// Occurs when a meaning is updated.
        /// </summary>
        public event RoutedEventHandler MeaningUpdated
        {
            add => AddHandler(MeaningUpdatedEvent, value);
            remove => RemoveHandler(MeaningUpdatedEvent, value);
        }

        /// <summary>
        /// Occurs when a new semantic domain is added.
        /// </summary>
        public event RoutedEventHandler SemanticDomainAdded
        {
            add => AddHandler(SemanticDomainAddedEvent, value);
            remove => RemoveHandler(SemanticDomainAddedEvent, value);
        }

        /// <summary>
        /// Occurs when a new semantic domain is removed.
        /// </summary>
        public event RoutedEventHandler SemanticDomainRemoved
        {
            add => AddHandler(SemanticDomainRemovedEvent, value);
            remove => RemoveHandler(SemanticDomainRemovedEvent, value);
        }

        /// <summary>
        /// Occurs when an existing semantic domain suggestion is selected.
        /// </summary>
        public event RoutedEventHandler SemanticDomainSelected
        {
            add => AddHandler(SemanticDomainSelectedEvent, value);
            remove => RemoveHandler(SemanticDomainSelectedEvent, value);
        }

        /// <summary>
        /// Occurs when a translation is deleted.
        /// </summary>
        public event RoutedEventHandler TranslationDeleted
        {
            add => AddHandler(TranslationDeletedEvent, value);
            remove => RemoveHandler(TranslationDeletedEvent, value);
        }

        /// <summary>
        /// Occurs when a translation is dropped on a meaning.
        /// </summary>
        public event RoutedEventHandler TranslationDropped
        {
            add => AddHandler(TranslationDroppedEvent, value);
            remove => RemoveHandler(TranslationDroppedEvent, value);
        }

        /// <summary>
        /// Occurs when a translation is selected.
        /// </summary>
        public event RoutedEventHandler TranslationSelected
        {
            add => AddHandler(TranslationSelectedEvent, value);
            remove => RemoveHandler(TranslationSelectedEvent, value);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion Public events

        public LexemeEditor()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

    }
}
