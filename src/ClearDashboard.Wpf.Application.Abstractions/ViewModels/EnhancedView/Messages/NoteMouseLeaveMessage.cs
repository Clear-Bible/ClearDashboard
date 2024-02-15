using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages
{
    public record NoteMouseLeaveMessage(NoteViewModel Note, EntityIdCollection Entities, bool IsNewNote = false);
}
