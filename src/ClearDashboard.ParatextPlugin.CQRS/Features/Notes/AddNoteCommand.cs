using System.Collections.Generic;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Paratext.PluginInterfaces;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public record AddNoteCommand(AddNoteCommandParam Data) : IRequest<RequestResult<IProjectNote>>
    {
        public AddNoteCommandParam Data { get; } = Data;
    }
}
