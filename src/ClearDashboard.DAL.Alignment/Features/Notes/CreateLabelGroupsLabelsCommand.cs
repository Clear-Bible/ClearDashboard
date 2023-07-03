using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record CreateLabelGroupsLabelsCommand(IDictionary<string, IEnumerable<(string Text, string? TemplateText)>>? LabelGroupNamesLabels) : ProjectRequestCommand<Unit>;
}
