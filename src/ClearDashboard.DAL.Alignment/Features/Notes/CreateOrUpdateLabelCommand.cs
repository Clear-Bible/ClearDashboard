using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record CreateOrUpdateLabelCommand(
        LabelId? LabelId,
        string Text,
        string? TemplateText) : ProjectRequestCommand<LabelId>;
}
