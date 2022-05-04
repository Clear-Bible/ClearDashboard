using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Verse
{
    public record GetCurrentVerseCommand() : IRequest<QueryResult<string>>;
}
