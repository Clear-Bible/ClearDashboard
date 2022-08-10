using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using Microsoft.Extensions.Logging;
using SIL.Scripture;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models.Common;

namespace ClearDashboard.DataAccessLayer.Features.Versification
{
    public class GetVersificationAndBookIdByParatextPluginIdQueryHandler : ParatextRequestHandler<GetVersificationAndBookIdByParatextPluginIdQuery, RequestResult<VersificationBookIds>, VersificationBookIds>
    {

        public GetVersificationAndBookIdByParatextPluginIdQueryHandler([NotNull] ILogger<GetVersificationAndBookIdByParatextPluginIdQueryHandler> logger) :
            base(logger)
        {
            //no-op
        }


        public override async
            Task<RequestResult<VersificationBookIds>> Handle(GetVersificationAndBookIdByParatextPluginIdQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("versificationbooksbyparatextid", request, cancellationToken);
        }

    }
}
