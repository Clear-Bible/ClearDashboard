using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetAlignmentSetByAlignmentSetIdQuery(AlignmentSetId AlignmentSetId) : 
        ProjectRequestQuery<(AlignmentSetId alignmentSetId, ParallelCorpusId parallelCorpusId)>;
}
