using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using System.Windows;

namespace ClearDashboard.Wpf.Application.Events.Notes
{
    public class NoteSeenEventArgs : RoutedEventArgs
    {
        public bool? Seen { get; set; }
        public NoteViewModel? NoteViewModel { get; set; }
    }
}
