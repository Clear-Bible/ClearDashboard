using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.Data.Features.Verse
{
    public record GetCurrentVerseCommand() : IRequest<QueryResult<string>>;
}
