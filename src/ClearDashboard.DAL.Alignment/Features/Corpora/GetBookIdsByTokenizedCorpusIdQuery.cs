using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetBookIdsByTokenizedCorpusIdQuery : ProjectRequestQuery<(IEnumerable<string> bookIds, TokenizedTextCorpusId tokenizedTextCorpusId, CorpusId corpusId)>
    {
        public GetBookIdsByTokenizedCorpusIdQuery(TokenizedTextCorpusId tokenizedTextCorpusId)
        {
            TokenizedTextCorpusId = tokenizedTextCorpusId;
        }
        public TokenizedTextCorpusId TokenizedTextCorpusId { get; }
    }
}
