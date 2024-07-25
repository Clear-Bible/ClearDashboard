using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.Lexicon
{
    public class GetWordAnalysesQueryHandler : ParatextRequestHandler<GetWordAnalysesQuery, RequestResult<IEnumerable<Models.Lexicon_WordAnalysis>>, IEnumerable<Models.Lexicon_WordAnalysis>>
    {
        public GetWordAnalysesQueryHandler([NotNull] ILogger<GetWordAnalysesQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<IEnumerable<Models.Lexicon_WordAnalysis>>> Handle(GetWordAnalysesQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("getwordanalysesquery", request, cancellationToken);
        }
    }
}
