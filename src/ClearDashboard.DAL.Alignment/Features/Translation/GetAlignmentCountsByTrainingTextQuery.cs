using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetAlignmentCountsByTrainingTextQuery(AlignmentSetId AlignmentSetId, bool SourceToTarget) : 
        ProjectRequestQuery<IDictionary<string, IDictionary<string, uint>>>;
}
