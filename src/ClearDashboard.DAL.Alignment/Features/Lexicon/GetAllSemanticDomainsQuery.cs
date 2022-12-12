using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record GetAllSemanticDomainsQuery() : LexiconRequestQuery<IEnumerable<SemanticDomain>>;
}
