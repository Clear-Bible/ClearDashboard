using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.Display.Messages
{
    public record NoteAddedMessage(NoteViewModel Note, EntityIdCollection Entities);
}
