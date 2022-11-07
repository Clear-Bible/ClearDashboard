using System.Windows;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.Events
{
    public class NoteAssociationEventArgs : RoutedEventArgs
    {
        public NoteViewModel Note { get; set; } = new();
        public IId AssociatedEntityId { get; set; } = new EmptyEntityId();
    }
}
