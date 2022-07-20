using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.WebApiParatextPlugin.Features.Project
{
    public class ProjectController : FeatureSliceController
    {
        public ProjectController(IMediator mediator, ILogger<ProjectController> logger) : base(mediator, logger)
        { 
          
        }

        [HttpPost]
        public async Task<RequestResult<DataAccessLayer.Models.ParatextProject>> GetAsync([FromBody]GetCurrentProjectQuery query)
        {
            return await ExecuteRequestAsync<RequestResult<DataAccessLayer.Models.ParatextProject>, DataAccessLayer.Models.ParatextProject>(query, CancellationToken.None);

        }

        [HttpPost]
        [Route("metadata")]
        public async Task<RequestResult<List<ParatextProjectMetadata>>> GetProjectMetadataAsync([FromBody] GetProjectMetadataQuery query)
        {
            return await ExecuteRequestAsync<RequestResult<List<ParatextProjectMetadata>>, List<ParatextProjectMetadata>>(query, CancellationToken.None);

        }

    }
}
