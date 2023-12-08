using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.Notes
{
    public class GetExternalLabelsQueryHandler : ParatextRequestHandler<GetExternalLabelsQuery, RequestResult<IReadOnlyList<ExternalLabel>?>, IReadOnlyList<ExternalLabel>?>
    {

        public GetExternalLabelsQueryHandler([NotNull] ILogger<GetExternalLabelsQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<IReadOnlyList<ExternalLabel>?>> Handle(GetExternalLabelsQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("getexternallabelsquery", request, cancellationToken);
        }
    }
}
