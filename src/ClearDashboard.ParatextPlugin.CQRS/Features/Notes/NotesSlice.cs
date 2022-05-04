using System.Collections.Generic;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Paratext.PluginInterfaces;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public record GetNotesQuery(GetNotesData Data) : IRequest<QueryResult<IReadOnlyList<IProjectNote>>>
    {
        public GetNotesData Data { get; } = Data;
    }
}
