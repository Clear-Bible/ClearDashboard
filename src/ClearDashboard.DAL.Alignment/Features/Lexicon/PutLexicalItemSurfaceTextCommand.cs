using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record PutLexicalItemSurfaceTextCommand(
        LexicalItemId LexicalItemId,
        LexicalItemSurfaceText LexicalItemSurfaceText) : ProjectRequestCommand<LexicalItemSurfaceTextId>;
}
