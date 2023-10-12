using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Collections.Notes;
using ClearDashboard.Wpf.Application.Events.Notes;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using NotesLabel = ClearDashboard.DAL.Alignment.Notes.Label;

namespace ClearDashboard.Wpf.Application.UserControls.Notes
{
    /// <summary>
    /// A control for displaying a collection of <see cref="NotesLabel"/> values.
    /// </summary>
    public partial class LabelsDisplay : INotifyPropertyChanged, IHandle<NoteLabelAttachedMessage>, IHandle<NoteLabelDetachedMessage>
    {
        #region Static Routed Events

        /// <summary>
        /// Identifies the LabelRemovedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelRemovedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelRemoved), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsDisplay));
        
        /// <summary>
        /// Identifies the LabelUpdatedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelUpdatedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelUpdated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsDisplay));
        
        #endregion
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the LabelBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelBackgroundProperty = DependencyProperty.Register(nameof(LabelBackground), typeof(SolidColorBrush), typeof(LabelsDisplay));

        /// <summary>
        /// Identifies the LabelCornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelCornerRadiusProperty = DependencyProperty.Register(nameof(LabelCornerRadius), typeof(CornerRadius), typeof(LabelsDisplay),
            new PropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// Identifies the LabelFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelFormFontFamilyProperty = DependencyProperty.Register(nameof(LabelFontFamily), typeof(FontFamily), typeof(LabelsDisplay),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the LabelFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelFontSizeProperty = DependencyProperty.Register(nameof(LabelFontSize), typeof(double), typeof(LabelsDisplay),
            new PropertyMetadata(14d));

        /// <summary>
        /// Identifies the LabelFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelFontStyleProperty = DependencyProperty.Register(nameof(LabelFontStyle), typeof(FontStyle), typeof(LabelsDisplay),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the LabelFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelFontWeightProperty = DependencyProperty.Register(nameof(LabelFontWeight), typeof(FontWeight), typeof(LabelsDisplay),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the LabelMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelMarginProperty = DependencyProperty.Register(nameof(LabelMargin), typeof(Thickness), typeof(LabelsDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 2)));

        /// <summary>
        /// Identifies the LabelPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelPaddingProperty = DependencyProperty.Register(nameof(LabelPadding), typeof(Thickness), typeof(LabelsDisplay),
            new PropertyMetadata(new Thickness(10, 6, 10, 50)));

        /// <summary>
        /// Identifies the LabelWidth dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(nameof(LabelWidth), typeof(double), typeof(LabelsDisplay),
            new PropertyMetadata(Double.NaN));

        /// <summary>
        /// Identifies the Labels dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelsProperty = DependencyProperty.Register(nameof(Labels), typeof(LabelCollection), typeof(LabelsDisplay));

        /// <summary>
        /// Identifies the Note dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(NoteViewModel), typeof(LabelsDisplay));

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(LabelsDisplay));

        #endregion Static DependencyProperties
        #region Private event handlers

        private void RaiseLabelEvent(RoutedEvent routedEvent, NotesLabel label)
        {
            RaiseEvent(new LabelEventArgs
            {
                RoutedEvent = routedEvent,
                Note = Note,
                Label = label
            });
        }

        private void OnRemoveLabel(object sender, RoutedEventArgs e)
        {
            var control = e.Source as FrameworkElement;
            if (control?.DataContext is NotesLabel label)
            {
                RaiseLabelEvent(LabelRemovedEvent, label);
            }
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private NotesLabel? _currentLabel;
        private string? _currentLabelText;
        private string? _currentLabelTemplate;

        /// <summary>
        /// Gets or sets the label being edited.
        /// </summary>
        public NotesLabel? CurrentLabel
        {
            get => _currentLabel;
            set
            {
                _currentLabel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the text of the label being edited.
        /// </summary>
        public string? CurrentLabelText
        {
            get => _currentLabelText;
            set
            {
                if (value == _currentLabelText) return;
                _currentLabelText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the template of the label being edited.
        /// </summary>
        public string? CurrentLabelTemplate
        {
            get => _currentLabelTemplate;
            set
            {
                if (value == _currentLabelTemplate) return;
                _currentLabelTemplate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="EventAggregator"/> to be used for participating in the Caliburn Micro eventing system.
        /// </summary>
        public static IEventAggregator? EventAggregator { get; set; }

        private void OnEditLabelClicked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                CurrentLabel = item.DataContext as NotesLabel;
                CurrentLabelText = CurrentLabel?.Text;
                CurrentLabelTemplate = CurrentLabel?.TemplateText ?? string.Empty;
            }

            EditLabelTextPopup.IsOpen = true;
            LabelTemplateTextBox.Focus();
        }

        private void OnEditLabelConfirmed(object sender, RoutedEventArgs e)
        {
            if (CurrentLabel != null && CurrentLabel?.TemplateText != CurrentLabelTemplate)
            {
                CurrentLabel!.TemplateText = CurrentLabelTemplate;
                RaiseLabelEvent(LabelUpdatedEvent, CurrentLabel);
            }
            EditLabelTextPopup.IsOpen = false;
        }

        private void OnEditLabelCancelled(object sender, RoutedEventArgs e)
        {
            EditLabelTextPopup.IsOpen = false;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= OnUnloaded;
            EventAggregator?.Unsubscribe(this);
        }

        public async Task HandleAsync(NoteLabelDetachedMessage message, CancellationToken cancellationToken)
        {
            void RemoveLabel()
            {
                Labels.RemoveIfExists(message.Label);
                OnPropertyChanged(nameof(Labels));
                Labels.Refresh();
            }
            Execute.OnUIThread(RemoveLabel);

            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteLabelAttachedMessage message, CancellationToken cancellationToken)
        {
            void AddLabel()
            {
                Labels.AddDistinct(message.Label);
                OnPropertyChanged(nameof(Labels));
                Labels.Refresh();
            }
            Execute.OnUIThread(AddLabel);

            await Task.CompletedTask;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        #region Public properties

        /// <summary>
        /// Gets or sets the orientation for displaying the labels.
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush for individual label boxes.
        /// </summary>
        public SolidColorBrush LabelBackground
        {
            get => (SolidColorBrush)GetValue(LabelBackgroundProperty);
            set => SetValue(LabelBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the corner radius for individual label boxes.
        /// </summary>
        public CornerRadius LabelCornerRadius
        {
            get => (CornerRadius)GetValue(LabelCornerRadiusProperty);
            set => SetValue(LabelCornerRadiusProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family for individual label boxes.
        /// </summary>
        public FontFamily LabelFontFamily
        {
            get => (FontFamily)GetValue(LabelFormFontFamilyProperty);
            set => SetValue(LabelFormFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for individual label boxes.
        /// </summary>
        public double LabelFontSize
        {
            get => (double)GetValue(LabelFontSizeProperty);
            set => SetValue(LabelFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for individual label boxes.
        /// </summary>
        public FontStyle LabelFontStyle
        {
            get => (FontStyle)GetValue(LabelFontStyleProperty);
            set => SetValue(LabelFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for individual label boxes.
        /// </summary>
        public FontWeight LabelFontWeight
        {
            get => (FontWeight)GetValue(LabelFontWeightProperty);
            set => SetValue(LabelFontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for individual label boxes.
        /// </summary>
        public Thickness LabelMargin
        {
            get => (Thickness)GetValue(LabelMarginProperty);
            set => SetValue(LabelMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for individual label boxes.
        /// </summary>
        public Thickness LabelPadding
        {
            get => (Thickness)GetValue(LabelPaddingProperty);
            set => SetValue(LabelPaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets the width of individual label text boxes.
        /// </summary>
        /// <remarks><see cref="Double.NaN"/> is equivalent to "Auto" in XAML.</remarks>
        public double LabelWidth
        {
            get => (double)GetValue(LabelWidthProperty);
            set => SetValue(LabelWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="NotesLabel"/> objects to display in the control.
        /// </summary>
        public LabelCollection Labels
        {
            get => (LabelCollection)GetValue(LabelsProperty);
            set => SetValue(LabelsProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="NoteViewModel"/> that the labels are associated with.
        /// </summary>
        public NoteViewModel Note
        {
            get => (NoteViewModel)GetValue(NoteProperty);
            set => SetValue(NoteProperty, value);
        }

        #endregion Public properties
        #region Public events
        /// <summary>
        /// Occurs when a label is removed.
        /// </summary>
        public event RoutedEventHandler LabelRemoved
        {
            add => AddHandler(LabelRemovedEvent, value);
            remove => RemoveHandler(LabelRemovedEvent, value);
        }        
        
        /// <summary>
        /// Occurs when a label is updated.
        /// </summary>
        public event RoutedEventHandler LabelUpdated
        {
            add => AddHandler(LabelUpdatedEvent, value);
            remove => RemoveHandler(LabelUpdatedEvent, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        public LabelsDisplay()
        {
            InitializeComponent();

            Unloaded += OnUnloaded;
            EventAggregator?.SubscribeOnUIThread(this);
        }
    }
}
