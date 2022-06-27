using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetAllTokenizedCorpusIdsByCorpusVersionIdQuery(CorpusVersionId CorpusVersionId) : IRequest<RequestResult<IEnumerable<TokenizedCorpusId>>>;
}
