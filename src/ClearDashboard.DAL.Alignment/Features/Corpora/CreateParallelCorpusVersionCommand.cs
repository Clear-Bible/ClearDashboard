using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record CreateParallelCorpusVersionCommand(
        ParallelCorpusId parallelCorpusId,
        EngineParallelTextCorpus engineParallelTextCorpus) 
        : IRequest<RequestResult<ParallelCorpusVersionId>>;
}
