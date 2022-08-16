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
    public class GetVersificationAndBookIdByDalParatextProjectIdQueryHandler : ParatextRequestHandler<GetVersificationAndBookIdByDalParatextProjectIdQuery, RequestResult<VersificationBookIds>, VersificationBookIds>
    {

        public GetVersificationAndBookIdByDalParatextProjectIdQueryHandler([NotNull] ILogger<GetVersificationAndBookIdByDalParatextProjectIdQueryHandler> logger) :
            base(logger)
        {
            //no-op
        }


        public override async
            Task<RequestResult<VersificationBookIds>> Handle(GetVersificationAndBookIdByDalParatextProjectIdQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("versificationbooksbyparatextid", request, cancellationToken);
        }

    }
}
