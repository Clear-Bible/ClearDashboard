using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using System.Collections.Generic;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.UsfmFilePath
{
    public record GetUsfmFilePathQuery(string ParatextId) : IRequest<RequestResult<List<ParatextBook>>>
    {
        public string ParatextId { get; } = ParatextId;
    }
}
