using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetBookIdsByTokenizedCorpusIdQuery : IRequest<RequestResult<(IEnumerable<string> bookIds, CorpusId corpusId)>>
    {
        public GetBookIdsByTokenizedCorpusIdQuery(TokenizedCorpusId tokenizedCorpusId)
        {
            TokenizedCorpusId = tokenizedCorpusId;
        }
        public TokenizedCorpusId TokenizedCorpusId { get; }
    }
}
