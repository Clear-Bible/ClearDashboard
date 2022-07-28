using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetTokensByTokenizedCorpusIdAndBookIdQuery : ProjectRequestQuery<IEnumerable<VerseTokens>>
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
