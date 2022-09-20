using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.Projects
{
    public record LoadCorporaQuery(string projectName) : ProjectRequestQuery<IEnumerable<Corpus>>;

    public class LoadCorporaQueryHandler : ProjectDbContextQueryHandler<LoadCorporaQuery,
        RequestResult<IEnumerable<Corpus>>, IEnumerable<Corpus>>
    {
        private readonly IMediator _mediator;
        public LoadCorporaQueryHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider, ILogger<LoadCorporaQueryHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IEnumerable<Corpus>>> GetDataAsync(LoadCorporaQuery request, CancellationToken cancellationToken)
        {
            var task = ProjectNameDbContextFactory.Get(request.projectName);
            var projectAssets = task.Result;
            return (RequestResult<IEnumerable<Corpus>>)EntityFrameworkQueryableExtensions
                .Include(projectAssets.ProjectDbContext.Corpa, corpus => corpus.TokenizedCorpora)
                .ThenInclude(tokenizedCorpus => tokenizedCorpus.Tokens);

        }
    }
}

