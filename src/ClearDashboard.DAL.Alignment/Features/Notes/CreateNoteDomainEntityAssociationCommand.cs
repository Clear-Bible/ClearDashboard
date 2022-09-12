using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record CreateNoteDomainEntityAssociationCommand(
        NoteId NoteId,
        object DomainEntityId,
        object? DomainSubEntityId) : ProjectRequestCommand<NoteDomainEntityAssociationId>;
}
