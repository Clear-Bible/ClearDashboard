using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record CreateOrUpdateNoteCommand(
        NoteId? NoteId,
        string Text,
        string? AbbreviatedText,
        Models.NoteStatus NoteStatus,
        EntityId<NoteId>? ThreadId,
        ICollection<Guid> SeenByUserIds) : ProjectRequestCommand<NoteId>;
}
