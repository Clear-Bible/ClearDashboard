using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using Microsoft.Extensions.Logging;
using ParaTextPlugin.Data.Features;
using ParaTextPlugin.Data.Features.Verse;

namespace ClearDashboard.WebApiParatextPlugin.Features.Verse
{
    public class VerseController : FeatureSliceController
    {
        public VerseController(IMediator mediator, ILogger<VerseController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<QueryResult<string>> GetAsync([FromBody] GetCurrentVerseCommand command)
        {
            return await ExecuteCommandAsync<QueryResult<string>, string>(command, CancellationToken.None);

        }

    }
}