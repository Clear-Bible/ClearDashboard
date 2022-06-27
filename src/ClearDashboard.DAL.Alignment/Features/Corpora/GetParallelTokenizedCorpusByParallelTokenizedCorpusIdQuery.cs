using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetParallelTokenizedCorpusByParallelTokenizedCorpusIdQuery(ParallelTokenizedCorpusId  ParallelTokenizedCorpus)
        : IRequest<RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId, TokenizedCorpusId targetTokenizedCorpusId, IEnumerable<EngineVerseMapping> engineVerseMappings, ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>>;
}
