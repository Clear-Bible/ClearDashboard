using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.UnifiedScripture;

public record GetUsxQuery(int? BookNumber = null) : IRequest<RequestResult<string>>
{
    public int? BookNumber { get; } = BookNumber;
}