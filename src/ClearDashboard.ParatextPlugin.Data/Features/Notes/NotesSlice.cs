using MediatR;
using Paratext.PluginInterfaces;
using ParaTextPlugin.Data.Models;
using System.Collections.Generic;

namespace ParaTextPlugin.Data.Features.Notes
{
    public record GetNotesQuery(GetNotesData Data) : IRequest<QueryResult<IReadOnlyList<IProjectNote>>>
    {
        public GetNotesData Data { get; } = Data;
    }
}
