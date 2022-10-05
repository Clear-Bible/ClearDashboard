using System.Collections.Generic;
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
    public record LoadProjectQuery(string projectName) : ProjectRequestQuery<IEnumerable<Corpus>>;

    public class LoadProjectQueryHandler : ProjectDbContextQueryHandler<LoadProjectQuery,
        RequestResult<IEnumerable<Corpus>>, IEnumerable<Corpus>>
    {
        private readonly IMediator _mediator;
        public LoadProjectQueryHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider, ILogger<LoadProjectQueryHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IEnumerable<Corpus>>> GetDataAsync(LoadProjectQuery request, CancellationToken cancellationToken)
        {
            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<Corpus>>(ProjectDbContext.Corpa
                .Include(corpus => corpus.TokenizedCorpora)
                    /*.ThenInclude(tokenizedCorpus => tokenizedCorpus.Tokens)*/);
        }
    }
}
