using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects;
using MediatR;
using Microsoft.Extensions.Logging;
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
        public async Task<RequestResult<List<ParatextProject>>> GetAsync([FromBody] GetAllProjectsQuery command)
        {
            var result = await ExecuteRequestAsync<RequestResult<List<ParatextProject>>, List<ParatextProject>>(command, CancellationToken.None);
            return result;
        }
    }
}
