using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClearDashboard.WebApiParatextPlugin.Features.AllProjects
{
    public class AllProjectsController : FeatureSliceController
    {
        public AllProjectsController(IMediator mediator, ILogger<AllProjectsController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<List<IProject>>> GetAsync([FromBody] GetAllProjectsQuery command)
        {
            var result = await ExecuteRequestAsync<RequestResult<List<IProject>>, List<IProject>>(command, CancellationToken.None);
            return result;
        }
    }
}
