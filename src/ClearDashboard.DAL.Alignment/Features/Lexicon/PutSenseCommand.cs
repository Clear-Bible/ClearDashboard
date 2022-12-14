using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record PutSenseCommand(
        LexemeId LexemeId,
        Sense Sense) : ProjectRequestCommand<SenseId>;
}
