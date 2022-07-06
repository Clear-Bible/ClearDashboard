using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record GetAllTokenizedCorpusIdsByCorpusIdQuery(CorpusId CorpusId) : ProjectRequestQuery<IEnumerable<TokenizedCorpusId>>;
}
