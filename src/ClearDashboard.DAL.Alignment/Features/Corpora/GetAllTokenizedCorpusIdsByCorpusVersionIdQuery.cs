using MediatR;

using ClearBible.Alignment.DataServices.Corpora;
using ClearDashboard.DAL.CQRS;

namespace ClearBible.Alignment.DataServices.Features.Corpora
{
    public record GetAllTokenizedCorpusIdsByCorpusVersionIdQuery(CorpusVersionId CorpusVersionId) : IRequest<RequestResult<IEnumerable<TokenizedCorpusId>>>;
}
