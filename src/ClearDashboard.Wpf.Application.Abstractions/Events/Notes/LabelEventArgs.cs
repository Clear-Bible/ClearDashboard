using System.Windows;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Events.Notes
{
    public class LabelEventArgs : RoutedEventArgs
    {
        public Label Label { get; set; } = new();
        public NoteViewModel Note { get; set; } = new();
    }
}
