using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetAlignmentCountsByTrainingOrSurfaceTextQuery(
        AlignmentSetId AlignmentSetId, 
        bool SourceToTarget,
        bool totalsByTraining,
        bool includeBookNumbers,
        AlignmentTypes AlignmentTypesToInclude) : 
        ProjectRequestQuery<IDictionary<string, IDictionary<string, (IDictionary<string, uint> StatusCounts, string BookNumbers)>>>;
}
