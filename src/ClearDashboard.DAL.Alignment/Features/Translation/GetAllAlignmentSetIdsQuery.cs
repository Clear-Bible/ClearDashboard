using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetAllAlignmentSetIdsQuery(ParallelCorpusId? ParallelCorpusId, UserId? UserId) : 
        ProjectRequestQuery<IEnumerable<(AlignmentSetId alignmentSetId, ParallelCorpusId parallelCorpusId, UserId userId)>>;
}
