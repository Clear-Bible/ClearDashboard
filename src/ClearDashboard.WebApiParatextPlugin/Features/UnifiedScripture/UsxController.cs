using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Paratext;
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
        public async Task<RequestResult<UsxObject>> GetAsync([FromBody] GetUsxQuery query)
        {
            return await ExecuteRequestAsync<RequestResult<UsxObject>, UsxObject>(query, CancellationToken.None);
        }
    }
}