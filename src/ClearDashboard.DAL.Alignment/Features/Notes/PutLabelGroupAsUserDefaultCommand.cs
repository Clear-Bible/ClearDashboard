using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record PutLabelGroupAsUserDefaultCommand(LabelGroupId? LabelGroupId, UserId UserId) : ProjectRequestCommand<Unit>;
}
