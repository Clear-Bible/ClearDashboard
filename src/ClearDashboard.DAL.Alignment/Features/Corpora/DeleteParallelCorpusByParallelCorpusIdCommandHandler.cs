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
    public class DeleteParallelCorpusByParallelCorpusIdCommandHandler : ProjectDbContextCommandHandler<DeleteParallelCorpusByParallelCorpusIdCommand,
        RequestResult<Unit>, Unit>
    {
        private readonly IMediator _mediator;

        public DeleteParallelCorpusByParallelCorpusIdCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteParallelCorpusByParallelCorpusIdCommandHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(
            DeleteParallelCorpusByParallelCorpusIdCommand request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var parallelCorpus = ProjectDbContext.ParallelCorpa
                .Where(e => e.Id == request.ParallelCorpusId.Id)
                .FirstOrDefault();

            if (parallelCorpus is null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid ParallelCorpusId '{request.ParallelCorpusId.Id}' found in request"
                );
            }

            ProjectDbContext.Remove(parallelCorpus);
            _ = await ProjectDbContext.SaveChangesAsync(cancellationToken);

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}