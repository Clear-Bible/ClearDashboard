using System.Collections;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A user control that suggests translation options and allows the user to select or enter one.
    /// </summary>
    public partial class TranslationSelector : UserControl
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
        public static readonly DependencyProperty TokenDisplayViewModelProperty = DependencyProperty.Register("TokenDisplayViewModel", typeof(TokenDisplayViewModel), typeof(TranslationSelector));

        /// <summary>
        /// Identifies the TranslationOptions dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationOptionsProperty = DependencyProperty.Register("TranslationOptions", typeof(IEnumerable), typeof(TranslationSelector));

        /// <summary>
        /// Identifies the SelectedItem dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(TranslationOption), typeof(TranslationSelector));

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
        /// Gets or sets a collection of <see cref="TokenDisplayViewModel"/> objects to display in the control.
        /// </summary>
        public IEnumerable TranslationOptions
        {
            get => (IEnumerable)GetValue(TranslationOptionsProperty);
            set => SetValue(TranslationOptionsProperty, value);
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
                Translation = new Translation(TokenDisplayViewModel.Token, TranslationValue.Text, "Assigned"),
                TranslationActionType = ApplyAllCheckbox != null && (bool) ApplyAllCheckbox.IsChecked ? TranslationActionTypes.PutPropagate : TranslationActionTypes.PutNoPropagate
            });
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs{RoutedEvent = TranslationCancelledEvent});
        }
    }
}
