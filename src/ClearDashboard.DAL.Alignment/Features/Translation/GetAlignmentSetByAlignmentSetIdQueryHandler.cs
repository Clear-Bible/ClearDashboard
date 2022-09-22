using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetAlignmentSetByAlignmentSetIdQueryHandler : ProjectDbContextQueryHandler<
        GetAlignmentSetByAlignmentSetIdQuery,
        RequestResult<(AlignmentSetId alignmentSetId, ParallelCorpusId parallelCorpusId)>,
        (AlignmentSetId alignmentSetId, ParallelCorpusId parallelCorpusId)>
    {

        public GetAlignmentSetByAlignmentSetIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAlignmentSetByAlignmentSetIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<(AlignmentSetId alignmentSetId, ParallelCorpusId parallelCorpusId)>> GetDataAsync(GetAlignmentSetByAlignmentSetIdQuery request, CancellationToken cancellationToken)
        {
            var alignmentSet = ProjectDbContext.AlignmentSets
                .Include(ast => ast.ParallelCorpus)
                    .ThenInclude(pc => pc!.SourceTokenizedCorpus)
                .Include(ast => ast.ParallelCorpus)
                    .ThenInclude(pc => pc!.TargetTokenizedCorpus)
                .Where(ast => ast.Id == request.AlignmentSetId.Id)
                .FirstOrDefault();
            if (alignmentSet == null)
            {
                return new RequestResult<(AlignmentSetId alignmentSetId, ParallelCorpusId parallelCorpusId)>
                (
                    success: false,
                    message: $"AlignmentSet not found for AlignmentSetId '{request.AlignmentSetId.Id}'"
                );
            }

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<(AlignmentSetId alignmentSetId, ParallelCorpusId parallelCorpusId)>
            ((
                ModelHelper.BuildAlignmentSetId(alignmentSet),
                ModelHelper.BuildParallelCorpusId(alignmentSet.ParallelCorpus!)
            ));
        }
    }


}
