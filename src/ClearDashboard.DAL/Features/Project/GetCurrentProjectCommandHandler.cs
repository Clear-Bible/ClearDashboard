using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.Project
{

    public class GetCurrentProjectQueryHandler : ParatextRequestHandler<GetCurrentProjectQuery, RequestResult<Models.ParatextProject>, Models.ParatextProject>
    {

        public GetCurrentProjectQueryHandler(ILogger<GetCurrentProjectQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<Models.ParatextProject>> Handle(GetCurrentProjectQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("project", request, cancellationToken);
        }

    }
}
