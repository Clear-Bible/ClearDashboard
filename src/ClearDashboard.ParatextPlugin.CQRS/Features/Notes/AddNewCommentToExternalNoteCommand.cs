using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public record AddNewCommentToExternalNoteCommand(AddNewCommentToExternalNoteCommandParam Data) : IRequest<RequestResult<string>>
    {
        public AddNewCommentToExternalNoteCommandParam Data { get; } = Data;
    }
}
