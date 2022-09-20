using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record CreateParallelCorpusCommand(
        TokenizedTextCorpusId SourceTokenizedCorpusId,
        TokenizedTextCorpusId TargetTokenizedCorpusId,
        IEnumerable<VerseMapping> VerseMappings,
        string? DisplayName) : ProjectRequestCommand<ParallelCorpus>;
}
