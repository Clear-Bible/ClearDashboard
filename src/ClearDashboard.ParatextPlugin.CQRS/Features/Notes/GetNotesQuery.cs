using System.Collections.Generic;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Paratext.PluginInterfaces;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public record GetNotesQuery(GetNotesQueryParam Data) : IRequest<RequestResult<IReadOnlyList<IProjectNote>>>
    {
        public GetNotesQueryParam Data { get; } = Data;
    }
}
