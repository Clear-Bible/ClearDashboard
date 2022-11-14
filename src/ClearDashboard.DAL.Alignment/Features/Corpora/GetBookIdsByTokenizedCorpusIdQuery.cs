using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetBookIdsByTokenizedCorpusIdQuery : ProjectRequestQuery<(IEnumerable<string> bookIds, TokenizedTextCorpusId tokenizedTextCorpusId, ScrVers versification)>
    {
        public GetBookIdsByTokenizedCorpusIdQuery(TokenizedTextCorpusId tokenizedTextCorpusId)
        {
            TokenizedTextCorpusId = tokenizedTextCorpusId;
        }
        public TokenizedTextCorpusId TokenizedTextCorpusId { get; }
    }
}
