using System.Diagnostics;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.SyntaxTree.Aligner.Legacy;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Extensions;
using SIL.Linq;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetAlignmentCountsByTrainingOrSurfaceTextQueryHandler : ProjectDbContextQueryHandler<
        GetAlignmentCountsByTrainingOrSurfaceTextQuery,
        RequestResult<IDictionary<string, IDictionary<string, IDictionary<string, uint>>>>,
        IDictionary<string, IDictionary<string, IDictionary<string, uint>>>>
    {

        public class AlignmentWithText : Models.Alignment
        {
            public string SourceText { get; set; } = string.Empty;
            public string TargetText { get; set; } = string.Empty;
            public string AlignmentTypeName { get; set; } = string.Empty;
        }

        public GetAlignmentCountsByTrainingOrSurfaceTextQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAlignmentCountsByTrainingOrSurfaceTextQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IDictionary<string, IDictionary<string, IDictionary<string, uint>>>>> GetDataAsync(GetAlignmentCountsByTrainingOrSurfaceTextQuery request, CancellationToken cancellationToken)
        {
            var alignmentSet = await ProjectDbContext!.AlignmentSets
                .FirstOrDefaultAsync(ts => ts.Id == request.AlignmentSetId.Id);

            if (alignmentSet == null)
            {
                return new RequestResult<IDictionary<string, IDictionary<string, IDictionary<string, uint>>>>
                (
                    success: false,
                    message: $"AlignmentSet not found for AlignmentSetId '{request.AlignmentSetId.Id}'"
                );
            }

#if DEBUG
            Stopwatch sw = new();
            sw.Start();
#endif

            var databaseAlignmentsQueryable = ProjectDbContext.Alignments
                .Include(e => e.SourceTokenComponent)
                .Include(e => e.TargetTokenComponent)
                .Where(e => e.AlignmentSetId == request.AlignmentSetId.Id)
                .Where(e => e.Deleted == null);

            List<AlignmentWithText>? databaseAlignments = null;
            if (request.totalsByTraining)
            {
                databaseAlignments = databaseAlignmentsQueryable
                    .Select(e => new AlignmentWithText
                    {
                        AlignmentSetId = e.AlignmentSetId,
                        SourceTokenComponentId = e.SourceTokenComponentId,
                        TargetTokenComponentId = e.TargetTokenComponentId,
                        AlignmentOriginatedFrom = e.AlignmentOriginatedFrom,
                        AlignmentVerification = e.AlignmentVerification,
                        SourceText = e.SourceTokenComponent!.TrainingText ?? string.Empty,
                        TargetText = e.TargetTokenComponent!.TrainingText ?? string.Empty
                    })
                    .ToList();
            }
            else
            {
                databaseAlignments = databaseAlignmentsQueryable
                    .Select(e => new AlignmentWithText
                    {
                        AlignmentSetId = e.AlignmentSetId,
                        SourceTokenComponentId = e.SourceTokenComponentId,
                        TargetTokenComponentId = e.TargetTokenComponentId,
                        AlignmentOriginatedFrom = e.AlignmentOriginatedFrom,
                        AlignmentVerification = e.AlignmentVerification,
                        SourceText = e.SourceTokenComponent!.SurfaceText ?? string.Empty,
                        TargetText = e.TargetTokenComponent!.SurfaceText ?? string.Empty
                    })
                    .ToList();
            }

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - Alignments+Tokens database query [count: {1}]", sw.Elapsed, databaseAlignments.Count);
            sw.Restart();
#endif
            databaseAlignments.ForEach(e => e.AlignmentTypeName = e.ToAlignmentType(request.AlignmentTypesToInclude).ToString());

            var filteredDatabaseAlignments = databaseAlignments                
                .WhereAlignmentTypesFilter(request.AlignmentTypesToInclude)
                .Cast<AlignmentWithText>();

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - Filtered alignments [count: {1}]", sw.Elapsed, filteredDatabaseAlignments.Count());
            sw.Restart();
#endif

            IDictionary<string, IDictionary<string, IDictionary<string, uint>>>? alignmentCountsByTrainingOrSurfaceText = default;

            if (request.SourceToTarget)
            {
                alignmentCountsByTrainingOrSurfaceText = filteredDatabaseAlignments
                    .GroupBy(e => e.SourceText)
                    .ToList()
                    .OrderByDescending(g => g.Count())
                    .ToDictionary(g => g.Key, g => g
                        .GroupBy(e => e.TargetText)
                        .OrderByDescending(g2 => g2.Count())
                        .ToDictionary(g2 => g2.Key, g2 => g2
                            .GroupBy(e => e.AlignmentTypeName)
                            .OrderByDescending (g3 => g3.Count())
                            .ToDictionary(g3 => g3.Key, g3 => (uint)g3.Count())
                        as IDictionary<string, uint>)
                    as IDictionary<string, IDictionary<string, uint>>);
            }
            else
            {
                alignmentCountsByTrainingOrSurfaceText = filteredDatabaseAlignments
                    .GroupBy(e => e.TargetText)
                    .ToList()
                    .OrderByDescending(g => g.Count())
                    .ToDictionary(g => g.Key, g => g
                        .GroupBy(e => e.SourceText)
                        .OrderByDescending(g2 => g2.Count())
                        .ToDictionary(g2 => g2.Key, g2 => g2
                            .GroupBy(e => e.AlignmentTypeName)
                            .OrderByDescending(g3 => g3.Count())
                            .ToDictionary(g3 => g3.Key, g3 => (uint)g3.Count())
                        as IDictionary<string, uint>)
                    as IDictionary<string, IDictionary<string, uint>>);
            }

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed {0}", sw.Elapsed);
#endif

            return new RequestResult<IDictionary<string, IDictionary<string, IDictionary<string, uint>>>>
            (
                alignmentCountsByTrainingOrSurfaceText
            );
        }
    }
}

