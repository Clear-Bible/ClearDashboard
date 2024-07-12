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
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Events.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ClearDashboard.Wpf.Application.UserControls.Lexicon
{
    /// <summary>
    /// A control to allow entry of lexeme forms.
    /// </summary>
    public partial class LexemeFormSelector : INotifyPropertyChanged                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
    {
        #region Static Routed Events
        /// <summary>
        /// Identifies the LexemeFormAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LexemeFormAddedEvent = EventManager.RegisterRoutedEvent
            ("LexemeFormAdded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexemeFormSelector));
        #endregion
        #region Private event handlers

        private void RaiseLexemeFormEvent(RoutedEvent routedEvent, Form form)
        {
            RaiseEvent(new LexemeFormEventArgs
            {
                RoutedEvent = routedEvent,
                Form = form
            });
        }

        private void CloseTextBox()
        {
            LexemeFormTextBox.Text = string.Empty;
            TextBoxVisibility = Visibility.Hidden;
            OnPropertyChanged(nameof(TextBoxVisibility));
        }

        private void OnLexemeFormTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(LexemeFormTextBox.Text))
            {
                RaiseLexemeFormEvent(LexemeFormAddedEvent, new Form {Text = LexemeFormTextBox.Text});
                CloseTextBox();
            }

            if (e.Key == Key.Escape)
            {
                CloseTextBox();
            }
        }

        private void SetTextBoxFocus()
        {
            LexemeFormTextBox.Focus();
            Keyboard.Focus(LexemeFormTextBox);
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

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(LexemeFormSelector));
        public Visibility TextBoxVisibility { get; set; } = Visibility.Hidden;

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        #endregion

        #region Public events

        /// <summary>
        /// Occurs when an new lexeme form is added.
        /// </summary>
        public event RoutedEventHandler LexemeFormAdded
        {
            add => AddHandler(LexemeFormAddedEvent, value);
            remove => RemoveHandler(LexemeFormAddedEvent, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        public LexemeFormSelector()
        {
            InitializeComponent();
        }

        private void OnLexemeFormTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            CloseTextBox();
        }

        private void OnLexemeFormTextBoxLostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            CloseTextBox();
        }
    }
}
