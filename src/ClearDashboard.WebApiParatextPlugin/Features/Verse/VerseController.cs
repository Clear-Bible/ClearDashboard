using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClearDashboard.WebApiParatextPlugin.Features.Verse
{
    public class VerseController : FeatureSliceController
    {
        public VerseController(IMediator mediator, ILogger<VerseController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<string>> GetAsync([FromBody] GetCurrentVerseQuery query)
        {
            return await ExecuteRequestAsync<RequestResult<string>, string>(query, CancellationToken.None);
        }

        [HttpPut]
        public async Task<RequestResult<string>> PutAsync([FromBody] SetCurrentVerseCommand command)
        {
            return await ExecuteRequestAsync<RequestResult<string>, string>(command, CancellationToken.None);
        }

    }
}