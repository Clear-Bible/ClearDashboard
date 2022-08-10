using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Scripture;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ClearDashboard.DataAccessLayer.Models.Common;

namespace ClearDashboard.WebApiParatextPlugin.Features.Versification
{
    public class VersificationBooksByParatextIdController : FeatureSliceController
    {
        public VersificationBooksByParatextIdController(IMediator mediator, ILogger<VersificationBooksByParatextIdController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<VersificationBookIds>> GetAsync([FromBody] GetVersificationAndBookIdByParatextPluginIdQuery command)
        {
            var result =
                await ExecuteRequestAsync<RequestResult<VersificationBookIds>, VersificationBookIds>(command, CancellationToken.None);
            return result;

        }
    }
}
