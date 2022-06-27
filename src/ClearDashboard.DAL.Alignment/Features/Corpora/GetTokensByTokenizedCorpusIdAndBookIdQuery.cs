using ClearBible.Alignment.DataServices.Corpora;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.CQRS;

using MediatR;

namespace ClearBible.Alignment.DataServices.Features.Corpora
{
    public record GetTokensByTokenizedCorpusIdAndBookIdQuery : IRequest<RequestResult<IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>>
    {

        public GetTokensByTokenizedCorpusIdAndBookIdQuery(TokenizedCorpusId tokenizedCorpusId, string bookId)
        {
            TokenizedCorpusId = tokenizedCorpusId;
            BookId = bookId;
        }

        public TokenizedCorpusId TokenizedCorpusId { get; }
        public string BookId { get; }

    }
}
