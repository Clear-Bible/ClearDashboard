using System.Windows;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.Events
{
    public class LabelEventArgs : RoutedEventArgs
    {
        public EntityIdCollection EntityIds { get; set; }
        public IId? EntityId { get; set; }
        public Label Label { get; set; }
        public Note Note { get; set; }
    }
}
