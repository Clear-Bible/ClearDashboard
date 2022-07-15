using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetParallelCorpusByParallelCorpusIdQuery(ParallelCorpusId  ParallelCorpusId)
        : ProjectRequestQuery<
            (TokenizedCorpusId sourceTokenizedCorpusId, 
            TokenizedCorpusId targetTokenizedCorpusId, 
            IEnumerable<VerseMapping> verseMappings,
            ParallelCorpusId parallelCorpusId)>;
}
