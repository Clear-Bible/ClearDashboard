using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.Projects
{
  
    public class GetProjectMetadataQueryHandler : ParatextRequestHandler<GetProjectMetadataQuery, RequestResult<List<ParatextProjectMetadata>>, List<ParatextProjectMetadata>>
    {
        public GetProjectMetadataQueryHandler(ILogger<GetProjectMetadataQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public async override Task<RequestResult<List<ParatextProjectMetadata>>> Handle(GetProjectMetadataQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("projects/metadata", request, cancellationToken);
        }
    }
}
