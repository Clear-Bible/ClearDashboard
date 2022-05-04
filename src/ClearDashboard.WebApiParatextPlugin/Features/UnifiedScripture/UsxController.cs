using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.UnifiedScripture;
using MediatR;
using Microsoft.Extensions.Logging;

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