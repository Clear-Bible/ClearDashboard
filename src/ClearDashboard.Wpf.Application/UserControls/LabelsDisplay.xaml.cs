using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// A control for displaying a collection of <see cref="Label"/> values.
    /// </summary>
    public partial class LabelsDisplay : UserControl
    {
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(LabelsDisplay));

        /// <summary>
        /// Identifies the LabelMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelMarginProperty = DependencyProperty.Register("LabelMargin", typeof(Thickness), typeof(LabelsDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the LabelPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelPaddingProperty = DependencyProperty.Register("LabelPadding", typeof(Thickness), typeof(LabelsDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the LabelCornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelCornerRadiusProperty = DependencyProperty.Register("LabelCornerRadius", typeof(CornerRadius), typeof(LabelsDisplay),
            new PropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// Identifies the LabelBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelBackgroundProperty = DependencyProperty.Register("LabelBackground", typeof(SolidColorBrush), typeof(LabelsDisplay)
            );

        /// <summary>
        /// Identifies the Labels dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelsProperty = DependencyProperty.Register("Labels", typeof(IEnumerable), typeof(LabelsDisplay));

        #endregion Static DependencyProperties

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
        /// Gets or sets a collection of <see cref="Label"/> objects to display in the control.
        /// </summary>
        public IEnumerable Labels
        {
            get => (IEnumerable)GetValue(LabelsProperty);
            set => SetValue(LabelsProperty, value);
        }

        public LabelsDisplay()
        {
            InitializeComponent();
        }
    }
}
