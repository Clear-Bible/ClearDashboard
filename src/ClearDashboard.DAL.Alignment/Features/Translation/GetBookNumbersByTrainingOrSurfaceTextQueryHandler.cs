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
    public class GetBookNumbersByTrainingOrSurfaceTextQueryHandler : ProjectDbContextQueryHandler<
        GetBookNumbersByTrainingOrSurfaceTextQuery,
        RequestResult<IEnumerable<int>>,
        IEnumerable<int>>
    {
        public GetBookNumbersByTrainingOrSurfaceTextQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetBookNumbersByTrainingOrSurfaceTextQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<int>>> GetDataAsync(GetBookNumbersByTrainingOrSurfaceTextQuery request, CancellationToken cancellationToken)
        {
            var alignmentSet = await ProjectDbContext!.AlignmentSets
                .Include(e => e.ParallelCorpus)
                .FirstOrDefaultAsync(ts => ts.Id == request.AlignmentSetId.Id);

            if (alignmentSet == null)
            {
                return new RequestResult<IEnumerable<int>>
                (
                    success: false,
                    message: $"AlignmentSet not found for AlignmentSetId '{request.AlignmentSetId.Id}'"
                );
            }

#if DEBUG
            Stopwatch sw = new();
            sw.Start();
#endif

            var databaseAlignmentsWithTokensQueryable = ProjectDbContext.Alignments
                .Include(e => e.SourceTokenComponent)
                .Include(e => e.TargetTokenComponent)
                .Where(e => e.AlignmentSetId == alignmentSet.Id)
                .Where(e => e.SourceTokenComponent!.GetType() == typeof(Models.Token))
                .Where(e => e.Deleted == null);

            if (request.StringsAreTraining)
            {
                databaseAlignmentsWithTokensQueryable = databaseAlignmentsWithTokensQueryable
                    .Where(e => e.SourceTokenComponent!.TrainingText == request.SourceString)
                    .Where(e => e.TargetTokenComponent!.TrainingText == request.TargetString);
            }
            else
            {
                databaseAlignmentsWithTokensQueryable = databaseAlignmentsWithTokensQueryable
                    .Where(e => e.SourceTokenComponent!.SurfaceText == request.SourceString)
                    .Where(e => e.TargetTokenComponent!.SurfaceText == request.TargetString);
            }

            var tokenBookNumbers = databaseAlignmentsWithTokensQueryable
                .Select(e => ((Models.Token)e.SourceTokenComponent!).BookNumber)
                .ToList();

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - BookNumbers from Alignment source tokens [count: {1}]", sw.Elapsed, tokenBookNumbers.Count);
            sw.Restart();
#endif

            var databaseAlignmentsWithTokenCompositesQueryable = ProjectDbContext.Alignments
                .Include(e => e.SourceTokenComponent!)
                    .ThenInclude(e => ((Models.TokenComposite)e).Tokens)
                .Include(e => e.TargetTokenComponent!)
                    .ThenInclude(e => ((Models.TokenComposite)e).Tokens)
                .Where(e => e.AlignmentSetId == alignmentSet.Id)
                .Where(e => e.SourceTokenComponent!.GetType() == typeof(Models.TokenComposite))
                .Where(e => e.Deleted == null);

            if (request.StringsAreTraining)
            {
                databaseAlignmentsWithTokenCompositesQueryable = databaseAlignmentsWithTokenCompositesQueryable
                    .Where(e => e.SourceTokenComponent!.TrainingText == request.SourceString)
                    .Where(e => e.TargetTokenComponent!.TrainingText == request.TargetString);
            }
            else
            {
                databaseAlignmentsWithTokenCompositesQueryable = databaseAlignmentsWithTokenCompositesQueryable
                    .Where(e => e.SourceTokenComponent!.SurfaceText == request.SourceString)
                    .Where(e => e.TargetTokenComponent!.SurfaceText == request.TargetString);
            }

            var compositeBookNumbers = databaseAlignmentsWithTokenCompositesQueryable
                .SelectMany(e => ((Models.TokenComposite)e.SourceTokenComponent!).Tokens.Select(e => e.BookNumber))
                .ToList();

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - BookNumbers from Alignment source composites [count: {1}]", sw.Elapsed, compositeBookNumbers.Count);
            sw.Restart();
#endif



#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed {0}", sw.Elapsed);
#endif

            return new RequestResult<IEnumerable<int>>
            (
                tokenBookNumbers.Union(compositeBookNumbers).Distinct().OrderBy(e => e)
            );
        }
    }
}

