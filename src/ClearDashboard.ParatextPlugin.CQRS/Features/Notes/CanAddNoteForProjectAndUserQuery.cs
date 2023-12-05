using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public record CanAddNoteForProjectAndUserQuery(string ExternalUserName, string ExternalProjectId) : IRequest<RequestResult<bool>>
    {
        public string ExternalUserName { get; } = ExternalUserName;
        public string ExternalProjectId { get; } = ExternalProjectId;
    }
}
