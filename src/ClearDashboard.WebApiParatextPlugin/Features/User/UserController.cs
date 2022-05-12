using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using ClearDashboard.ParatextPlugin.CQRS.Features.User;
using ClearDashboard.WebApiParatextPlugin.Features.Project;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.WebApiParatextPlugin.Features.User
{
    public class UserController : FeatureSliceController
    {
        public UserController(IMediator mediator, ILogger<UserController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<AssignedUser>> GetAsync([FromBody] GetCurrentParatextUserQuery query)
        {
            return await ExecuteRequestAsync<RequestResult<AssignedUser>, AssignedUser>(query, CancellationToken.None);

        }
    }
}
