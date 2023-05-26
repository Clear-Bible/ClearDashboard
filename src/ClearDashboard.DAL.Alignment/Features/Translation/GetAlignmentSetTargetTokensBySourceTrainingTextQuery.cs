using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetAlignmentSetTargetTokensBySourceTrainingTextQuery(
        AlignmentSetId AlignmentSetId, 
        string SourceTrainingText, 
        AlignmentOriginationFilterMode AlignmentOriginationFilterMode) : 
        ProjectRequestQuery<IEnumerable<Token>>;
}
