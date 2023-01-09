using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.Projects
{
    public class GetProjectMetadataQueryHandler : ParatextRequestHandler<GetProjectMetadataQuery, RequestResult<List<ParatextProjectMetadata>>, List<ParatextProjectMetadata>>
    {
        public GetProjectMetadataQueryHandler(ILogger<GetProjectMetadataQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<List<ParatextProjectMetadata>>> Handle(GetProjectMetadataQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("project/metadata", request, cancellationToken);
        }
    }
}
