using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.UnifiedScripture;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.UnifiedScripture
{
    public class GetUsxQueryHandler : ParatextRequestHandler<GetUsxQuery, RequestResult<string>, string>
    {

        public GetUsxQueryHandler([NotNull] ILogger<GetUsxQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<string>> Handle(GetUsxQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("usx", request, cancellationToken);
        }
    }
}
