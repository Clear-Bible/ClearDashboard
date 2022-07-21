using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.ParatextPlugin.CQRS.Features.ReferenceUsfm;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.ReferenceUsfm
{
    public class GetReferenceUsfmQueryHandler : ParatextRequestHandler<GetReferenceUsfmQuery, RequestResult<Models.Common.ReferenceUsfm>, Models.Common.ReferenceUsfm>
    {
        public GetReferenceUsfmQueryHandler(ILogger<GetReferenceUsfmQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<Models.Common.ReferenceUsfm>> Handle(GetReferenceUsfmQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("referenceusfm", request, cancellationToken);
        }
    }
}
