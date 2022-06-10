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
    public class UsfmController : FeatureSliceController
    {
        public UsfmController(IMediator mediator, ILogger<UsfmController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<StringObject>> GetAsync([FromBody] GetUsfmQuery query)
        {
            return await ExecuteRequestAsync<RequestResult<StringObject>, StringObject>(query, CancellationToken.None);
        }

    }
}