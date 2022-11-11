using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.Display.Messages
{
    public record NoteMouseLeaveMessage(NoteViewModel Note, EntityIdCollection Entities);
}
