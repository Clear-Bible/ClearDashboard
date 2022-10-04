using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetAllAlignmentSetIdsQueryHandler : ProjectDbContextQueryHandler<
        GetAllAlignmentSetIdsQuery,
        RequestResult<IEnumerable<(AlignmentSetId translationSetId, ParallelCorpusId parallelCorpusId, UserId userId)>>,
        IEnumerable<(AlignmentSetId translationSetId, ParallelCorpusId parallelCorpusId, UserId userId)>>
    {

        public GetAllAlignmentSetIdsQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAllAlignmentSetIdsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<(AlignmentSetId translationSetId, ParallelCorpusId parallelCorpusId, UserId userId)>>> GetDataAsync(GetAllAlignmentSetIdsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Models.AlignmentSet> alignmentSets = ProjectDbContext.AlignmentSets
                .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc!.SourceTokenizedCorpus)
                        .ThenInclude(tc => tc!.User)
                .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc!.TargetTokenizedCorpus)
                        .ThenInclude(tc => tc!.User)
                .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc!.User)
                .Include(ts => ts.User);
            if (request.ParallelCorpusId != null)
            {
                alignmentSets = alignmentSets.Where(ast => ast.ParallelCorpusId == request.ParallelCorpusId.Id);
            }
            if (request.UserId != null)
            {
                alignmentSets = alignmentSets.Where(ast => ast.UserId == request.UserId.Id);
            }

            var alignmentSetIds = alignmentSets
                .AsEnumerable()   // To avoid error CS8143:  An expression tree may not contain a tuple literal
                .Select(ast => (
                    ModelHelper.BuildAlignmentSetId(ast), 
                    ModelHelper.BuildParallelCorpusId(ast.ParallelCorpus!),
                    ModelHelper.BuildUserId(ast.User!)));

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<(AlignmentSetId alignmentSetId, ParallelCorpusId parallelCorpusId, UserId userId)>>( alignmentSetIds.ToList() );
        }
    }


}
