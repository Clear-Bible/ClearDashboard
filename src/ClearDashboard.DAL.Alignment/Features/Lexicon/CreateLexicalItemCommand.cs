using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record CreateLexicalItemCommand(
        string Text,
        string? Language,
        string? Type) : ProjectRequestCommand<LexicalItemId>;
}
