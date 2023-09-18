using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetBookNumbersByTrainingOrSurfaceTextQuery(
        AlignmentSetId AlignmentSetId,
        string SourceString, 
        string TargetString, 
        bool StringsAreTraining,
        AlignmentTypes AlignmentTypesToInclude) : 
        ProjectRequestQuery<IEnumerable<int>>;
}
