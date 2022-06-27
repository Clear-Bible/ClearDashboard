using ClearBible.Alignment.DataServices.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;
using SIL.Scripture;

namespace ClearBible.Alignment.DataServices.Features.Corpora
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
