using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetAlignmentCountsByTrainingOrSurfaceTextQueryHandler : ProjectDbContextQueryHandler<
        GetAlignmentCountsByTrainingOrSurfaceTextQuery,
        RequestResult<IDictionary<string, IDictionary<string, (IDictionary<string, uint> StatusCounts, string BookNumbers)>>>,
        IDictionary<string, IDictionary<string, (IDictionary<string, uint> StatusCounts, string BookNumbers)>>>
    {

        public class AlignmentWithText : Models.Alignment
        {
            public string SourceText { get; set; } = string.Empty;
            public string TargetText { get; set; } = string.Empty;
            public string AlignmentTypeName { get; set; } = string.Empty;
            public int? TokenBookNumber { get; set; }
            public IEnumerable<int>? CompositeBookNumbers { get; set; } = null;
        }

        public GetAlignmentCountsByTrainingOrSurfaceTextQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAlignmentCountsByTrainingOrSurfaceTextQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IDictionary<string, IDictionary<string, (IDictionary<string, uint> StatusCounts, string BookNumbers)>>>> GetDataAsync(GetAlignmentCountsByTrainingOrSurfaceTextQuery request, CancellationToken cancellationToken)
        {
            var alignmentSet = await ProjectDbContext!.AlignmentSets
                .FirstOrDefaultAsync(ts => ts.Id == request.AlignmentSetId.Id);

            if (alignmentSet == null)
            {
                return new RequestResult<IDictionary<string, IDictionary<string, (IDictionary<string, uint> StatusCounts, string BookNumbers)>>>
                (
                    success: false,
                    message: $"AlignmentSet not found for AlignmentSetId '{request.AlignmentSetId.Id}'"
                );
            }

#if DEBUG
            Stopwatch sw = new();
            sw.Start();
#endif

            IQueryable<Models.Alignment> databaseAlignmentsQueryable = request.includeBookNumbers
                ? ProjectDbContext.Alignments
                    .Include(e => e.SourceTokenComponent!)
                        .ThenInclude(e => ((Models.TokenComposite)e).Tokens)
                : ProjectDbContext.Alignments
                    .Include(e => e.SourceTokenComponent);

            databaseAlignmentsQueryable = databaseAlignmentsQueryable
                .Include(e => e.TargetTokenComponent!)
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
                        TargetText = e.TargetTokenComponent!.TrainingText ?? string.Empty,
                        TokenBookNumber = request.includeBookNumbers ? ((Models.Token)e.SourceTokenComponent!).BookNumber : null,
                        CompositeBookNumbers = request.includeBookNumbers ? ((Models.TokenComposite)e.SourceTokenComponent!).Tokens.Select(t => t.BookNumber) : null
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
                        TargetText = e.TargetTokenComponent!.SurfaceText ?? string.Empty,
                        TokenBookNumber = request.includeBookNumbers ? ((Models.Token)e.SourceTokenComponent!).BookNumber : null,
                        CompositeBookNumbers = request.includeBookNumbers ? ((Models.TokenComposite)e.SourceTokenComponent!).Tokens.Select(t => t.BookNumber) : null
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

            IDictionary<string, IDictionary<string, (IDictionary<string, uint>, string)>>? alignmentCountsByTrainingOrSurfaceText = default;

            if (request.SourceToTarget)
            {
                alignmentCountsByTrainingOrSurfaceText = filteredDatabaseAlignments
                    .GroupBy(e => e.SourceText)
                    .ToList()
                    .OrderByDescending(g => g.Count())
                    .ToDictionary(g => g.Key, g => g
                        .GroupBy(e => e.TargetText)
                        .OrderByDescending(g2 => g2.Count())
                        .ToDictionary(g2 => g2.Key, g2 => (g2
                            .GroupBy(e => e.AlignmentTypeName)
                            .OrderByDescending(g3 => g3.Count())
                            .ToDictionary(g3 => g3.Key, g3 => (uint)g3.Count()) as IDictionary<string, uint>, GetBookNumbersAsDelimitedString(g2.AsEnumerable())))
                    as IDictionary<string, (IDictionary<string, uint>, string)>);
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
                        .ToDictionary(g2 => g2.Key, g2 => (g2
                            .GroupBy(e => e.AlignmentTypeName)
                            .OrderByDescending(g3 => g3.Count())
                            .ToDictionary(g3 => g3.Key, g3 => (uint)g3.Count()) as IDictionary<string, uint>, GetBookNumbersAsDelimitedString(g2.AsEnumerable())))
                    as IDictionary<string, (IDictionary<string, uint>, string)>);
            }

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed {0}", sw.Elapsed);
#endif

            return new RequestResult<IDictionary<string, IDictionary<string, (IDictionary<string, uint> StatusCounts, string BookNumbers)>>>
            (
                alignmentCountsByTrainingOrSurfaceText
            );
        }

        private static string GetBookNumbersAsDelimitedString(IEnumerable<AlignmentWithText> alignmentsWithText)
        {
            var bookNumbers = new List<int>();
            foreach (var alignmentWithText in alignmentsWithText)
            {
                if (alignmentWithText.TokenBookNumber is not null && alignmentWithText.TokenBookNumber.HasValue)
                    bookNumbers.Add(alignmentWithText.TokenBookNumber.Value);

                if (alignmentWithText.CompositeBookNumbers is not null && alignmentWithText.CompositeBookNumbers.Any())
                    bookNumbers.AddRange(alignmentWithText.CompositeBookNumbers);
            }
            return string.Join(',', bookNumbers.Distinct().OrderBy(e => e));
        }
    }
}

