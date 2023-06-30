using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record GetLabelsByPartialTextQuery(string? PartialText, LabelGroupId? LabelGroupId) : ProjectRequestQuery<IEnumerable<Label>>;
}
