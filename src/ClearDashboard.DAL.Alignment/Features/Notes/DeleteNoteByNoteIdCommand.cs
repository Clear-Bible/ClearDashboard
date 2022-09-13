using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record DeleteNoteByNoteIdCommand(
        NoteId NoteId) : ProjectRequestCommand<object>;
}
