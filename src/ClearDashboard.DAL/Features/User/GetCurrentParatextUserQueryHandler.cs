using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using ClearDashboard.ParatextPlugin.CQRS.Features.User;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.User
{


    public class GetCurrentParatextUserQueryHandler : ParatextRequestHandler<GetCurrentParatextUserQuery, RequestResult<AssignedUser>, AssignedUser>
    {

        public GetCurrentParatextUserQueryHandler([NotNull] ILogger<GetCurrentParatextUserQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<AssignedUser>> Handle(GetCurrentParatextUserQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("user", request, cancellationToken);
        }

    }
}
