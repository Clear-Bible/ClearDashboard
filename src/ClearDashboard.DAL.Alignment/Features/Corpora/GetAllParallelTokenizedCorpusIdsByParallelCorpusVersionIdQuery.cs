using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQuery(ParallelCorpusVersionId ParallelCorpusVersionId) 
        : IRequest<RequestResult<IEnumerable<ParallelTokenizedCorpusId>>>;
}
