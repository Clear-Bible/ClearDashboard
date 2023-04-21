using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SIL.Extensions;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using System.Diagnostics;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Features.Common;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class CreateParallelCorpusCommandHandler : ProjectDbContextCommandHandler<CreateParallelCorpusCommand,
        RequestResult<ParallelCorpusId>, ParallelCorpusId>
    {
        private readonly IMediator _mediator;

        public CreateParallelCorpusCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateParallelCorpusCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<ParallelCorpusId>> SaveDataAsync(CreateParallelCorpusCommand request,
            CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            //DB Impl notes:
            //1. Create a new record in ParallelCorpus, save ParallelCorpusId
            //2. Create VerseMappings with verses
            //3. return created ParallelCorpus based on ParallelCorpusId
            //var parallelCorpus = await ParallelCorpus.Get(_mediator, new ParallelCorpusId(new Guid()));

            var sourceTokenizedCorpus = ProjectDbContext.TokenizedCorpora.FirstOrDefault(tc => tc.Id == request.SourceTokenizedCorpusId.Id);
            var targetTokenizedCorpus = ProjectDbContext.TokenizedCorpora.FirstOrDefault(tc => tc.Id == request.TargetTokenizedCorpusId.Id);

            if (sourceTokenizedCorpus == null || targetTokenizedCorpus == null)
            {
                return new RequestResult<ParallelCorpusId>
                (
                    success: false,
                    message: sourceTokenizedCorpus == null ?
                        $"SourceTokenizedCorpus not found for TokenizedCorpusId '{request.SourceTokenizedCorpusId.Id}'" :
                        $"TargetTokenizedCorpus not found for TokenizedCorpusId '{request.TargetTokenizedCorpusId.Id}'"
                );
            }

            if (request.VerseMappings
                .Any(vm => vm.SourceVerses
                    .Any(v => v.TokenIds.Where(tid => tid.GetType() == typeof(CompositeTokenId)).Any())))
            {
                return new RequestResult<ParallelCorpusId>
                (
                    success: false,
                    message: $"VerseMappings SourceVerses contain at least one CompositeTokenId (only regular TokenIds allowed)"
                );
            }

            if (request.VerseMappings
                .Any(vm => vm.TargetVerses
                    .Any(v => v.TokenIds.Where(tid => tid.GetType() == typeof(CompositeTokenId)).Any())))
            {
                return new RequestResult<ParallelCorpusId>
                (
                    success: false,
                    message: $"VerseMappings TargetVerses contain at least one CompositeTokenId (only regular TokenIds allowed)"
                );
            }

#if DEBUG
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Insert ParallelCorpus '{request.DisplayName}' [verse mapping count: {request.VerseMappings.Count()}]");
            sw.Restart();
            Process proc = Process.GetCurrentProcess();

            proc.Refresh();
            Logger.LogInformation($"Private memory usage (BEFORE INSERT): {proc.PrivateMemorySize64}");
#endif

            // Create and Save the Parallel Corpus Model
            // + with Verse Mappings
            var parallelCorpusModel = ParallelCorpusDataBuilder.BuildParallelCorpus(
                Guid.NewGuid(),
                sourceTokenizedCorpus,
                targetTokenizedCorpus,
                request.VerseMappings,
                request.DisplayName,
                cancellationToken);

            ProjectDbContext.ParallelCorpa.Add(parallelCorpusModel);
            await ProjectDbContext.SaveChangesAsync(cancellationToken);

#if DEBUG
            proc.Refresh();
            Logger.LogInformation($"Private memory usage (AFTER INSERT): {proc.PrivateMemorySize64}");

            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

            return new RequestResult<ParallelCorpusId>(new ParallelCorpusId(parallelCorpusModel.Id));
        }
    }
}