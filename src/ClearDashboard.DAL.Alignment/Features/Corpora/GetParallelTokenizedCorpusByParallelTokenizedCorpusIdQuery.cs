using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetParallelTokenizedCorpusByParallelTokenizedCorpusIdQuery(ParallelTokenizedCorpusId  ParallelTokenizedCorpus)
        : ProjectRequestQuery<(TokenizedCorpusId sourceTokenizedCorpusId, TokenizedCorpusId targetTokenizedCorpusId, IEnumerable<EngineVerseMapping> engineVerseMappings, ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>;
}
