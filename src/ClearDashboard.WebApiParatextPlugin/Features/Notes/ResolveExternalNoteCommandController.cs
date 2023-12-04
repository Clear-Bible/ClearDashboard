using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using ClearDashboard.WebApiParatextPlugin.Features.AllProjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    public class ResolveExternalNoteCommandController : FeatureSliceController
    {
        public ResolveExternalNoteCommandController(IMediator mediator, ILogger<AllProjectsController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<string>> GetAsync([FromBody] ResolveExternalNoteCommand command)
        {
            var result = await ExecuteRequestAsync<RequestResult<string>, string>(command, CancellationToken.None);
            return result;
        }
    }
}
