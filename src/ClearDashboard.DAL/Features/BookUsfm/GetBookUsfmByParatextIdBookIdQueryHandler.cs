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

namespace ClearDashboard.DataAccessLayer.Features.BookUsfm
{
    public class GetBookUsfmByParatextIdBookIdQueryHandler 
        : ParatextRequestHandler<GetRowsByParatextProjectIdAndBookIdQuery, RequestResult<List<UsfmVerse>>, List<UsfmVerse>>
    {

        public GetBookUsfmByParatextIdBookIdQueryHandler([NotNull] ILogger<GetBookUsfmByParatextIdBookIdQueryHandler> logger) :
            base(logger)
        {
            //no-op
        }
        

        public override async
            Task<RequestResult<List<UsfmVerse>>> Handle(
                GetRowsByParatextProjectIdAndBookIdQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("bookusfmbyparatextidbookid", request, cancellationToken);
        }

    }
}
