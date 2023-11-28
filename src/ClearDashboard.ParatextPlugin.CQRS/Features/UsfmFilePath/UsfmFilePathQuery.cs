using ClearDashboard.DAL.CQRS;
using MediatR;
using System.Collections.Generic;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.UsfmFilePath
{
    public record GetUsfmFilePathQuery(string ParatextId) : IRequest<RequestResult<List<string>>>
    {
        public string ParatextId { get; } = ParatextId;
    }
}
