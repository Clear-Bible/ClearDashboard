﻿using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Data.Entity;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class GetSemanticDomainsByPartialTextQueryHandler : ProjectDbContextQueryHandler<GetSemanticDomainsByPartialTextQuery,
        RequestResult<IEnumerable<SemanticDomain>>, IEnumerable<SemanticDomain>>
    {
        public GetSemanticDomainsByPartialTextQueryHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<GetSemanticDomainsByPartialTextQueryHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<SemanticDomain>>> GetDataAsync(GetSemanticDomainsByPartialTextQuery request, CancellationToken cancellationToken)
        {
            var semanticDomains = ProjectDbContext.Lexicon_SemanticDomains
                .Include(sd => sd.User)
                .Where(sd => sd.Text != null && sd.Text.StartsWith(request.PartialText))
                .Select(sd => new SemanticDomain(
                    ModelHelper.BuildSemanticDomainId(sd),
                    sd.Text!
                ));

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<SemanticDomain>>(semanticDomains.ToList());
        }
    }
}
