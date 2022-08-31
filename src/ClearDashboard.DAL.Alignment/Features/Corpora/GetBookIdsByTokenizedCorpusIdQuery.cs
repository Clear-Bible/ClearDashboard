using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetBookIdsByTokenizedCorpusIdQuery : ProjectRequestQuery<(IEnumerable<string> bookIds, CorpusId corpusId)>
    {
        public GetBookIdsByTokenizedCorpusIdQuery(TokenizedTextCorpusId tokenizedCorpusId)
        {
            TokenizedCorpusId = tokenizedCorpusId;
        }
        public TokenizedTextCorpusId TokenizedCorpusId { get; }
    }
}
