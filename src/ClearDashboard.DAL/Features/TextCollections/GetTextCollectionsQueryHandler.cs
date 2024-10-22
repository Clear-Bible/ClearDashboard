﻿using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.ParatextPlugin.CQRS.Features.TextCollections;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.TextCollections
{
    public class GetTextCollectionsQueryHandler : ParatextRequestHandler<GetTextCollectionsQuery, RequestResult<List<TextCollection>>, List<TextCollection>>
    {
        public GetTextCollectionsQueryHandler(ILogger<GetTextCollectionsQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public async override Task<RequestResult<List<TextCollection>>> Handle(GetTextCollectionsQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("textcollections", request, cancellationToken);
        }
    }
}
