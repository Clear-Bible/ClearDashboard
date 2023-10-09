using System.Collections.Generic;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public record AddNoteCommand(AddNoteCommandParam Data) : IRequest<RequestResult<ExternalNote>>
    {
        public AddNoteCommandParam Data { get; } = Data;
    }
}
