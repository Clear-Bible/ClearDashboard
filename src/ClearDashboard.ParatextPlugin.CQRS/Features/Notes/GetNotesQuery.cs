using System.Collections.Generic;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public record GetNotesQuery(GetNotesQueryParam Data) : IRequest<RequestResult<IReadOnlyList<ExternalNote>>>
    {
        public GetNotesQueryParam Data { get; } = Data;
    }
}
