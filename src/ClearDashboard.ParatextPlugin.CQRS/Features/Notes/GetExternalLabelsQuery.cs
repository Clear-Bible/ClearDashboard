using ClearDashboard.DAL.CQRS;
using MediatR;
using System.Collections.Generic;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes
{
    public record GetExternalLabelsQuery(GetExternalLabelsQueryParam Data) : IRequest<RequestResult<IReadOnlyList<ExternalLabel>>>
    {
        public GetExternalLabelsQueryParam Data { get; } = Data;
    }
}
