using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.Projects
{
    public record GetAllProjectsFromDatabaseQuery(string projectName) : ProjectRequestQuery<IEnumerable<Corpus>>;

    public class GetAllProjectsFromDatabaseQueryHandler : ProjectDbContextQueryHandler<GetAllProjectsFromDatabaseQuery,
        RequestResult<IEnumerable<Corpus>>, IEnumerable<Corpus>>
    {
        private readonly IMediator _mediator;
        public GetAllProjectsFromDatabaseQueryHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider, ILogger<GetAllProjectsFromDatabaseQueryHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IEnumerable<Corpus>>> GetDataAsync(GetAllProjectsFromDatabaseQuery request, CancellationToken cancellationToken)
        {
            var task = ProjectNameDbContextFactory.Get(request.projectName);
            var projectAssets = task.Result;
            return (RequestResult<IEnumerable<Corpus>>)EntityFrameworkQueryableExtensions
                .Include(projectAssets.ProjectDbContext.Corpa, corpus => corpus.TokenizedCorpora)
                .ThenInclude(tokenizedCorpus => tokenizedCorpus.Tokens);

        }
    }
}
