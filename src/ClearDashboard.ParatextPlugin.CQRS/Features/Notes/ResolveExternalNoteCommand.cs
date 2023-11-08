using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public record ResolveExternalNoteCommand(ResolveExternalNoteCommandParam Data) : IRequest<RequestResult<string>>
    {
        public ResolveExternalNoteCommandParam Data { get; } = Data;
    }
}
