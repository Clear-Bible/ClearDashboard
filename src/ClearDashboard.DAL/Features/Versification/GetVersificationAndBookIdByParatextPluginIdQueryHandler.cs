using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Features.BiblicalTerms;
using ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models.Common;
using SIL.Scripture;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;

namespace ClearDashboard.DataAccessLayer.Features.Versification
{
    public class GetVersificationAndBookIdByParatextPluginIdQueryHandler : ParatextRequestHandler<GetVersificationAndBookIdByParatextPluginIdQuery, RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>, (ScrVers? versification, IEnumerable<string> bookAbbreviations)>
    {

        public GetVersificationAndBookIdByParatextPluginIdQueryHandler([NotNull] ILogger<GetVersificationAndBookIdByParatextPluginIdQueryHandler> logger) :
            base(logger)
        {
            //no-op
        }


        public override async
            Task<RequestResult<(ScrVers? versification, IEnumerable<string> bookAbbreviations)>> Handle(GetVersificationAndBookIdByParatextPluginIdQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("versificationandbookidbyparatextpluginId", request, cancellationToken);
        }

    }
}
