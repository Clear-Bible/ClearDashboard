using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetParallelCorpusByParallelCorpusIdQueryHandler : ProjectDbContextQueryHandler<
        GetParallelCorpusByParallelCorpusIdQuery,
        RequestResult<(TokenizedTextCorpusId sourceTokenizedCorpusId,
            TokenizedTextCorpusId targetTokenizedCorpusId,
            ParallelCorpusId parallelCorpusId)>,
        (TokenizedTextCorpusId sourceTokenizedCorpusId,
        TokenizedTextCorpusId targetTokenizedCorpusId,
        ParallelCorpusId parallelCorpusId)>
    {

        public GetParallelCorpusByParallelCorpusIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetParallelCorpusByParallelCorpusIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<(TokenizedTextCorpusId sourceTokenizedCorpusId, 
            TokenizedTextCorpusId targetTokenizedCorpusId,
            ParallelCorpusId parallelCorpusId)>> GetDataAsync(GetParallelCorpusByParallelCorpusIdQuery request, CancellationToken cancellationToken)

        {
#if DEBUG
            Stopwatch sw = new();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            await Task.CompletedTask;

            //DB Impl notes: use command.ParallelCorpusId to retrieve from ParallelCorpus table and return
            //1. the result of gathering all the VerseMappings to build an VerseMapping list.
            //2. associated source and target TokenizedCorpusId

            var parallelCorpus =
                ModelHelper.AddIdIncludesParallelCorpaQuery(ProjectDbContext)
                    .Include(pc => pc.TokenComposites.Where(tc => tc.Deleted == null))
                        .ThenInclude(tc => tc.Tokens)
                            .ThenInclude(t => t.VerseRow)
                    .FirstOrDefault(pc => pc.Id == request.ParallelCorpusId.Id);

#if DEBUG
            sw.Stop();
#endif

            var invalidArgMsg = "";
            if (parallelCorpus == null)
            {
                invalidArgMsg = $"ParallelCorpus not found for ParallelCorpusId '{request.ParallelCorpusId.Id}'";
            }
            else if (parallelCorpus!.SourceTokenizedCorpus == null || parallelCorpus.TargetTokenizedCorpus == null)
            {
                // Not sure this condition is possible, since we
                // are using .Include() for both properties:
                invalidArgMsg = $"ParallelCorpus '{request.ParallelCorpusId.Id}' has null source or target tokenized corpus";
            }

            if (!string.IsNullOrEmpty(invalidArgMsg))
            {
                return new RequestResult<(TokenizedTextCorpusId sourceTokenizedCorpusId,
                    TokenizedTextCorpusId targetTokenizedCorpusId,
                    ParallelCorpusId parallelCorpusId)>
                (
                    success: false,
                    message: invalidArgMsg
                );
            }

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

            return new RequestResult<(TokenizedTextCorpusId sourceTokenizedCorpusId,
                    TokenizedTextCorpusId targetTokenizedCorpusId,
                    ParallelCorpusId parallelCorpusId)>
                ((
                    ModelHelper.BuildTokenizedTextCorpusId(parallelCorpus!.SourceTokenizedCorpus!),
                    ModelHelper.BuildTokenizedTextCorpusId(parallelCorpus!.TargetTokenizedCorpus!),
                    ModelHelper.BuildParallelCorpusId(parallelCorpus)
                ));
        }
    }
}
