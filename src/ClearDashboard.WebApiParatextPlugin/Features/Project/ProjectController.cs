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
        public async Task<RequestResult<ParatextProject>> GetAsync([FromBody] GetCurrentProjectQuery query)
        {
            return await ExecuteRequestAsync<RequestResult<ParatextProject>, DataAccessLayer.Models.ParatextProject>(query, CancellationToken.None);

        }

        [HttpPost]
        //[Route("api/projects/metadata}")] Comment out in order for GetCurrentProjectTest to pass
        [ActionName("metadata")]
        public async Task<RequestResult<List<ParatextProjectMetadata>>> GetProjectMetadataAsync([FromBody] GetProjectMetadataQuery query)
        {
            return await ExecuteRequestAsync<RequestResult<List<ParatextProjectMetadata>>, List<ParatextProjectMetadata>>(query, CancellationToken.None);
        }

        [HttpPost]
        [ActionName("fontfamily")]
        public async Task<RequestResult<string>> GetProjectFontFamilyAsync([FromBody] GetProjectFontFamilyQuery query)
        {
            return await ExecuteRequestAsync<RequestResult<string>, string>(query, CancellationToken.None);
        }

    }
}
