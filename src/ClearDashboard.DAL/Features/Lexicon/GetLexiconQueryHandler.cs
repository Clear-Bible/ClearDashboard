using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.Lexicon
{
    public class GetLexiconQueryHandler : ParatextRequestHandler<GetLexiconQuery, RequestResult<Models.Lexicon_Lexicon>, Models.Lexicon_Lexicon>
    {
        public GetLexiconQueryHandler([NotNull] ILogger<GetLexiconQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<Models.Lexicon_Lexicon>> Handle(GetLexiconQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("getlexiconquery", request, cancellationToken);
        }
    }
}
