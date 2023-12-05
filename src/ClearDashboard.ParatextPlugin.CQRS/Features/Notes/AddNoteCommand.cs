using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public record AddNoteCommand(AddNoteCommandParam Data) : IRequest<RequestResult<ExternalNote>>
    {
        public AddNoteCommandParam Data { get; } = Data;
    }
}
