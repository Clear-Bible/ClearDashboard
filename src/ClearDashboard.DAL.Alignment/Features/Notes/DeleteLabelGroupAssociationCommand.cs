using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record DeleteLabelGroupAssociationCommand(
        LabelId LabelId, LabelGroupId LabelGroupId) : ProjectRequestCommand<Unit>;
}
