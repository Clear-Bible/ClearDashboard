using MediatR;

using ClearBible.Alignment.DataServices.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearBible.Engine.Corpora;

namespace ClearBible.Alignment.DataServices.Features.Corpora
{
    public record CreateParallelCorpusVersionCommand(
        ParallelCorpusId parallelCorpusId,
        EngineParallelTextCorpus engineParallelTextCorpus) 
        : IRequest<RequestResult<ParallelCorpusVersionId>>;
}
