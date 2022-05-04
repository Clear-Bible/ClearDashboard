using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.UnifiedScripture
{
    public record GetUsxQuery() : IRequest<QueryResult<string>>
    {

    }

    public record GetUsfmQuery() : IRequest<QueryResult<string>>
    {

    }
}
