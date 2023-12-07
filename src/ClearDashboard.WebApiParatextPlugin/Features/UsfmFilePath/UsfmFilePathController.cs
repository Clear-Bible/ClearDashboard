using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.UsfmFilePath;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClearDashboard.WebApiParatextPlugin.Features.UsfmFilePath
{

    public class UsfmFilePathController : FeatureSliceController
    {
        public UsfmFilePathController(IMediator mediator, ILogger<UsfmFilePathController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<List<ParatextBook>>> GetAsync([FromBody] GetUsfmFilePathQuery command)
        {
            var result = await ExecuteRequestAsync<RequestResult<List<ParatextBook>>, List<ParatextBook>>(command, CancellationToken.None);
            return result;

        }

    }
}
