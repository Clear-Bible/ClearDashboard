using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using Microsoft.Extensions.Logging;
using ParaTextPlugin.Data.Features;
using ParaTextPlugin.Data.Features.UnifiedScripture;

namespace ClearDashboard.WebApiParatextPlugin.Features.UnifiedScripture
{
    public class UsfmController : FeatureSliceController
    {
        public UsfmController(IMediator mediator, ILogger<UsfmController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<QueryResult<string>> GetAsync([FromBody] GetUsfmQuery query)
        {
            return await ExecuteCommandAsync<QueryResult<string>, string>(query, CancellationToken.None);
        }

    }
}