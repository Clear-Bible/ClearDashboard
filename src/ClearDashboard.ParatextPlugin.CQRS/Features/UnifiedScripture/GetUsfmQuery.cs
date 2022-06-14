using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.UnifiedScripture
{
    public record GetUsfmQuery(int? BookNumber) : IRequest<RequestResult<string>>
    {
        public int? BookNumber { get; } = BookNumber;
    }
}
