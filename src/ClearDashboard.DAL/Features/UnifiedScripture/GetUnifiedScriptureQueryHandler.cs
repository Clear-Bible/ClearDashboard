using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using ClearDashboard.ParatextPlugin.CQRS.Features.UnifiedScripture;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.UnifiedScripture
{
    public class GetUnifiedScriptureQueryHandler : ParatextRequestHandler<GetUsxQuery, RequestResult<string>, String>
    {

        public GetUnifiedScriptureQueryHandler([NotNull] ILogger<GetUnifiedScriptureQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<string>> Handle(GetUsxQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("unifiedscripture", request, cancellationToken);
        }

    }
}
