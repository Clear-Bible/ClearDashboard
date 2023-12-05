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
    public class GetNotesQueryController : FeatureSliceController
    {
        public GetNotesQueryController(IMediator mediator, ILogger<AllProjectsController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<IReadOnlyList<ExternalNote>>> GetAsync([FromBody] GetNotesQuery command)
        {
            var result = await ExecuteRequestAsync<RequestResult<IReadOnlyList<ExternalNote>>, IReadOnlyList<ExternalNote>>(command, CancellationToken.None);
            return result;
        }
    }
}
