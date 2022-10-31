using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record GetDomainEntityContextsByIIdsQuery(IEnumerable<IId> DomainEntityIIds) : 
        ProjectRequestQuery<Dictionary<IId, Dictionary<string, string>>>;
}
