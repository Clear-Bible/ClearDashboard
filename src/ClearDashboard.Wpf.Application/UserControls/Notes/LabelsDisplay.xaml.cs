using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using NotesLabel = ClearDashboard.DAL.Alignment.Notes.Label;

namespace ClearDashboard.Wpf.Application.UserControls.Notes
{
    /// <summary>
    /// A control for displaying a collection of <see cref="NotesLabel"/> values.
    /// </summary>
    public partial class LabelsDisplay
    {
        #region Static Routed Events
        /// <summary>
        /// Identifies the LabelAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelRemovedEvent = EventManager.RegisterRoutedEvent
            ("LabelRemoved", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsDisplay));
        #endregion
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the Note dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(NoteViewModel), typeof(LabelsDisplay));

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(LabelsDisplay));

        /// <summary>
        /// Identifies the LabelBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelBackgroundProperty = DependencyProperty.Register("LabelBackground", typeof(SolidColorBrush), typeof(LabelsDisplay));

        /// <summary>
        /// Identifies the LabelCornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelCornerRadiusProperty = DependencyProperty.Register(nameof(LabelCornerRadius), typeof(CornerRadius), typeof(LabelsDisplay),
            new PropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// Identifies the LabelMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelMarginProperty = DependencyProperty.Register(nameof(LabelMargin), typeof(Thickness), typeof(LabelsDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the LabelPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelPaddingProperty = DependencyProperty.Register(nameof(LabelPadding), typeof(Thickness), typeof(LabelsDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the Labels dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelsProperty = DependencyProperty.Register("Labels", typeof(IEnumerable), typeof(LabelsDisplay));

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
            var label = control?.DataContext as NotesLabel;

            if (label != null)
            {
                RaiseLabelEvent(LabelRemovedEvent, label);
            }
        }

        #endregion
        #region Public properties

        /// <summary>
        /// Gets or sets the <see cref="NoteViewModel"/> that the labels are associated with.
        /// </summary>
        public NoteViewModel Note
        {
            get => (NoteViewModel) GetValue(NoteProperty);
            set => SetValue(NoteProperty, value);
        }

        /// <summary>
        /// Gets or sets the orientation for displaying the labels.
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
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
        /// Gets or sets a collection of <see cref="System.Windows.Controls.Label"/> objects to display in the control.
        /// </summary>
        public IEnumerable Labels
        {
            get => (IEnumerable)GetValue(LabelsProperty);
            set => SetValue(LabelsProperty, value);
        }
        #endregion Public properties
        #region Public events
        /// <summary>
        /// Occurs when an new label is removed.
        /// </summary>
        public event RoutedEventHandler LabelRemoved
        {
            add => AddHandler(LabelRemovedEvent, value);
            remove => RemoveHandler(LabelRemovedEvent, value);
        }
        #endregion

        public LabelsDisplay()
        {
            InitializeComponent();
        }

    }
}
