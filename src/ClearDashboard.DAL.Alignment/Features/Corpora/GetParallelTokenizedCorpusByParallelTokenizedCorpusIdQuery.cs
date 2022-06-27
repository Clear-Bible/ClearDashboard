using MediatR;

using ClearBible.Alignment.DataServices.Corpora;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.CQRS;

namespace ClearBible.Alignment.DataServices.Features.Corpora
{
    public record GetParallelTokenizedCorpusByParallelTokenizedCorpusIdQuery(ParallelTokenizedCorpusId  ParallelTokenizedCorpus)
        : IRequest<RequestResult<
            (TokenizedCorpusId sourceTokenizedCorpusId, 
            TokenizedCorpusId targetTokenizedCorpusId,
            IEnumerable<EngineVerseMapping> engineVerseMappings,
            ParallelCorpusVersionId parallelCorpusVersionId,
            ParallelCorpusId parallelCorpusId)>>;
}
