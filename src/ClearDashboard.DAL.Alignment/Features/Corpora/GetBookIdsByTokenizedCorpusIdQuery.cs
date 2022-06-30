using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetBookIdsByTokenizedCorpusIdQuery(string ProjectName) : ProjectRequestQuery<(IEnumerable<string> bookIds, CorpusId corpusId)>(ProjectName)
    {
        public GetBookIdsByTokenizedCorpusIdQuery(string ProjectName, TokenizedCorpusId tokenizedCorpusId): this(ProjectName)
        {
            TokenizedCorpusId = tokenizedCorpusId;
        }
        public TokenizedCorpusId TokenizedCorpusId { get; }
    }
}
