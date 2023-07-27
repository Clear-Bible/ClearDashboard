using System.Windows;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.UserControls.Notes
{
    /// <summary>
    /// A control for displaying the entity associations of a <see cref="ClearDashboard.DAL.Alignment.Notes.Note"/>.
    /// </summary>
    public partial class NoteAssociationsDisplay
    {
        #region Static Routed Events
        
        /// <summary>
        /// Identifies the NoteAssociationClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAssociationClickedEvent = EventManager.RegisterRoutedEvent
            ("NoteAssociationClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteAssociationsDisplay));

        /// <summary>
        /// Identifies the NoteAssociationDoubleClickedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAssociationDoubleClickedEvent = EventManager.RegisterRoutedEvent
            ("NoteAssociationDoubleClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteAssociationsDisplay));

        /// <summary>
        /// Identifies the NoteAssociationLeftButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAssociationLeftButtonDownEvent = EventManager.RegisterRoutedEvent
            ("NoteAssociationLeftButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteAssociationsDisplay));

        /// <summary>
        /// Identifies the NoteAssociationLeftButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAssociationLeftButtonUpEvent = EventManager.RegisterRoutedEvent
            ("NoteAssociationLeftButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteAssociationsDisplay));

        /// <summary>
        /// Identifies the NoteAssociationRightButtonDownEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAssociationRightButtonDownEvent = EventManager.RegisterRoutedEvent
            ("NoteAssociationRightButtonDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteAssociationsDisplay));

        /// <summary>
        /// Identifies the NoteAssociationRightButtonUpEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAssociationRightButtonUpEvent = EventManager.RegisterRoutedEvent
            ("NoteAssociationRightButtonUp", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteAssociationsDisplay));

        /// <summary>
        /// Identifies the NoteAssociationMouseEnterEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAssociationMouseEnterEvent = EventManager.RegisterRoutedEvent
            ("NoteAssociationMouseEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteAssociationsDisplay));

        /// <summary>
        /// Identifies the NoteAssociationMouseLeaveEvent routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAssociationMouseLeaveEvent = EventManager.RegisterRoutedEvent
            ("NoteAssociationMouseLeave", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteAssociationsDisplay));

        #endregion
        #region Static DependencyProperties

        /// <summary>
        /// Identifies the Note dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(NoteViewModel), typeof(NoteAssociationsDisplay));

        /// <summary>
        /// Identifies the InnerMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerMarginProperty = DependencyProperty.Register(nameof(InnerMargin), typeof(Thickness), typeof(NoteAssociationsDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the InnerPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerPaddingProperty = DependencyProperty.Register(nameof(InnerPadding), typeof(Thickness), typeof(NoteAssociationsDisplay),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        #endregion Static DependencyProperties
        #region Private event handlers

        private void RaiseNoteAssociationEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        {
            var control = e.Source as FrameworkElement;
            var association = control?.DataContext as NoteAssociationViewModel;
            RaiseEvent(new NoteAssociationEventArgs
            {
                RoutedEvent = routedEvent,
                Note = Note,
                AssociatedEntityId = association.AssociatedEntityId
            });
        }

        private void OnNoteAssociationClicked(object sender, RoutedEventArgs e)
        {
            RaiseNoteAssociationEvent(NoteAssociationClickedEvent, e);
        }

        private void OnNoteAssociationDoubleClicked(object sender, RoutedEventArgs e)
        {
            RaiseNoteAssociationEvent(NoteAssociationDoubleClickedEvent, e);
        }

        private void OnNoteAssociationLeftButtonDown(object sender, RoutedEventArgs e)
        {
            RaiseNoteAssociationEvent(NoteAssociationLeftButtonDownEvent, e);
        }

        private void OnNoteAssociationLeftButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseNoteAssociationEvent(NoteAssociationRightButtonUpEvent, e);
        }
        private void OnNoteAssociationRightButtonDown(object sender, RoutedEventArgs e)
        {
            RaiseNoteAssociationEvent(NoteAssociationRightButtonDownEvent, e);
        }

        private void OnNoteAssociationRightButtonUp(object sender, RoutedEventArgs e)
        {
            RaiseNoteAssociationEvent(NoteAssociationRightButtonUpEvent, e);
        }

        private void OnNoteAssociationMouseEnter(object sender, RoutedEventArgs e)
        {
            RaiseNoteAssociationEvent(NoteAssociationMouseEnterEvent, e);
        }

        private void OnNoteAssociationMouseLeave(object sender, RoutedEventArgs e)
        {
            RaiseNoteAssociationEvent(NoteAssociationMouseLeaveEvent, e);
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
        /// Gets or sets the margin for individual associations.
        /// </summary>
        public Thickness InnerMargin
        {
            get => (Thickness)GetValue(InnerMarginProperty);
            set => SetValue(InnerMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for individual associations.
        /// </summary>
        public Thickness InnerPadding
        {
            get => (Thickness)GetValue(InnerPaddingProperty);
            set => SetValue(InnerPaddingProperty, value);
        }

        #endregion Public properties
        #region Public events

        /// <summary>
        /// Occurs when an individual note association is clicked.
        /// </summary>
        public event RoutedEventHandler NoteAssociationClicked
        {
            add => AddHandler(NoteAssociationClickedEvent, value);
            remove => RemoveHandler(NoteAssociationClickedEvent, value);
        }

        /// <summary>
        /// Occurs when an individual note association is clicked two or more times.
        /// </summary>
        public event RoutedEventHandler NoteAssociationDoubleClicked
        {
            add => AddHandler(NoteAssociationDoubleClickedEvent, value);
            remove => RemoveHandler(NoteAssociationDoubleClickedEvent, value);
        }

        /// <summary>
        /// Occurs when the left mouse button is pressed while the mouse pointer is over a note association.
        /// </summary>
        public event RoutedEventHandler NoteAssociationLeftButtonDown
        {
            add => AddHandler(NoteAssociationLeftButtonDownEvent, value);
            remove => RemoveHandler(NoteAssociationLeftButtonDownEvent, value);
        }

        /// <summary>
        /// Occurs when the left mouse button is released while the mouse pointer is over a note association.
        /// </summary>
        public event RoutedEventHandler NoteAssociationLeftButtonUp
        {
            add => AddHandler(NoteAssociationLeftButtonUpEvent, value);
            remove => RemoveHandler(NoteAssociationLeftButtonUpEvent, value);
        }

        /// <summary>
        /// Occurs when the right mouse button is pressed while the mouse pointer is over a note association.
        /// </summary>
        public event RoutedEventHandler NoteAssociationRightButtonDown
        {
            add => AddHandler(NoteAssociationRightButtonDownEvent, value);
            remove => RemoveHandler(NoteAssociationRightButtonDownEvent, value);
        }

        /// <summary>
        /// Occurs when the right mouse button is released while the mouse pointer is over a note association.
        /// </summary>
        public event RoutedEventHandler NoteAssociationRightButtonUp
        {
            add => AddHandler(NoteAssociationRightButtonUpEvent, value);
            remove => RemoveHandler(NoteAssociationRightButtonUpEvent, value);
        }

        /// <summary>
        /// Occurs when the mouse pointer enters the bounds of a note association.
        /// </summary>
        public event RoutedEventHandler NoteAssociationMouseEnter
        {
            add => AddHandler(NoteAssociationMouseEnterEvent, value);
            remove => RemoveHandler(NoteAssociationMouseEnterEvent, value);
        }

        /// <summary>
        /// Occurs when the mouse pointer leaves the bounds of a note association.
        /// </summary>
        public event RoutedEventHandler NoteAssociationMouseLeave
        {
            add => AddHandler(NoteAssociationMouseLeaveEvent, value);
            remove => RemoveHandler(NoteAssociationMouseLeaveEvent, value);
        }

        #endregion

        public NoteAssociationsDisplay()
        {
            InitializeComponent();
        }
    }
}
