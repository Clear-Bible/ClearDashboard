using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record CreateOrUpdateNoteCommand(
        NoteId? NoteId,
        string Text,
        string? AbbreviatedText) : ProjectRequestCommand<NoteId>;
}
