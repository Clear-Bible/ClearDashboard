using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using Microsoft.Extensions.Logging;
using ParaTextPlugin.Data.Features;
using ParaTextPlugin.Data.Features.Project;

namespace ClearDashboard.WebApiParatextPlugin.Features.Project
{
    public class ProjectController : FeatureSliceController
    {
        public ProjectController(IMediator mediator, ILogger<ProjectController> logger) : base(mediator, logger)
        { 
          
        }

        [HttpPost]
        public async Task<QueryResult<ParaTextPlugin.Data.Models.Project>> GetAsync([FromBody]GetCurrentProjectCommand command)
        {
            return await ExecuteCommandAsync<QueryResult<ParaTextPlugin.Data.Models.Project>, ParaTextPlugin.Data.Models.Project>(command, CancellationToken.None);

        }

    }
}
