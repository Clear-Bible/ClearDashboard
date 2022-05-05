using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
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
        public async Task<RequestResult<DataAccessLayer.Models.Project>> GetAsync([FromBody]GetCurrentProjectQuery query)
        {
            return await ExecuteRequestAsync<RequestResult<DataAccessLayer.Models.Project>, DataAccessLayer.Models.Project>(query, CancellationToken.None);

        }

    }
}
