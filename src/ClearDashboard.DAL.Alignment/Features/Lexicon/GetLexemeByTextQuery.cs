using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record GetLexemeByTextQuery(
        string Lemma,
        string? Language,
        string? SenseLanguage) : ProjectRequestQuery<Lexeme?>;
}
