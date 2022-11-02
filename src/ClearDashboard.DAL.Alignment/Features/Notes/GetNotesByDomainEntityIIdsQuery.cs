using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record GetNotesByDomainEntityIIdsQuery(IEnumerable<IId>? DomainEntityIIds) : 
        ProjectRequestQuery<IEnumerable<Note>>;
}
