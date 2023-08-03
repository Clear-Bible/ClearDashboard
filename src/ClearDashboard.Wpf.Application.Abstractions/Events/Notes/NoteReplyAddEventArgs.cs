using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using System.Windows;

namespace ClearDashboard.Wpf.Application.Events.Notes
{
    public class NoteReplyAddEventArgs : RoutedEventArgs
    {
        public string? Text { get; set; }
        public NoteViewModel? NoteViewModelWithReplies { get; set; }
    }
}
