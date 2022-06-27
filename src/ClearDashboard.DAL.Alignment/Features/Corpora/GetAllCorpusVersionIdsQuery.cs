using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetAllCorpusVersionIdsQuery() : IRequest<RequestResult<IEnumerable<(CorpusVersionId corpusVersionId, CorpusId corpusId)>>>;
}
