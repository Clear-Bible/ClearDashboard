using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record GetLexemesByLemmaOrFormQuery(
        string LemmaOrForm,
        string? Language,
        string? MeaningLanguage) : ProjectRequestQuery<IEnumerable<Lexeme>>;
}
