using ClearBible.Engine.Utils;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record SaveLexiconCommand(
        Alignment.Lexicon.Lexicon Lexicon) : ProjectRequestCommand<IEnumerable<IId>>;
}
