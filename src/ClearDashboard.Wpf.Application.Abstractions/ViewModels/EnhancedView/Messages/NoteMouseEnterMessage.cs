using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages
{
    public record NoteMouseEnterMessage(NoteViewModel Note, EntityIdCollection Entities, bool NewNote=false);
}
