using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Events.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.UserControls.Lexicon
{
    /// <summary>
    /// A control for displaying a collection of <see cref="SemanticDomain"/> values.
    /// </summary>
    public partial class SemanticDomainDisplay
    {
        #region Static Routed Events
        /// <summary>
        /// Identifies the SemanticDomainRemoved routed event.
        /// </summary>
        public static readonly RoutedEvent SemanticDomainRemovedEvent = EventManager.RegisterRoutedEvent
            ("SemanticDomainRemoved", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SemanticDomainDisplay));
        #endregion
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the Meaning dependency property.
        /// </summary>
        public static readonly DependencyProperty MeaningProperty = DependencyProperty.Register(nameof(Meaning), typeof(MeaningViewModel), typeof(SemanticDomainDisplay));

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(SemanticDomainDisplay));

        /// <summary>
        /// Identifies the SemanticDomainBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainBackgroundProperty = DependencyProperty.Register(nameof(SemanticDomainBackground), typeof(SolidColorBrush), typeof(SemanticDomainDisplay));

        /// <summary>
        /// Identifies the SemanticDomainCornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainCornerRadiusProperty = DependencyProperty.Register(nameof(SemanticDomainCornerRadius), typeof(CornerRadius), typeof(SemanticDomainDisplay),
            new PropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// Identifies the SemanticDomainMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainMarginProperty = DependencyProperty.Register(nameof(SemanticDomainMargin), typeof(Thickness), typeof(SemanticDomainDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the SemanticDomainPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainPaddingProperty = DependencyProperty.Register(nameof(SemanticDomainPadding), typeof(Thickness), typeof(SemanticDomainDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the SemanticDomains dependency property.
        /// </summary>
        public static readonly DependencyProperty SemanticDomainsProperty = DependencyProperty.Register(nameof(SemanticDomains), typeof(SemanticDomainCollection), typeof(SemanticDomainDisplay));

        #endregion Static DependencyProperties
        #region Private event handlers

        private void RaiseSemanticDomainEvent(RoutedEvent routedEvent, SemanticDomain semanticDomain)
        {
            RaiseEvent(new SemanticDomainEventArgs()
            {
                RoutedEvent = routedEvent,
                Meaning = Meaning,
                SemanticDomain = semanticDomain
            });
        }

        private void OnRemoveSemanticDomain(object sender, RoutedEventArgs e)
        {
            var control = e.Source as FrameworkElement;
            var semanticDomain = control?.DataContext as SemanticDomain;

            if (semanticDomain != null)
            {
                RaiseSemanticDomainEvent(SemanticDomainRemovedEvent, semanticDomain);
            }
        }

        #endregion
        #region Public properties

        /// <summary>
        /// Gets or sets the <see cref="Meaning"/> that the semantic domains are associated with.
        /// </summary>
        public MeaningViewModel Meaning
        {
            get => (MeaningViewModel) GetValue(MeaningProperty);
            set => SetValue(MeaningProperty, value);
        }

        /// <summary>
        /// Gets or sets the orientation for displaying the semantic domains.
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for individual semantic domain boxes.
        /// </summary>
        public Thickness SemanticDomainMargin
        {
            get => (Thickness)GetValue(SemanticDomainMarginProperty);
            set => SetValue(SemanticDomainMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for individual semantic domain boxes.
        /// </summary>
        public Thickness SemanticDomainPadding
        {
            get => (Thickness)GetValue(SemanticDomainPaddingProperty);
            set => SetValue(SemanticDomainPaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush for individual semantic domain boxes.
        /// </summary>
        public SolidColorBrush SemanticDomainBackground
        {
            get => (SolidColorBrush)GetValue(SemanticDomainBackgroundProperty);
            set => SetValue(SemanticDomainBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the corner radius for individual semantic domain boxes.
        /// </summary>
        public CornerRadius SemanticDomainCornerRadius
        {
            get => (CornerRadius)GetValue(SemanticDomainCornerRadiusProperty);
            set => SetValue(SemanticDomainCornerRadiusProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="SemanticDomain"/> objects to display in the control.
        /// </summary>
        public SemanticDomainCollection SemanticDomains
        {
            get => (SemanticDomainCollection)GetValue(SemanticDomainsProperty);
            set => SetValue(SemanticDomainsProperty, value);
        }
        #endregion Public properties
        #region Public events
        /// <summary>
        /// Occurs when an new label is removed.
        /// </summary>
        public event RoutedEventHandler SemanticDomainRemoved
        {
            add => AddHandler(SemanticDomainRemovedEvent, value);
            remove => RemoveHandler(SemanticDomainRemovedEvent, value);
        }
        #endregion

        public SemanticDomainDisplay()
        {
            InitializeComponent();
        }
    }
}
