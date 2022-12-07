using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Extensions;

using ModelCorpusType = ClearDashboard.DataAccessLayer.Models.CorpusType;
using ModelCorpus = ClearDashboard.DataAccessLayer.Models.Corpus;
using System.Diagnostics;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class DeleteCorpusByCorpusIdCommandHandler : ProjectDbContextCommandHandler<DeleteCorpusByCorpusIdCommand,
        RequestResult<Unit>, Unit>
    {
        private readonly IMediator _mediator;

        public DeleteCorpusByCorpusIdCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteCorpusByCorpusIdCommandHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(
            DeleteCorpusByCorpusIdCommand request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var corpus = ProjectDbContext.Corpa
                .Include(c => c.TokenizedCorpora)
                    .ThenInclude(tc => tc.SourceParallelCorpora)
                .Include(c => c.TokenizedCorpora)
                    .ThenInclude(tc => tc.TargetParallelCorpora)
                .Where(e => e.Id == request.CorpusId.Id)
                .FirstOrDefault();

            if (corpus is null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid CorpusId '{request.CorpusId.Id}' found in request"
                );
            }

            ProjectDbContext.Remove(corpus);
            foreach (var tc in corpus.TokenizedCorpora)
            {
                ProjectDbContext.Remove(tc);

                // Cascade delete of a ParallelCorpus when its SourceTokenizedCorpus
                // is deleted doesn't work - maybe because of its TargetTokenziedCorpus
                // property?  So, we do the cascade delete manually:
                if (tc.SourceParallelCorpora.Any())
                {
                    ProjectDbContext.RemoveRange(tc.SourceParallelCorpora);
                }
                if (tc.TargetParallelCorpora.Any())
                {
                    ProjectDbContext.RemoveRange(tc.TargetParallelCorpora);
                }
            }
            
            _ = await ProjectDbContext.SaveChangesAsync(cancellationToken);

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}