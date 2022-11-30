using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages
{
    public record NoteAddedMessage(NoteViewModel Note, EntityIdCollection Entities);
}
