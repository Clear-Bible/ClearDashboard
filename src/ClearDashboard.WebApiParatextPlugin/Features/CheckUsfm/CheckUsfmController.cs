using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClearDashboard.WebApiParatextPlugin.Features.CheckUsfm
{
    public class CheckUsfmController : FeatureSliceController
    {
        public CheckUsfmController(IMediator mediator, ILogger<GetCheckUsfmQuery> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<List<UsfmError>>> GetAsync([FromBody] GetCheckUsfmQuery command)
        {
            var result = await ExecuteRequestAsync<RequestResult<List<UsfmError>>, List<UsfmError>>(command, CancellationToken.None);
            return result;
        }

    }
}
