using MediatR;

using ClearBible.Alignment.DataServices.Corpora;
using ClearDashboard.DAL.CQRS;

namespace ClearBible.Alignment.DataServices.Features.Corpora
{
    public record GetAllCorpusVersionIdsQuery() : IRequest<RequestResult<IEnumerable<(CorpusVersionId corpusVersionId, CorpusId corpusId)>>>;
}
