using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetParallelCorpusByParallelCorpusIdQuery(ParallelCorpusId  ParallelCorpusId)
        : ProjectRequestQuery<
            (TokenizedTextCorpusId sourceTokenizedCorpusId, 
            TokenizedTextCorpusId targetTokenizedCorpusId,
            ParallelCorpusId parallelCorpusId)>;
}
