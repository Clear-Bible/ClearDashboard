using System.Collections.Generic;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;

namespace ClearDashboard.DataAccessLayer.Features.CheckUsfm
{
    public class GetCheckUsfmQueryHandler : ParatextRequestHandler<GetCheckUsfmQuery, RequestResult<List<UsfmHelper>>, List<UsfmHelper>>
    {

        public GetCheckUsfmQueryHandler(ILogger<GetCheckUsfmQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<List<UsfmHelper>>> Handle(GetCheckUsfmQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("checkusfm", request, cancellationToken);
        }
    }


}
