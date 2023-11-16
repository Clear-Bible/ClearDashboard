using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClearDashboard.WebApiParatextPlugin.Features.Users
{
    public class GetAllEditableProjectUsersController : FeatureSliceController
    {
        public GetAllEditableProjectUsersController(IMediator mediator, ILogger<GetAllEditableProjectUsersController> logger) : base(mediator, logger)
        {
        }

        [HttpPost]
        public async Task<RequestResult<List<string>>> GetAsync([FromBody] GetAllEditableProjectUsersQuery command)
        {
            var result = await ExecuteRequestAsync<RequestResult<List<string>>, List<string>>(command, CancellationToken.None);
            return result;
        }

    }
}
