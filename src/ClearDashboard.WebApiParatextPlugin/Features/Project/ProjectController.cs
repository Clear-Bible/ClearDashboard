using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.Data.Features.Project;
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
        public async Task<QueryResult<ParatextPlugin.Data.Models.Project>> GetAsync([FromBody]GetCurrentProjectCommand command)
        {
            return await ExecuteCommandAsync<QueryResult<ParatextPlugin.Data.Models.Project>, ParatextPlugin.Data.Models.Project>(command, CancellationToken.None);

        }

    }
}
