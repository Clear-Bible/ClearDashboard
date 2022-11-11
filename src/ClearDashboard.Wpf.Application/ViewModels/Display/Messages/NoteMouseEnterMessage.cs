using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.Display.Messages
{
    public record NoteMouseEnterMessage(NoteViewModel Note, EntityIdCollection Entities);
}
