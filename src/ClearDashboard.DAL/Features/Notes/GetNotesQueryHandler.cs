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
    public class GetNotesQueryHandler : ParatextRequestHandler<GetNotesQuery, RequestResult<IReadOnlyList<ExternalNote>>, IReadOnlyList<ExternalNote>>
    {

        public GetNotesQueryHandler([NotNull] ILogger<GetNotesQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<IReadOnlyList<ExternalNote>>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("getnotesquery", request, cancellationToken);
        }
        
    }
}
