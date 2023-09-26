using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record CreateOrUpdateLexemeCommand(
        LexemeId LexemeId,
        string Lemma,
        string? Language,
        string? Type) : ProjectRequestCommand<LexemeId>;
}
