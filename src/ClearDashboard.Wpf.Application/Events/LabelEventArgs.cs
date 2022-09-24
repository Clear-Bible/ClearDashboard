using System.Windows;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;

namespace ClearDashboard.Wpf.Application.Events
{
    public class LabelEventArgs : RoutedEventArgs
    {
        public IId? EntityId { get; set; }
        public Label Label { get; set; }
        public Note Note { get; set; }
    }
}
