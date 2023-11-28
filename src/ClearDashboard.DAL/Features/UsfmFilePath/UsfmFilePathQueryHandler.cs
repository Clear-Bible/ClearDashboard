using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.UsfmFilePath;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.UsfmFilePath
{
    public class UsfmFilePathQueryHandler : ParatextRequestHandler<GetUsfmFilePathQuery, RequestResult<List<string>>, List<string>>
    {

        public UsfmFilePathQueryHandler([NotNull] ILogger<GetUsfmFilePathQuery> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<List<string>>> Handle(GetUsfmFilePathQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("usfmfilepath", request, cancellationToken);
        }

    }
}
