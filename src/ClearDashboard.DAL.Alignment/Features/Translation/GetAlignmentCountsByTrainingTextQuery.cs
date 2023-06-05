using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetAlignmentCountsByTrainingTextQuery(
        AlignmentSetId AlignmentSetId, 
        bool SourceToTarget,
        AlignmentTypes AlignmentTypesToInclude) : 
        ProjectRequestQuery<IDictionary<string, IDictionary<string, IDictionary<string, uint>>>>;
}
