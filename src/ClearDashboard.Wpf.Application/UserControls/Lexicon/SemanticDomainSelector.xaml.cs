using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Events.Lexicon;

namespace ClearDashboard.Wpf.Application.UserControls.Lexicon
{
    /// <summary>
    /// A control to allow entry of semantic domains with autocomplete functionality.
    /// </summary>
    public partial class SemanticDomainSelector : INotifyPropertyChanged
    {
        #region Static Routed Events
        /// <summary>
        /// Identifies the SemanticDomainSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent SemanticDomainSelectedEvent = EventManager.RegisterRoutedEvent
            ("SemanticDomainSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SemanticDomainSelector));

        /// <summary>
        /// Identifies the SemanticDomainAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent SemanticDomainAddedEvent = EventManager.RegisterRoutedEvent
            ("SemanticDomainAdded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SemanticDomainSelector));
        #endregion
        #region Static Dependency Properties

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(SemanticDomainSelector));
     
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }
        /// <summary>
        /// Identifies the SemanticDomainSuggestions dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainSuggestionsProperty = DependencyProperty.Register(nameof(SemanticDomainSuggestions), typeof(SemanticDomainCollection), typeof(SemanticDomainSelector));
        #endregion
        #region Private event handlers

        private void RaiseSemanticDomainEvent(RoutedEvent routedEvent, SemanticDomain semanticDomain)
        {
            RaiseEvent(new SemanticDomainEventArgs
            {
                RoutedEvent = routedEvent,
                SemanticDomain = semanticDomain
            });
        }

        private void OnSemanticDomainTextChanged(object sender, TextChangedEventArgs e)
        {
            if (SemanticDomainSuggestions != null && SemanticDomainTextBox.Text.Length > 0)
            {
                SemanticDomainSuggestionListBox.ItemsSource = SemanticDomainSuggestions.Where(sd => sd.Text != null && sd.Text.Contains(SemanticDomainTextBox.Text, StringComparison.InvariantCultureIgnoreCase));
                OpenSuggestionPopup();
            }
        }

        private void OnSemanticDomainListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CloseSuggestionPopup();

            if (SemanticDomainSuggestionListBox.SelectedItem is SemanticDomain selectedSemanticDomain)
            {
                RaiseSemanticDomainEvent(SemanticDomainSelectedEvent, selectedSemanticDomain);
                CloseTextBox();
            }
        }

        private void CloseTextBox()
        {
            SemanticDomainTextBox.Text = string.Empty;
            SemanticDomainSuggestionListBox.SelectedIndex = -1;
            TextBoxVisibility = Visibility.Hidden;
            OnPropertyChanged(nameof(TextBoxVisibility));
        }

        private void OnSemanticDomainTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(SemanticDomainTextBox.Text))
            {
                var matchingSemanticDomain = SemanticDomainSuggestions?.FirstOrDefault(sd => sd.Text != null && sd.Text.Equals(SemanticDomainTextBox.Text, StringComparison.CurrentCultureIgnoreCase));
                if (matchingSemanticDomain != null)
                {
                    RaiseSemanticDomainEvent(SemanticDomainSelectedEvent, matchingSemanticDomain);
                }
                else
                {
                    RaiseSemanticDomainEvent(SemanticDomainAddedEvent, new SemanticDomain {Text = SemanticDomainTextBox.Text});
                }
                CloseTextBox();
                CloseSuggestionPopup();
            }
            if (e.Key == Key.Escape)
            {
                CloseTextBox();
                CloseSuggestionPopup();
            }
        }

        private void SetTextBoxFocus()
        {
            SemanticDomainTextBox.Focus();
            Keyboard.Focus(SemanticDomainTextBox);
        }

        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            TextBoxVisibility = Visibility.Visible;
            OnPropertyChanged(nameof(TextBoxVisibility));

            System.Windows.Application.Current.Dispatcher.Invoke(SetTextBoxFocus, DispatcherPriority.Render);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        #region Public properties


        /// <summary>
        /// Gets or sets a collection of <see cref="SemanticDomain"/> objects for auto selection in the control.
        /// </summary>
        public SemanticDomainCollection SemanticDomainSuggestions
        {
            get => (SemanticDomainCollection)GetValue(SemanticDomainSuggestionsProperty);
            set => SetValue(SemanticDomainSuggestionsProperty, value);
        }

        public Visibility TextBoxVisibility { get; set; } = Visibility.Hidden;

        #endregion

        #region Public events

        /// <summary>
        /// Occurs when an existing semantic domain suggestion is selected.
        /// </summary>
        public event RoutedEventHandler SemanticDomainSelected
        {
            add => AddHandler(SemanticDomainSelectedEvent, value);
            remove => RemoveHandler(SemanticDomainSelectedEvent, value);
        }

        /// <summary>
        /// Occurs when an new semantic domain is added.
        /// </summary>
        public event RoutedEventHandler SemanticDomainAdded
        {
            add => AddHandler(SemanticDomainAddedEvent, value);
            remove => RemoveHandler(SemanticDomainAddedEvent, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        private void OpenSuggestionPopup()
        {
            SemanticDomainPopup.Visibility = Visibility.Visible;
            SemanticDomainPopup.IsOpen = true;
            SemanticDomainSuggestionListBox.Visibility = Visibility.Visible;
        }

        private void CloseSuggestionPopup()
        {
            SemanticDomainPopup.Visibility = Visibility.Collapsed;
            SemanticDomainPopup.IsOpen = false;
        }

        public SemanticDomainSelector()
        {
            InitializeComponent();
        }

        private void OnSemanticDomainTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            CloseTextBox();
        }

        private void OnSemanticDomainTextBoxLostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            CloseTextBox();
        }
    }
}
