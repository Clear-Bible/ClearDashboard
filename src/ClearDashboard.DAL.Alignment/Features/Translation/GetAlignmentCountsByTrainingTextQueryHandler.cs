using System.Diagnostics;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Extensions;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetAlignmentCountsByTrainingTextQueryHandler : ProjectDbContextQueryHandler<
        GetAlignmentCountsByTrainingTextQuery,
        RequestResult<IDictionary<string, IDictionary<string, uint>>>,
        IDictionary<string, IDictionary<string, uint>>>
    {

        public GetAlignmentCountsByTrainingTextQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAlignmentCountsByTrainingTextQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IDictionary<string, IDictionary<string, uint>>>> GetDataAsync(GetAlignmentCountsByTrainingTextQuery request, CancellationToken cancellationToken)
        {
            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            var alignmentSet = ProjectDbContext!.AlignmentSets
                .Include(e => e.ParallelCorpus)
                    .ThenInclude(e => e!.TargetTokenizedCorpus)
                .FirstOrDefault(ts => ts.Id == request.AlignmentSetId.Id);

            if (alignmentSet == null)
            {
                return new RequestResult<IDictionary<string, IDictionary<string, uint>>>
                (
                    success: false,
                    message: $"AlignmentSet not found for AlignmentSetId '{request.AlignmentSetId.Id}'"
                );
            }

            Stopwatch sw = new();
            sw.Start();

            var alignmentsQueryable = ProjectDbContext.Alignments
                .Include(e => e.SourceTokenComponent)
                .Include(e => e.TargetTokenComponent)
                .Where(e => e.AlignmentSetId == request.AlignmentSetId.Id)
                .Where(e => e.Deleted == null)
                .Select(e => new { SourceTrainingText = e.SourceTokenComponent!.TrainingText!, TargetTrainingText = e.TargetTokenComponent!.TrainingText! });

            IDictionary<string, IDictionary<string, uint>>? alignmentCountsByTrainingText = default;

            if (request.SourceToTarget)
            {
                alignmentCountsByTrainingText = alignmentsQueryable
                    .GroupBy(e => e.SourceTrainingText)
                    .ToList()
                    .OrderByDescending(g => g.Count())
                    .ToDictionary(g => g.Key, g => g
                        .GroupBy(e => e.TargetTrainingText)
                        .OrderByDescending(g2 => g2.Count())
                        .ToDictionary(g2 => g2.Key, g2 => (uint)g2.Count())
                        as IDictionary<string, uint>)
                    as IDictionary<string, IDictionary<string, uint>>;
            }
            else
            {
                alignmentCountsByTrainingText = alignmentsQueryable
                    .GroupBy(e => e.TargetTrainingText)
                    .ToList()
                    .OrderByDescending(g => g.Count())
                    .ToDictionary(g => g.Key, g => g
                        .GroupBy(e => e.SourceTrainingText)
                        .OrderByDescending(g2 => g2.Count())
                        .ToDictionary(g2 => g2.Key, g2 => (uint)g2.Count())
                        as IDictionary<string, uint>)
                    as IDictionary<string, IDictionary<string, uint>>;
            }

            sw.Stop();
            Logger.LogInformation("Elapsed {0}", sw.Elapsed);

            return new RequestResult<IDictionary<string, IDictionary<string, uint>>>
            (
                alignmentCountsByTrainingText
            );
        }
    }
}

