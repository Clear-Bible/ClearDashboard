using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public record CanAddNoteForProjectAndUserQuery(string ParatextUserName, string ParatextProjectId) : IRequest<RequestResult<bool>>
    {
        public string ParatextUserName { get; } = ParatextUserName;
        public string ParatextProjectId { get; } = ParatextProjectId;
    }
}
