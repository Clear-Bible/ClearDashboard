using System.Windows;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;

namespace ClearDashboard.Wpf.Application.Events.Notes
{
    public class LabelEventArgs : RoutedEventArgs
    {
        public Label Label { get; set; } = new();
        public LabelGroupViewModel? LabelGroup { get; set; }
        public NoteViewModel Note { get; set; } = new();
    }
}
