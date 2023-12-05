using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public record FindTokensBySurfaceTextQuery(
        TokenizedTextCorpusId TokenizedTextCorpusId,
        string SearchString,
        WordPart WordPart,
        bool IgnoreCase = true) : ProjectRequestQuery<IEnumerable<Token>>;
}
