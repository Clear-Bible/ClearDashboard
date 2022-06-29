using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features.Features;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.Project
{

    public class GetCurrentProjectCommandHandler : ParatextRequestHandler<GetCurrentProjectQuery, RequestResult<Models.Project>, Models.Project>
    {

        public GetCurrentProjectCommandHandler(ILogger<GetCurrentProjectCommandHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<Models.Project>> Handle(GetCurrentProjectQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("project", request, cancellationToken, HttpVerb.PUT);
        }

    }
}
