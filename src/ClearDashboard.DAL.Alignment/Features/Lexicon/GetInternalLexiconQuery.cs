using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record GetInternalLexiconQuery() : ProjectRequestQuery<Alignment.Lexicon.Lexicon>;
}
