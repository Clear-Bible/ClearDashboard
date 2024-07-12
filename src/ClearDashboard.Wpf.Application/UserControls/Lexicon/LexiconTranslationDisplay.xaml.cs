using ClearDashboard.Wpf.Application.Events.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace ClearDashboard.Wpf.Application.UserControls.Lexicon
{
    /// <summary>
    /// A control that displays and allows selection of a single translation option and allows drag-and-drop within the meaning editor.
    /// </summary>
    public partial class LexiconTranslationDisplay : INotifyPropertyChanged
    {
        #region Static Routed Events

        /// <summary>
        /// Identifies the TranslationDeletedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationDeletedEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationDeleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexiconTranslationDisplay));

        /// <summary>
        /// Identifies the TranslationSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationSelectedEvent = EventManager.RegisterRoutedEvent
            (nameof(TranslationSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LexiconTranslationDisplay));

        #endregion Static Routed Events
        #region Static Dependency Properties

        /// <summary>
        /// Identifies the CountVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty CountVisibilityProperty = DependencyProperty.Register(nameof(CountVisibility), typeof(Visibility), typeof(LexiconTranslationDisplay),
            new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Identifies the DeleteVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty DeleteVisibilityProperty = DependencyProperty.Register(nameof(DeleteVisibility), typeof(Visibility), typeof(LexiconTranslationDisplay),
            new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(LexiconTranslationDisplay));

        #endregion Static Dependency Properties
        #region Private Methods

        private LexiconTranslationViewModel Translation => (DataContext as LexiconTranslationViewModel)!;

        private void RaiseTranslationEntryEvent(RoutedEvent routedEvent, LexiconTranslationViewModel translation)
        {
            RaiseEvent(new LexiconTranslationEventArgs()
            {
                RoutedEvent = routedEvent,
                Translation = translation
            });
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion Private Methods
        #region Private Event Handlers

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is FrameworkElement control)
                {
                    DragDrop.DoDragDrop(control, JsonSerializer.Serialize(Translation), DragDropEffects.Move);
                }
            }
        }

        private void ConfirmTranslationDeletion(object sender, RoutedEventArgs e)
        {
            ConfirmDeletePopup.IsOpen = true;
        }

        private void DeleteTranslationConfirmed(object sender, RoutedEventArgs e)
        {
            RaiseTranslationEntryEvent(TranslationDeletedEvent, Translation);
            ConfirmDeletePopup.IsOpen = false;
        }

        private void DeleteTranslationCancelled(object sender, RoutedEventArgs e)
        {
            ConfirmDeletePopup.IsOpen = false;
        }

        private void OnSelectTranslationEntry(object sender, RoutedEventArgs e)
        {
            RaiseTranslationEntryEvent(TranslationSelectedEvent, Translation);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Private Event Handlers
        #region Public Properties

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }
        /// <summary>
        /// Gets or sets the visibility of the count text.
        /// </summary>
        public Visibility CountVisibility
        {
            get => (Visibility)GetValue(CountVisibilityProperty);
            set => SetValue(CountVisibilityProperty, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the delete button.
        /// </summary>
        public Visibility DeleteVisibility
        {
            get => (Visibility)GetValue(DeleteVisibilityProperty);
            set => SetValue(DeleteVisibilityProperty, value);
        }

        #endregion Public Properties
        #region Public Events

        /// <summary>
        /// Occurs when a translation is deleted.
        /// </summary>
        public event RoutedEventHandler TranslationDeleted
        {
            add => AddHandler(TranslationDeletedEvent, value);
            remove => RemoveHandler(TranslationDeletedEvent, value);
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

        #endregion Public Events

        public LexiconTranslationDisplay()
        {
            InitializeComponent();
        }
    }
}
