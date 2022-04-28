using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using Microsoft.Extensions.Logging;
using ParaTextPlugin.Data.Features;
using ParaTextPlugin.Data.Features.UnifiedScripture;

namespace ClearDashboard.WebApiParatextPlugin.Features.UnifiedScripture
{
    public class UsxController : FeatureSliceController
    {
        public UsxController(IMediator mediator, ILogger<UsxController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<QueryResult<string>> GetAsync([FromBody] GetUsxQuery query)
        {
            return await ExecuteCommandAsync<QueryResult<string>, string>(query, CancellationToken.None);
        }
    }
}