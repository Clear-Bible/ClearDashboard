using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetTokensByTokenizedCorpusIdAndBookIdQuery : ProjectRequestQuery<IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>
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
