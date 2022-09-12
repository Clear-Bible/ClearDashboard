using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record GetLabelNoteAssociationsByLabelIdOrNoteIdQuery(LabelId? LabelId, NoteId? NoteId) : 
        ProjectRequestQuery<IEnumerable<LabelNoteAssociation>>;
}
