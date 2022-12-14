using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record GetLexicalItemByTextQuery(
        string Lemma,
        string? Language,
        string? DefinitionLanguage) : ProjectRequestQuery<LexicalItem?>;
}
