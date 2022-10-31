using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record GetDomainEntityContextsQuery(IEnumerable<IId> EntityIds) : 
        ProjectRequestQuery<Dictionary<IId, Dictionary<string, string>>>;
}
