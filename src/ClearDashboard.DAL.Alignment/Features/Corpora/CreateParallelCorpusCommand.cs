using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record CreateParallelCorpusCommand(
        TokenizedCorpusId SourceTokenizedCorpusId,
        TokenizedCorpusId TargetTokenizedCorpusId,
        IEnumerable<VerseMapping> engineVerseMappingList) : ProjectRequestCommand<ParallelCorpus>;
}
