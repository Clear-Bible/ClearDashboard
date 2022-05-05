using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Verse
{
    public record GetCurrentVerseQuery() : IRequest<RequestResult<string>>;
}
