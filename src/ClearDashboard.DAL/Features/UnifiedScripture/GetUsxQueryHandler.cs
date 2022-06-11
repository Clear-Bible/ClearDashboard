using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Features.BiblicalTerms;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using ClearDashboard.ParatextPlugin.CQRS.Features.UnifiedScripture;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.UnifiedScripture
{
    public class GetUsxQueryHandler : ParatextRequestHandler<GetUsxQuery, RequestResult<StringObject>, StringObject>
    {

        public GetUsxQueryHandler([NotNull] ILogger<GetUsxQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<StringObject>> Handle(GetUsxQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("usx", request, cancellationToken);
        }
    }
}
