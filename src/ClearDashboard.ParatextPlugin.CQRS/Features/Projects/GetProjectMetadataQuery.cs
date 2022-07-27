using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using System.Collections.Generic;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Projects
{
    public record GetProjectMetadataQuery() : IRequest<RequestResult<List<ParatextProjectMetadata>>>;
}
