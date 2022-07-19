using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.AllProjects
{
    public class GetAllProjectsQueryHandler : ParatextRequestHandler<GetAllProjectsQuery, RequestResult<List<IProject>>, List<IProject>>
    {
        public GetAllProjectsQueryHandler(ILogger<GetAllProjectsQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public async override Task<RequestResult<List<IProject>>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("allprojects", request, cancellationToken);
        }
    }
}
