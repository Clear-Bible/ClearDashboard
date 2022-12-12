using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record GetLexicalItemByTextQuery(
        string TrainingText,
        string Language,
        string? DefinitionLanguage) : LexiconRequestQuery<LexicalItem?>;
}
