using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.Events.Lexicon;
using ClearDashboard.Wpf.Application.Messages.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;
using FontFamily = System.Windows.Media.FontFamily;

namespace ClearDashboard.Wpf.Application.UserControls.Lexicon
{
    /// <summary>
    /// A control that displays translation details within a concordance (translations that are not part of a lexeme).
    /// </summary>
    public partial class ConcordanceDisplay : INotifyPropertyChanged, IHandle<LexiconTranslationMovedMessage>
    {
        #region Static Routed Events

        /// <summary>
        /// Identifies the TranslationAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationAddedEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ConcordanceDisplay));

        /// <summary>
        /// Identifies the NewTranslationChangedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NewTranslationChangedEvent = EventManager.RegisterRoutedEvent
            (nameof(NewTranslationChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ConcordanceDisplay));

        /// <summary>
        /// Identifies the TranslationSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationSelectedEvent = EventManager.RegisterRoutedEvent
        (nameof(TranslationSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ConcordanceDisplay));

        #endregion Static Routed Events

        #region Static Dependency Properties

        /// <summary>
        /// Identifies the NewTranslation dependency property.
        /// </summary>
        public static readonly DependencyProperty NewTranslationProperty = 
            DependencyProperty.Register(nameof(NewTranslation), typeof(LexiconTranslationViewModel), typeof(ConcordanceDisplay), new PropertyMetadata(null, OnNewTranslationPropertyChanged));

        /// <summary>
        /// Identifies the TokenDisplay dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenDisplayProperty =
            DependencyProperty.Register(nameof(TokenDisplay), typeof(TokenDisplayViewModel),
                typeof(ConcordanceDisplay));

        /// <summary>
        /// Identifies the Translations dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationsProperty =
            DependencyProperty.Register(nameof(Translations), typeof(LexiconTranslationViewModelCollection),
                typeof(ConcordanceDisplay));

        /// <summary>
        /// Identifies the TranslationFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontFamilyProperty = DependencyProperty.Register(
            nameof(TranslationFontFamily), typeof(FontFamily), typeof(ConcordanceDisplay),
            new PropertyMetadata(new FontFamily(
                new Uri(
                    "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"),
                ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the TranslationFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontSizeProperty = DependencyProperty.Register(
            nameof(TranslationFontSize), typeof(double), typeof(ConcordanceDisplay),
            new PropertyMetadata(11d));

        /// <summary>
        /// Identifies the TranslationFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontStyleProperty = DependencyProperty.Register(
            nameof(TranslationFontStyle), typeof(FontStyle), typeof(ConcordanceDisplay),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the TranslationFontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationFontWeightProperty = DependencyProperty.Register(
            nameof(TranslationFontWeight), typeof(FontWeight), typeof(ConcordanceDisplay),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the TranslationMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationMarginProperty = DependencyProperty.Register(
            nameof(TranslationMargin), typeof(Thickness), typeof(ConcordanceDisplay),
            new PropertyMetadata(new Thickness(3, 0, 3, 0)));

        /// <summary>
        /// Identifies the TranslationPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationPaddingProperty = DependencyProperty.Register(
            nameof(TranslationPadding), typeof(Thickness), typeof(ConcordanceDisplay),
            new PropertyMetadata(new Thickness(3, 3, 3, 3)));

        #endregion

        #region Private Properties

        public bool NewTranslationTextBoxIsEnabled { get; set; }

        #endregion

        #region Private Methods

        private void RaiseTranslationEntryEvent(RoutedEvent routedEvent, LexiconTranslationViewModel translation)
        {
            RaiseEvent(new LexiconTranslationEventArgs()
            {
                RoutedEvent = routedEvent,
                Translation = translation
            });
        }

        private void CommitEdit()
        {
            if (!String.IsNullOrWhiteSpace(NewTranslationTextBox.Text))
            {
                RaiseTranslationEntryEvent(TranslationAddedEvent,
                    new LexiconTranslationViewModel { Text = NewTranslationTextBox.Text });
            }
        }

        private void UndoEdit()
        {
            NewTranslationTextBox.Text = String.Empty;
        }

        #endregion

        #region Private Event Handlers

        private void OnNewTranslationTextBoxKeyUp(object sender, KeyEventArgs e)
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

        private void OnTranslationSelected(object sender, RoutedEventArgs e)
        {
            if (e is LexiconTranslationEventArgs args)
            {
                RaiseTranslationEntryEvent(TranslationSelectedEvent, args.Translation!);
            }
        }        
        
        private void OnNewTranslationChanged(object sender, RoutedEventArgs e)
        {
            NewTranslationCheckBox.IsChecked = true;
            RaiseTranslationEntryEvent(NewTranslationChangedEvent, new LexiconTranslationViewModel { Text = NewTranslationTextBox.Text });
        }

        private void OnNewTranslationChecked(object sender, RoutedEventArgs e)
        {
            NewTranslationTextBoxIsEnabled = true;
            OnPropertyChanged(nameof(NewTranslationTextBoxIsEnabled));
            NewTranslationTextBox.Focus();
            NewTranslationTextBox.CaretIndex = NewTranslationTextBox.Text.Length;
        }

        private void OnNewTranslationUnchecked(object sender, RoutedEventArgs e)
        {
            NewTranslationTextBoxIsEnabled = false;
            OnPropertyChanged(nameof(NewTranslationTextBoxIsEnabled));
        }

        public async Task HandleAsync(LexiconTranslationMovedMessage message, CancellationToken cancellationToken)
        {
            if (message.SourceTranslation.Text != null)
            {
                Translations.RemoveIfContainsText(message.SourceTranslation.Text);
            }

            await Task.CompletedTask;
        }

        private static void OnNewTranslationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var display = d as ConcordanceDisplay;
            display!.SetNewTranslation();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetNewTranslation();
            Loaded -= OnLoaded;
        }

        private void SetNewTranslation()
        {
            if (NewTranslation != null)
            {
                NewTranslationTextBox.Text = NewTranslation.Text;
                NewTranslationCheckBox.IsChecked = !string.IsNullOrWhiteSpace(NewTranslation.Text);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Private event handlers

        #region Public Properties

        public static IEventAggregator? EventAggregator { get; set; }

        /// <summary>
        /// Gets or sets the translation to be displayed in the New Translation box, if any.
        /// </summary>
        public LexiconTranslationViewModel NewTranslation
        {
            get => (LexiconTranslationViewModel)GetValue(NewTranslationProperty);
            set => SetValue(NewTranslationProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="TokenDisplayViewModel"/> that this concordance pertains to.
        /// </summary>
        public TokenDisplayViewModel TokenDisplay
        {
            get => (TokenDisplayViewModel)GetValue(TokenDisplayProperty);
            set => SetValue(TokenDisplayProperty, value);
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="LexiconTranslationViewModel"/> instances in this concordance.
        /// </summary>
        public LexiconTranslationViewModelCollection Translations
        {
            get => (LexiconTranslationViewModelCollection)GetValue(TranslationsProperty);
            set => SetValue(TranslationsProperty, value);
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
        /// Occurs when a new translation is added.
        /// </summary>
        public event RoutedEventHandler TranslationAdded
        {
            add => AddHandler(TranslationAddedEvent, value);
            remove => RemoveHandler(TranslationAddedEvent, value);
        }

        /// <summary>
        /// Occurs when a new translation is changed.
        /// </summary>
        public event RoutedEventHandler NewTranslationChanged
        {
            add => AddHandler(NewTranslationChangedEvent, value);
            remove => RemoveHandler(NewTranslationChangedEvent, value);
        }

        /// <summary>
        /// Occurs when an existing translation is selected.
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

        public ConcordanceDisplay()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            EventAggregator?.SubscribeOnUIThread(this);
        }
    }
}