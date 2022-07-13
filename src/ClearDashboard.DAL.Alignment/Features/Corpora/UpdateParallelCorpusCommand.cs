using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record UpdateParallelCorpusCommand(
        IEnumerable<VerseMapping> engineVerseMappingList,
        ParallelCorpusId parallelCorpusId) : ProjectRequestCommand<ParallelCorpus>;
}
