using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Features.BiblicalTerms;
using ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.BookUsfm
{
    public class GetBookUsfmByParatextIdBookIdQueryHandler 
        : ParatextRequestHandler<GetBookUsfmByParatextIdBookIdQuery, RequestResult<
                IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>, 
            IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>
    {

        public GetBookUsfmByParatextIdBookIdQueryHandler([NotNull] ILogger<GetBiblicalTermsByTypeQueryHandler> logger) :
            base(logger)
        {
            //no-op
        }
        

        public override async
            Task<RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>> Handle(
                GetBookUsfmByParatextIdBookIdQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("bookusfmbyparatextidbookid", request, cancellationToken);
        }

    }
}
