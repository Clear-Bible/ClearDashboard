using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Features.BiblicalTerms;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.Project
{


    public class GetCurrentProjectCommandHandler : ParatextRequestHandler<GetCurrentProjectCommand, QueryResult<Models.Project>, Models.Project>
    {

        public GetCurrentProjectCommandHandler([NotNull] ILogger<GetCurrentProjectCommandHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<QueryResult<Models.Project>> Handle(GetCurrentProjectCommand request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("project", request, cancellationToken);
        }

    }
}
