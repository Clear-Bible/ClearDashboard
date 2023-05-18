using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record GetLabelGroupDefaultForUserQuery(UserId UserId) : ProjectRequestQuery<LabelGroupId?>;
}
