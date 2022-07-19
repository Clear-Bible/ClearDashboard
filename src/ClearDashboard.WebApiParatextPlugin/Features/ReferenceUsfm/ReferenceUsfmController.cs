using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.ReferenceUsfm;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClearDashboard.WebApiParatextPlugin.Features.ReferenceUsfm
{
    public class ReferenceUsfmController : FeatureSliceController
    {
        public ReferenceUsfmController(IMediator mediator, ILogger<ReferenceUsfmController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<DataAccessLayer.Models.Common.ReferenceUsfm>> GetAsync([FromBody] GetReferenceUsfmQuery command)
        {
            var result = await ExecuteRequestAsync<RequestResult<DataAccessLayer.Models.Common.ReferenceUsfm>, DataAccessLayer.Models.Common.ReferenceUsfm>(command, CancellationToken.None);
            return result;
        }

    }
}
