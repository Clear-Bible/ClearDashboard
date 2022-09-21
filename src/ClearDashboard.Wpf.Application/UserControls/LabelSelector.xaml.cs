using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClearBible.Engine.Utils;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.Wpf.Application.Events;
using NotesLabel = ClearDashboard.DAL.Alignment.Notes.Label;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control to allow entry of note labels with autocomplete functionality.
    /// </summary>
    public partial class LabelSelector : UserControl, INotifyPropertyChanged
    {
        #region Static Routed Events
        /// <summary>
        /// Identifies the LabelSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelSelectedEvent = EventManager.RegisterRoutedEvent
            ("LabelSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelSelector));

        /// <summary>
        /// Identifies the LabelAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelAddedEvent = EventManager.RegisterRoutedEvent
            ("LabelAdded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelSelector));
        #endregion
        #region Static Dependency Properties
        /// <summary>
        /// Identifies the EntityId dependency property.
        /// </summary>
        public static readonly DependencyProperty EntityIdProperty = DependencyProperty.Register("EntityId", typeof(IId), typeof(LabelSelector));

        /// <summary>
        /// Identifies the LabelSuggestions dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelSuggestionsProperty = DependencyProperty.Register("LabelSuggestions", typeof(IEnumerable<NotesLabel>), typeof(LabelSelector));
        #endregion
        #region Private event handlers

        private void RaiseLabelEvent(RoutedEvent routedEvent, NotesLabel label)
        {
            RaiseEvent(new LabelEventArgs
            {
                RoutedEvent = routedEvent,
                EntityId = EntityId,
                Label = label
            });
        }

        private void OnLabelTextChanged(object sender, TextChangedEventArgs e)
        {
            if (LabelSuggestions != null)
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
                LabelTextBox.Text = string.Empty;
                LabelSuggestionListBox.SelectedIndex = -1;
                TextBoxVisibility = Visibility.Hidden;
                OnPropertyChanged(nameof(TextBoxVisibility));
            }
        }

        private void OnLabelTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !String.IsNullOrWhiteSpace(LabelTextBox.Text))
            {
                var matchingLabel = LabelSuggestions?.FirstOrDefault(ls => ls.Text.Equals(LabelTextBox.Text, StringComparison.CurrentCultureIgnoreCase));
                if (matchingLabel != null)
                {
                    RaiseLabelEvent(LabelSelectedEvent, matchingLabel);
                }
                else
                {
                    RaiseLabelEvent(LabelAddedEvent, new NotesLabel(null, LabelTextBox.Text));
                }
            }
        }

        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            TextBoxVisibility = Visibility.Visible;
            OnPropertyChanged(nameof(TextBoxVisibility));
            LabelTextBox.Focus();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        #region Public properties

        /// <summary>
        /// Gets or sets the <see cref="EntityId{T}"/> that this control is operating on..
        /// </summary>
        public IId? EntityId
        {
            get => (IId)GetValue(EntityIdProperty);
            set => SetValue(EntityIdProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="Label"/> objects for auto selection in the control.
        /// </summary>
        public IEnumerable<NotesLabel> LabelSuggestions
        {
            get => (IEnumerable<NotesLabel>)GetValue(LabelSuggestionsProperty);
            set => SetValue(LabelSuggestionsProperty, value);
        }

        public Visibility TextBoxVisibility { get; set; } = Visibility.Hidden;

        #endregion

        #region Public events

        /// <summary>
        /// Occurs when an existing label suggestion is selected.
        /// </summary>
        public event RoutedEventHandler LabelSelected
        {
            add => AddHandler(LabelSelectedEvent, value);
            remove => RemoveHandler(LabelSelectedEvent, value);
        }

        /// <summary>
        /// Occurs when an new label is added.
        /// </summary>
        public event RoutedEventHandler LabelAdded
        {
            add => AddHandler(LabelAddedEvent, value);
            remove => RemoveHandler(LabelAddedEvent, value);
        }

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
    }
}
