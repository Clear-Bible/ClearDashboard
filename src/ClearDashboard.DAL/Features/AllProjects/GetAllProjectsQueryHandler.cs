using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.AllProjects
{
    public class GetAllProjectsQueryHandler : ParatextRequestHandler<GetAllProjectsQuery, RequestResult<List<ParatextProject>>, List<ParatextProject>>
    {
        public GetAllProjectsQueryHandler(ILogger<GetAllProjectsQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public async override Task<RequestResult<List<ParatextProject>>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("allprojects", request, cancellationToken);
        }
    }
}
