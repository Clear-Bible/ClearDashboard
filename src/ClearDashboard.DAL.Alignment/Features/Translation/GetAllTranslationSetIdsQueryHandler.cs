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
    public class GetAllTranslationSetIdsQueryHandler : ProjectDbContextQueryHandler<
        GetAllTranslationSetIdsQuery,
        RequestResult<IEnumerable<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId)>>,
        IEnumerable<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId)>>
    {

        public GetAllTranslationSetIdsQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAllTranslationSetIdsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId)>>> GetDataAsync(GetAllTranslationSetIdsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Models.TranslationSet> translationSets = ProjectDbContext.TranslationSets
                .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc.SourceTokenizedCorpus)
                .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc.TargetTokenizedCorpus);
            if (request.ParallelCorpusId != null)
            {
                translationSets = translationSets.Where(ts => ts.ParallelCorpusId == request.ParallelCorpusId.Id);
            }
            if (request.UserId != null)
            {
                translationSets = translationSets.Where(ts => ts.UserId == request.UserId.Id);
            }

            var translationSetIds = translationSets
                .AsEnumerable()   // To avoid error CS8143:  An expression tree may not contain a tuple literal
                .Select(ts => (ModelHelper.BuildTranslationSetId(ts), ModelHelper.BuildParallelCorpusId(ts.ParallelCorpus!)));

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId)>>( translationSetIds );
        }
    }


}
