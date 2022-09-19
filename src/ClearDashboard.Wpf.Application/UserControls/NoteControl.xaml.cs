using System.Collections;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.UserControls
{
    /// <summary>
    /// Interaction logic for NoteControl.xaml
    /// </summary>
    public partial class NoteControl : UserControl
    {
        /// <summary>
        /// Identifies the NoteApplied routed event.
        /// </summary>
        public static readonly RoutedEvent NoteAppliedEvent = EventManager.RegisterRoutedEvent
            ("NoteApplied", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteControl));

        /// <summary>
        /// Identifies the Noteancelled routed event.
        /// </summary>
        public static readonly RoutedEvent NoteCancelledEvent = EventManager.RegisterRoutedEvent
            ("NoteCancelled", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteControl));

        /// <summary>
        /// Identifies the TokenDisplayProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty TokenDisplayProperty = DependencyProperty.Register("TokenDisplay", typeof(TokenDisplay), typeof(NoteControl));

        /// <summary>
        /// Occurs when a note is applied.
        /// </summary>
        public event RoutedEventHandler NoteApplied
        {
            add => AddHandler(NoteAppliedEvent, value);
            remove => RemoveHandler(NoteAppliedEvent, value);
        }

        /// <summary>
        /// Occurs when a note is cancelled.
        /// </summary>
        public event RoutedEventHandler NoteCancelled
        {
            add => AddHandler(NoteCancelledEvent, value);
            remove => RemoveHandler(NoteCancelledEvent, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="TokenDisplay"/> token display information to display in this control.
        /// </summary>
        public TokenDisplay TokenDisplay
        {
            get => (TokenDisplay)GetValue(TokenDisplayProperty);
            set => SetValue(TokenDisplayProperty, value);
        }

        public NoteControl()
        {
            InitializeComponent();
        }

        private void ApplyNote(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new NoteEventArgs()
            {
                RoutedEvent = NoteAppliedEvent,
                TokenDisplay = TokenDisplay
            });
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs{RoutedEvent = NoteCancelledEvent});
        }
    }
}
