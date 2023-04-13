using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Extensions;
using System.Diagnostics;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;


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
                .Include(e => e.SourceTokenizedCorpus)
                .Include(e => e.TargetTokenizedCorpus)
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

            parallelCorpusEntity.DisplayName = request.ParallelCorpusId.DisplayName;
            parallelCorpusEntity.Metadata = request.ParallelCorpusId.Metadata;

            if (request.VerseMappings.Any())
            {
                var previousVerseMappings = ProjectDbContext.VerseMappings
                    .Where(e => e.ParallelCorpusId == request.ParallelCorpusId.Id)
;
                ProjectDbContext.VerseMappings.RemoveRange(previousVerseMappings);

                var sourceCorpusId = parallelCorpusEntity.SourceTokenizedCorpus!.CorpusId;
                var targetCorpusId = parallelCorpusEntity.TargetTokenizedCorpus!.CorpusId;

                var newVerseMappings = request.VerseMappings
                    .Select(vm =>
                    {
                        var verseMapping = new Models.VerseMapping
                        {
                            ParallelCorpusId = parallelCorpusEntity.Id
                        };

                        verseMapping.Verses.AddRange(ParallelCorpusDataBuilder.BuildVerses(vm.SourceVerses, parallelCorpusEntity.Id, sourceCorpusId, cancellationToken));
                        verseMapping.Verses.AddRange(ParallelCorpusDataBuilder.BuildVerses(vm.TargetVerses, parallelCorpusEntity.Id, targetCorpusId, cancellationToken));

                        return verseMapping;
                    });

                parallelCorpusEntity.VerseMappings.AddRange(newVerseMappings);

#if DEBUG
                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - built replacement verse mapping entities [count: {newVerseMappings.Count()}]");
                sw.Restart();
#endif
            }

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

            return new RequestResult<Unit>(result: Unit.Value);
        }
    }
}
