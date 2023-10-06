using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.Wpf.Application.Collections.Notes;
using ClearDashboard.Wpf.Application.Events.Notes;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;
using NotesLabel = ClearDashboard.DAL.Alignment.Notes.Label;

namespace ClearDashboard.Wpf.Application.UserControls.Notes
{
    /// <summary>
    /// A control to allow entry of note labels with autocomplete functionality.
    /// </summary>
    public partial class LabelSelector : UserControl, INotifyPropertyChanged
    {
        #region Static Routed Events

        /// <summary>
        /// Identifies the LabelAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelAddedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelSelector));
        
        /// <summary>
        /// Identifies the LabelDeletedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelDeletedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelDeleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelSelector));

        /// <summary>
        /// Identifies the LabelSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelSelectedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelSelector));

        #endregion
        #region Static Dependency Properties

        /// <summary>
        /// Identifies the LabelGroup dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelGroupProperty = DependencyProperty.Register(nameof(LabelGroup), typeof(LabelGroupViewModel), typeof(LabelSelector));
        
        /// <summary>
        /// Identifies the LabelSuggestions dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelSuggestionsProperty = DependencyProperty.Register(nameof(LabelSuggestions), typeof(LabelCollection), typeof(LabelSelector));
        
        #endregion
        #region Private event handlers

        private void RaiseLabelEvent(RoutedEvent routedEvent, NotesLabel label)
        {
            RaiseEvent(new LabelEventArgs
            {
                RoutedEvent = routedEvent,
                Label = label
            });
        }

        private void OnLabelTextChanged(object sender, TextChangedEventArgs e)
        {
            if (LabelSuggestions != null && LabelTextBox.Text.Length > 0)
            {
                LabelSuggestionListBox.ItemsSource = LabelSuggestions.Where(ls => ls.Text.Contains(LabelTextBox.Text, StringComparison.InvariantCultureIgnoreCase));
                OpenSuggestionPopup();
            }
        }

        private void OnLabelListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CloseSuggestionPopup();

            if (LabelSuggestionListBox.SelectedItem is NotesLabel selectedLabel)
            {
                RaiseLabelEvent(LabelSelectedEvent, selectedLabel);
                CloseTextBox();
            }
        }

        private void CloseTextBox()
        {
            LabelTextBox.Text = string.Empty;
            LabelSuggestionListBox.SelectedIndex = -1;
            TextBoxVisibility = Visibility.Collapsed;
            AddButtonVisibility = Visibility.Visible;
            OnPropertyChanged(nameof(TextBoxVisibility));
            OnPropertyChanged(nameof(AddButtonVisibility));
        }

        private void OnLabelTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!String.IsNullOrWhiteSpace(LabelTextBox.Text))
                {
                    var matchingLabel = LabelSuggestions?.FirstOrDefault(ls => ls.Text.Equals(LabelTextBox.Text, StringComparison.CurrentCultureIgnoreCase));
                    if (matchingLabel != null)
                    {
                        RaiseLabelEvent(LabelSelectedEvent, matchingLabel);
                    }
                    else
                    {
                        RaiseLabelEvent(LabelAddedEvent, new NotesLabel { Text = LabelTextBox.Text });
                    }
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

        void SetTextboxFocus()
        {
            LabelTextBox.Focus();
            Keyboard.Focus(LabelTextBox);
        }

        private void AddLabelButtonClicked(object sender, RoutedEventArgs e)
        {
            TextBoxVisibility = Visibility.Visible;
            AddButtonVisibility = Visibility.Hidden;
            OnPropertyChanged(nameof(TextBoxVisibility));
            OnPropertyChanged(nameof(AddButtonVisibility));

            System.Windows.Application.Current.Dispatcher.Invoke(SetTextboxFocus, DispatcherPriority.Render);
        }

        private void DeleteLabelClicked(object sender, RoutedEventArgs e)
        {
            RaiseLabelEvent(LabelDeletedEvent, (sender as Button).DataContext as NotesLabel);
        }

        private void OnToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var button = (FrameworkElement) sender;
            var label = button.DataContext as NotesLabel;

            var tooltipText = LabelGroup.IsNoneLabelGroup
                ? $"{LocalizationService!["Notes_LabelTooltipDelete"]} '{label!.Text}'"
                : $"{LocalizationService!["Notes_LabelTooltipDisassociate"]} '{label!.Text}' {LocalizationService["Notes_LabelFromLabelGroup"]} '{LabelGroup.Name}'";
            button.ToolTip = new ToolTip { Content = new TextBlock { Text = tooltipText } };
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        #region Public properties

        /// <summary>
        /// Gets or sets the <see cref="ILocalizationService"/> to be used for participating in the Caliburn Micro eventing system.
        /// </summary>
        public static ILocalizationService? LocalizationService { get; set; }

        /// <summary>
        /// Gets or sets the currently-selected <see cref="LabelGroupViewModel"/>.
        /// </summary>
        public LabelGroupViewModel LabelGroup
        {
            get => (LabelGroupViewModel)GetValue(LabelGroupProperty);
            set => SetValue(LabelGroupProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="Label"/> objects for auto selection in the control.
        /// </summary>
        public LabelCollection LabelSuggestions
        {
            get => (LabelCollection)GetValue(LabelSuggestionsProperty);
            set => SetValue(LabelSuggestionsProperty, value);
        }

        public Visibility AddButtonVisibility { get; set; } = Visibility.Visible;
        public Visibility TextBoxVisibility { get; set; } = Visibility.Collapsed;

        #endregion

        #region Public events

        /// <summary>
        /// Occurs when an new label is added.
        /// </summary>
        public event RoutedEventHandler LabelAdded
        {
            add => AddHandler(LabelAddedEvent, value);
            remove => RemoveHandler(LabelAddedEvent, value);
        }

        /// <summary>
        /// Occurs when the user asks to remove an existing label suggestion.
        /// </summary>
        public event RoutedEventHandler LabelDeleted
        {
            add => AddHandler(LabelDeletedEvent, value);
            remove => RemoveHandler(LabelDeletedEvent, value);
        }

        /// <summary>
        /// Occurs when an existing label suggestion is selected.
        /// </summary>
        public event RoutedEventHandler LabelSelected
        {
            add => AddHandler(LabelSelectedEvent, value);
            remove => RemoveHandler(LabelSelectedEvent, value);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        private void OpenSuggestionPopup()
        {
            LabelPopup.Visibility = Visibility.Visible;
            LabelPopup.IsOpen = true;
            LabelSuggestionListBox.Visibility = Visibility.Visible;
        }

        private void CloseSuggestionPopup()
        {
            LabelPopup.Visibility = Visibility.Collapsed;
            LabelPopup.IsOpen = false;
        }

        public LabelSelector()
        {
            InitializeComponent();
        }

        private void LabelTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void LabelTextBox_OnLostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            var args = e as KeyboardFocusChangedEventArgs;
            if (args?.NewFocus?.GetType() == typeof(ScrollViewer))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(SetTextboxFocus, DispatcherPriority.Render);
            }
        }
    }
}
