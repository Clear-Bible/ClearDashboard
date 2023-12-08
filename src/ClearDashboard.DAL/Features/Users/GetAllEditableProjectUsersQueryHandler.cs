using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.Users;

namespace ClearDashboard.DataAccessLayer.Features.Users
{
    public class GetAllEditableProjectUsersQueryHandler : ParatextRequestHandler<GetAllEditableProjectUsersQuery, RequestResult<List<string>>, List<string>>
    {

        public GetAllEditableProjectUsersQueryHandler([NotNull] ILogger<GetAllEditableProjectUsersQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<List<string>>> Handle(GetAllEditableProjectUsersQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("GetAllEditableProjectUsers", request, cancellationToken);
        }

    }
}
