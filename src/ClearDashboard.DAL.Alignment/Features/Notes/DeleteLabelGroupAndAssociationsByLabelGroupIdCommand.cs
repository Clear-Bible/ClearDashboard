using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record DeleteLabelGroupAndAssociationsByLabelGroupIdCommand(
        LabelGroupId LabelGroupId) : ProjectRequestCommand<Unit>;
}
