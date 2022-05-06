using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.UnifiedScripture;

public record GetUsxQuery() : IRequest<RequestResult<string>>
{

}