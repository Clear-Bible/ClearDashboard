
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public class UpdateLabelsWithExternalLabelsCommandParam
    {
        public List<ExternalLabel> ExternalLabels { get; set; } = new List<ExternalLabel>();
    }
}
