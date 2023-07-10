using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.Events.Lexicon;
using ClearDashboard.Wpf.Application.Messages.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;

namespace ClearDashboard.Wpf.Application.UserControls.Lexicon
{
    /// <summary>
    /// A control that displays and edits the details of a particular meaning within a <see cref="LexemeViewModel"/>.
    /// </summary>
    public partial class MeaningEditor : INotifyPropertyChanged, 
        IHandle<LexiconTranslationAddedMessage>,
        IHandle<LexiconTranslationMovedMessage>
    {
        #region Static Routed Events

        /// <summary>
        /// Identifies the SemanticDomainAdded routed event.
        /// </summary>
        public static readonly RoutedEvent SemanticDomainAddedEvent = EventManager.RegisterRoutedEvent
            (nameof(SemanticDomainAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MeaningEditor));

        /// <summary>
        /// Identifies the SemanticDomainRemovedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent SemanticDomainRemovedEvent = EventManager.RegisterRoutedEvent
            (nameof(SemanticDomainRemoved), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MeaningEditor));

        /// <summary>
        /// Identifies the SemanticDomainSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent SemanticDomainSelectedEvent = EventManager.RegisterRoutedEvent
            (nameof(SemanticDomainSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MeaningEditor));

        /// <summary>
        /// Identifies the MeaningDeletedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent MeaningDeletedEvent = EventManager.RegisterRoutedEvent
            (nameof(MeaningDeleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MeaningEditor));

        /// <summary>
        /// Identifies the MeaningUpdatedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent MeaningUpdatedEvent = EventManager.RegisterRoutedEvent
            (nameof(MeaningUpdated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MeaningEditor));

        /// <summary>
        /// Identifies the TranslationAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationAddedEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MeaningEditor));

        /// <summary>
        /// Identifies the TranslationDeletedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationDeletedEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationDeleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MeaningEditor));

        /// <summary>
        /// Identifies the TranslationDroppedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationDroppedEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationDropped), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MeaningEditor));

        /// <summary>
        /// Identifies the TranslationSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationSelectedEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MeaningEditor));


        #endregion Static Routed Events
        #region Static Dependency Properties

        /// <summary>
        /// Identifies the Lexeme dependency property.
        /// </summary>
        public static readonly DependencyProperty LexemeProperty = DependencyProperty.Register(nameof(Lexeme), typeof(LexemeViewModel), typeof(MeaningEditor));

        /// <summary>
        /// Identifies the Meaning dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningProperty = DependencyProperty.Register(nameof(Meaning), typeof(MeaningViewModel), typeof(MeaningEditor));

        /// <summary>
        /// Identifies the MeaningTextFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningTextFontFamilyProperty = DependencyProperty.Register(nameof(MeaningTextFontFamily), typeof(FontFamily), typeof(MeaningEditor),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the MeaningTextFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningTextFontSizeProperty = DependencyProperty.Register(nameof(MeaningTextFontSize), typeof(double), typeof(MeaningEditor),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the MeaningTextFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningTextFontStyleProperty = DependencyProperty.Register(nameof(MeaningTextFontStyle), typeof(FontStyle), typeof(MeaningEditor),
            new PropertyMetadata(FontStyles.Italic));

        /// <summary>
        /// Identifies the MeaningTextFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningTextFontWeightProperty = DependencyProperty.Register(nameof(MeaningTextFontWeight), typeof(FontWeight), typeof(MeaningEditor),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the MeaningTextMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningTextMarginProperty = DependencyProperty.Register(nameof(MeaningTextMargin), typeof(Thickness), typeof(MeaningEditor),
            new PropertyMetadata(new Thickness(3, 0, 3, 0)));

        /// <summary>
        /// Identifies the MeaningTextPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningTextPaddingProperty = DependencyProperty.Register(nameof(MeaningTextPadding), typeof(Thickness), typeof(MeaningEditor),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the SemanticDomainBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainBackgroundProperty = DependencyProperty.Register(nameof(SemanticDomainBackground), typeof(SolidColorBrush), typeof(MeaningEditor),
            new PropertyMetadata(Brushes.AliceBlue));

        /// <summary>
        /// Identifies the SemanticDomainCornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainCornerRadiusProperty = DependencyProperty.Register(nameof(SemanticDomainCornerRadius), typeof(CornerRadius), typeof(MeaningEditor),
            new PropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// Identifies the SemanticDomainFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainFontFamilyProperty = DependencyProperty.Register(nameof(SemanticDomainFontFamily), typeof(FontFamily), typeof(MeaningEditor),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the SemanticDomainFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainFontSizeProperty = DependencyProperty.Register(nameof(SemanticDomainFontSize), typeof(double), typeof(MeaningEditor),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the SemanticDomainFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainFontStyleProperty = DependencyProperty.Register(nameof(SemanticDomainFontStyle), typeof(FontStyle), typeof(MeaningEditor),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the SemanticDomainFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainFontWeightProperty = DependencyProperty.Register(nameof(SemanticDomainFontWeight), typeof(FontWeight), typeof(MeaningEditor),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the SemanticDomainMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainMarginProperty = DependencyProperty.Register(nameof(SemanticDomainMargin), typeof(Thickness), typeof(MeaningEditor),
            new PropertyMetadata(new Thickness(3, 0, 3, 0)));

        /// <summary>
        /// Identifies the SemanticDomainPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainPaddingProperty = DependencyProperty.Register(nameof(SemanticDomainPadding), typeof(Thickness), typeof(MeaningEditor),
            new PropertyMetadata(new Thickness(3, 3, 3, 3)));

        /// <summary>
        /// Identifies the SemanticDomainSuggestions dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainSuggestionsProperty = DependencyProperty.Register(nameof(SemanticDomainSuggestions), typeof(SemanticDomainCollection), typeof(MeaningEditor));

        /// <summary>
        /// Identifies the TranslationFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontFamilyProperty = DependencyProperty.Register(nameof(TranslationFontFamily), typeof(FontFamily), typeof(MeaningEditor),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the TranslationFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontSizeProperty = DependencyProperty.Register(nameof(TranslationFontSize), typeof(double), typeof(MeaningEditor),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the TranslationFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontStyleProperty = DependencyProperty.Register(nameof(TranslationFontStyle), typeof(FontStyle), typeof(MeaningEditor),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the TranslationFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontWeightProperty = DependencyProperty.Register(nameof(TranslationFontWeight), typeof(FontWeight), typeof(MeaningEditor),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the TranslationMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationMarginProperty = DependencyProperty.Register(nameof(TranslationMargin), typeof(Thickness), typeof(MeaningEditor),
            new PropertyMetadata(new Thickness(3, 0, 3, 0)));

        /// <summary>
        /// Identifies the TranslationPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationPaddingProperty = DependencyProperty.Register(nameof(TranslationPadding), typeof(Thickness), typeof(MeaningEditor),
            new PropertyMetadata(new Thickness(3, 3, 3, 3)));

        #endregion
        #region Private Properties
        private string? OriginalMeaningText { get; set; } = string.Empty;

        private bool _isEditingMeaning;
        private bool IsEditingMeaning
        {
            get => _isEditingMeaning;
            set
            {
                _isEditingMeaning = value;
                OnPropertyChanged(nameof(MeaningTextBlockVisibility));
                OnPropertyChanged(nameof(MeaningTextBoxVisibility));
            }
        }

        public Visibility MeaningTextBlockVisibility => IsEditingMeaning ? Visibility.Hidden : Visibility.Visible;
        public Visibility MeaningTextBoxVisibility => IsEditingMeaning ? Visibility.Visible : Visibility.Hidden;

        #endregion
        #region Private Methods

        private void BeginMeaningEdit()
        {
            IsEditingMeaning = true;

            MeaningTextBox.SelectAll();
            MeaningTextBox.Focus();

            OriginalMeaningText = Meaning.Text;
        }

        private void CommitMeaningEdit()
        {
            if (MeaningTextBox.Text != OriginalMeaningText)
            {
                Meaning.Text = MeaningTextBox.Text;
                RaiseMeaningEvent(MeaningUpdatedEvent);
            }

            IsEditingMeaning = false;
        }

        private void UndoMeaningEdit()
        {
            MeaningTextBox.Text = OriginalMeaningText;
            IsEditingMeaning = false;
        }        
        
        private void CommitTranslationAdd()
        {
            if (!string.IsNullOrWhiteSpace(NewTranslationTextBox.Text))
            {
                var newTranslation = new LexiconTranslationViewModel(new Translation { Text = NewTranslationTextBox.Text }, Meaning) {IsSelected = true};
                //Meaning.Translations.Add(newTranslation);
                RaiseLexicalTranslationEvent(TranslationAddedEvent, newTranslation);

                NewTranslationTextBox.Text = string.Empty;
            }
        }

        private void UndoTranslationAdd()
        {
            NewTranslationTextBox.Text = string.Empty;
        }

        private void RaiseMeaningEvent(RoutedEvent routedEvent)
        {
            RaiseEvent(new MeaningEventArgs()
            {
                RoutedEvent = routedEvent,
                Meaning = Meaning,
                Lexeme = Lexeme
            });
        }

        private void RaiseSemanticDomainEvent(RoutedEvent routedEvent, SemanticDomainEventArgs args)
        {
            RaiseEvent(new SemanticDomainEventArgs
            {
                RoutedEvent = routedEvent,
                SemanticDomain = args.SemanticDomain,
                Meaning = Meaning
            });
        }

        private void RaiseLexicalTranslationEvent(RoutedEvent routedEvent, LexiconTranslationViewModel translation)
        {
            RaiseEvent(new LexiconTranslationEventArgs()
            {
                RoutedEvent = routedEvent,
                Lexeme = Lexeme,
                Meaning = Meaning,
                Translation = translation
            });
        }

        #endregion
        #region Private Event Handlers

        private void OnMeaningLabelClick(object sender, MouseButtonEventArgs e)
        {
            BeginMeaningEdit();
        }

        private void OnMeaningTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitMeaningEdit();
            }

            if (e.Key == Key.Escape)
            {
                UndoMeaningEdit();
            }
        }

        private void OnMeaningTextBoxLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            CommitMeaningEdit();
        }

        private void OnSemanticDomainAdded(object sender, RoutedEventArgs e)
        {
            if (e is SemanticDomainEventArgs args)
            {
                Meaning.SemanticDomains.Add(args.SemanticDomain);
                RaiseSemanticDomainEvent(SemanticDomainAddedEvent, args);
            }
        }

        private void OnSemanticDomainRemoved(object sender, RoutedEventArgs e)
        {
            if (e is SemanticDomainEventArgs args)
            {
                Meaning.SemanticDomains.Remove(args.SemanticDomain);
                RaiseSemanticDomainEvent(SemanticDomainRemovedEvent, args);
            }
        }

        private void OnSemanticDomainSelected(object sender, RoutedEventArgs e)
        {
            if (e is SemanticDomainEventArgs args)
            {
                Meaning.SemanticDomains.Add(args.SemanticDomain);
                RaiseSemanticDomainEvent(SemanticDomainSelectedEvent, args);
            }
        }

        private void OnNewTranslationTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitTranslationAdd();
            }

            if (e.Key == Key.Escape)
            {
                UndoTranslationAdd();
            }
        }

        private void ConfirmMeaningDeletion(object sender, RoutedEventArgs e)
        {
            ConfirmDeletePopup.IsOpen = true;
        }

        private void DeleteMeaningConfirmed(object sender, RoutedEventArgs e)
        {
            RaiseMeaningEvent(MeaningDeletedEvent);
            ConfirmDeletePopup.IsOpen = false;
        }

        private void DeleteMeaningCancelled(object sender, RoutedEventArgs e)
        {
            ConfirmDeletePopup.IsOpen = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Meaning.Text == "New Meaning")
            {
                BeginMeaningEdit();
                MeaningTextBox.SelectAll();
                MeaningTextBox.Focus();
            }

            Loaded -= OnLoaded;
        }

        private void OnMeaningEditorDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                var dataString = (string?)e.Data.GetData(DataFormats.StringFormat);
                if (dataString != null)
                {
                    var translation = JsonSerializer.Deserialize<LexiconTranslationViewModel>(dataString);
                    if (translation != null && !translation.MeaningEquals(Meaning))
                    {
                        RaiseLexicalTranslationEvent(TranslationDroppedEvent, translation);
                        translation.Meaning = Meaning;
                        Meaning.Translations.Add(translation);
                    }
                }
            }
        }

        private void OnTranslationDeleted(object sender, RoutedEventArgs e)
        {
            if (e is LexiconTranslationEventArgs args)
            {
                RaiseLexicalTranslationEvent(TranslationDeletedEvent, args.Translation!);
                Meaning.Translations.Remove(args.Translation);
            }
        }

        private void OnTranslationSelected(object sender, RoutedEventArgs e)
        {
            if (e is LexiconTranslationEventArgs args)
            {
                RaiseLexicalTranslationEvent(TranslationSelectedEvent, args.Translation!);
            }
        }

        public async Task HandleAsync(LexiconTranslationAddedMessage message, CancellationToken cancellationToken)
        {
            if (Meaning.MeaningId != null && Meaning.MeaningId.IdEquals(message.Meaning.MeaningId))
            {
                Meaning.Translations.Add(message.Translation);
            }
            await Task.CompletedTask;
        }        
        
        public async Task HandleAsync(LexiconTranslationMovedMessage message, CancellationToken cancellationToken)
        {
            if (message.SourceMeaning != null)
            {
                if (Meaning.MeaningId != null && Meaning.MeaningId.IdEquals(message.SourceMeaning.MeaningId))
                {
                    if (!string.IsNullOrWhiteSpace(message.SourceTranslation.Text))
                    {
                        Meaning.Translations.RemoveIfContainsText(message.SourceTranslation.Text);
                    }
                }
            }

            if (Meaning.MeaningId != null && Meaning.MeaningId.IdEquals(message.TargetMeaning.MeaningId))
            {
                if (!string.IsNullOrWhiteSpace(message.SourceTranslation.Text))
                {
                    Meaning.Translations.RemoveIfContainsText(message.SourceTranslation.Text);
                }
                Meaning.Translations.Add(message.TargetTranslation);
            }
            await Task.CompletedTask;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Private event handlers
        #region Private Properties

        #endregion
        #region Public Properties

        public static IEventAggregator? EventAggregator { get; set; }

        /// <summary>
        /// Gets or sets the lexeme containing the meaning associated with the editor.
        /// </summary>
        public LexemeViewModel Lexeme
        {
            get => (LexemeViewModel)GetValue(LexemeProperty);
            set => SetValue(LexemeProperty, value);
        }

        /// <summary>
        /// Gets or sets the meaning associated with the editor.
        /// </summary>
        public MeaningViewModel Meaning
        {
            get => (MeaningViewModel)GetValue(MeaningProperty);
            set => SetValue(MeaningProperty, value);
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
        /// Gets or sets the font style for individual semantic domain boxes.
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
            set => SetValue(TranslationFontStyleProperty, value);
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
        /// Occurs when a new translation is added.
        /// </summary>
        public event RoutedEventHandler TranslationAdded
        {
            add => AddHandler(TranslationAddedEvent, value);
            remove => RemoveHandler(TranslationAddedEvent, value);
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

        public MeaningEditor()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            EventAggregator?.SubscribeOnUIThread(this);
        }
    }
}
