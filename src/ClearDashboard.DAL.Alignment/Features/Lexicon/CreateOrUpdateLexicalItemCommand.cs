using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record CreateOrUpdateLexicalItemCommand(
        LexicalItemId? LexicalItemId,
        string Lemma,
        string? Language) : ProjectRequestCommand<LexicalItemId>;
}
