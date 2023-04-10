using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;


//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;


namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class UpdateParallelCorpusCommandHandler : ProjectDbContextCommandHandler<UpdateParallelCorpusCommand, RequestResult<Unit>, Unit>
    {
        private readonly IMediator _mediator;

        public UpdateParallelCorpusCommandHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<UpdateParallelCorpusCommandHandler> logger) : base(projectNameDbContextFactory,projectProvider,  logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(UpdateParallelCorpusCommand request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            //DB Impl notes:
            //1. find parallelCorpus based on request.ParallelCorpusId
            //2. Update the verse mappings

            var parallelCorpusEntity = ProjectDbContext.ParallelCorpa
                .FirstOrDefault(pc => pc.Id == request.ParallelCorpusId.Id);

            if (parallelCorpusEntity == null)
            {
#if DEBUG
                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"ParallelCorpus not found for ParallelCorpusId '{request.ParallelCorpusId.Id}'"
                );
            }

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

            return new RequestResult<Unit>(result: Unit.Value);
        }
    }
}
