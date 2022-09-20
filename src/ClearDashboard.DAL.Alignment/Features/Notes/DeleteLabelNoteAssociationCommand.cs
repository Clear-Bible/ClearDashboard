using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record DeleteLabelNoteAssociationCommand(
        LabelId LabelId, NoteId NoteId) : ProjectRequestCommand<object>;
}
