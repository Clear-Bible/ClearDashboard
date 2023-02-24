using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;

namespace ClearDashboard.Wpf.Application.UserControls.Lexicon
{
    /// <summary>
    /// A control that displays and edits the details of a particular meaning within a <see cref="LexemeViewModel"/>.
    /// </summary>
    public partial class MeaningEditor : INotifyPropertyChanged
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

        private bool _isEditing;
        private bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(MeaningTextBlockVisibility));
                OnPropertyChanged(nameof(MeaningTextBoxVisibility));
            }
        }

        public Visibility MeaningTextBlockVisibility => IsEditing ? Visibility.Hidden : Visibility.Visible;
        public Visibility MeaningTextBoxVisibility => IsEditing ? Visibility.Visible : Visibility.Hidden;

        #endregion
        #region Private Methods

        private void BeginEdit()
        {
            IsEditing = true;

            MeaningTextBox.SelectAll();
            MeaningTextBox.Focus();

            OriginalMeaningText = Meaning.Text;
        }

        private void CommitEdit()
        {
            if (MeaningTextBox.Text != OriginalMeaningText)
            {
                Meaning.Text = MeaningTextBox.Text;
                RaiseMeaningEvent(MeaningUpdatedEvent);
            }

            IsEditing = false;
        }

        private void UndoEdit()
        {
            MeaningTextBox.Text = OriginalMeaningText;
            IsEditing = false;
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

        #endregion
        #region Private Event Handlers

        private void OnMeaningLabelClick(object sender, MouseButtonEventArgs e)
        {
            BeginEdit();
        }

        private void OnMeaningTextBoxKeyUp(object sender, KeyEventArgs e)
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

        private void OnMeaningTextBoxLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            CommitEdit();
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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Private event handlers
        #region Public Properties

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
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion Public events

        public MeaningEditor()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Meaning.Text == "New Meaning")
            {
                BeginEdit();
                MeaningTextBox.SelectAll();
                MeaningTextBox.Focus();
            }

            Loaded -= OnLoaded;
        }
    }
}
