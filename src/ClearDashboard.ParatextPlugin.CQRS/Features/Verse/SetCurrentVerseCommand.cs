using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Verse
{
    public record SetCurrentVerseCommand(string Verse) : IRequest<RequestResult<string>>
    {
        public string Verse { get; } = Verse;
    }
}
