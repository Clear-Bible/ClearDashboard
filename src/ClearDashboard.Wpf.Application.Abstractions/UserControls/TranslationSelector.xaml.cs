using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A user control that suggests translation options and allows the user to select or enter one.
    /// </summary>
    public partial class TranslationSelector : INotifyPropertyChanged
    {
        /// <summary>
        /// Identifies the TranslationApplied routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationAppliedEvent = EventManager.RegisterRoutedEvent
            ("TranslationApplied", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationSelector));

        /// <summary>
        /// Identifies the TranslationCancelled routed event.
        /// </summary>
        public static readonly RoutedEvent TranslationCancelledEvent = EventManager.RegisterRoutedEvent
            ("TranslationCancelled", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TranslationSelector));

        /// <summary>
        /// Identifies the TokenDisplayProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenDisplayViewModelProperty = DependencyProperty.Register(nameof(TokenDisplayViewModel), typeof(TokenDisplayViewModel), typeof(TranslationSelector));

        /// <summary>
        /// Identifies the TranslationOptions dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationOptionsProperty = DependencyProperty.Register(nameof(TranslationOptions), typeof(IEnumerable), typeof(TranslationSelector));

        /// <summary>
        /// Identifies the SelectedItem dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(TranslationOption), typeof(TranslationSelector));

        /// <summary>
        /// Identifies the TranslationControlsVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationControlsVisibilityProperty = DependencyProperty.Register(nameof(TranslationControlsVisibility), typeof(Visibility), typeof(TranslationSelector),
            new PropertyMetadata(Visibility.Hidden));

        /// <summary>
        /// Identifies the ApplyButtonVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationOptionsVisibilityProperty = DependencyProperty.Register(nameof(TranslationOptionsVisibility), typeof(Visibility), typeof(TranslationSelector),
            new PropertyMetadata(Visibility.Hidden));

        /// <summary>
        /// Occurs when a translation is applied.
        /// </summary>
        public event RoutedEventHandler TranslationApplied
        {
            add => AddHandler(TranslationAppliedEvent, value);
            remove => RemoveHandler(TranslationAppliedEvent, value);
        }

        /// <summary>
        /// Occurs when a translation is cancelled.
        /// </summary>
        public event RoutedEventHandler TranslationCancelled
        {
            add => AddHandler(TranslationCancelledEvent, value);
            remove => RemoveHandler(TranslationCancelledEvent, value);
        }

        /// <summary>
        /// Gets or sets the currently selected item.
        /// </summary>
        public TranslationOption SelectedItem
        {
            get => (TranslationOption)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="TokenDisplayViewModel"/> token display information to display in this control.
        /// </summary>
        public TokenDisplayViewModel TokenDisplayViewModel
        {
            get => (TokenDisplayViewModel)GetValue(TokenDisplayViewModelProperty);
            set => SetValue(TokenDisplayViewModelProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the translation controls should be visible.
        /// </summary>
        public Visibility TranslationControlsVisibility
        {
            get => (Visibility)GetValue(TranslationControlsVisibilityProperty);
            set => SetValue(TranslationControlsVisibilityProperty, value);
        }        
        
        /// <summary>
        /// Gets or sets whether the translation options grid should be visible.
        /// </summary>
        public Visibility TranslationOptionsVisibility
        {
            get => (Visibility)GetValue(TranslationOptionsVisibilityProperty);
            set => SetValue(TranslationOptionsVisibilityProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="TokenDisplayViewModel"/> objects to display in the control.
        /// </summary>
        public IEnumerable TranslationOptions
        {
            get => (IEnumerable)GetValue(TranslationOptionsProperty);
            set
            {
                SetValue(TranslationOptionsProperty, value);

                TranslationControlsVisibility = Visibility.Visible;
                OnPropertyChanged(nameof(TranslationControlsVisibility));

                TranslationOptionsVisibility = Visibility.Visible;
                OnPropertyChanged(nameof(TranslationOptionsVisibility));
            }
        }

        public TranslationSelector()
        {
            InitializeComponent();
        }

        private void ApplyTranslation(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new TranslationEventArgs
            {
                RoutedEvent = TranslationAppliedEvent,
                TokenDisplay = TokenDisplayViewModel,
                Translation = new Translation(TokenDisplayViewModel.TokenForTranslation, TranslationValue.Text, Translation.OriginatedFromValues.Assigned),
                TranslationActionType = ApplyAllCheckbox != null && (bool) ApplyAllCheckbox.IsChecked ? TranslationActionTypes.PutPropagate : TranslationActionTypes.PutNoPropagate
            });
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs{RoutedEvent = TranslationCancelledEvent});
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
