using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using ClearDashboard.WebApiParatextPlugin.Features.AllProjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    public class GetExternalLabelsQueryController : FeatureSliceController
    {
        public GetExternalLabelsQueryController(IMediator mediator, ILogger<AllProjectsController> logger) : base(mediator, logger)
        {
        }

        [HttpPost]
        public async Task<RequestResult<IReadOnlyList<ExternalLabel>>> GetAsync([FromBody] GetExternalLabelsQuery command)
        {
            var result = await ExecuteRequestAsync<RequestResult<IReadOnlyList<ExternalLabel>>, IReadOnlyList<ExternalLabel>>(command, CancellationToken.None);
            return result;
        }
    }
}
