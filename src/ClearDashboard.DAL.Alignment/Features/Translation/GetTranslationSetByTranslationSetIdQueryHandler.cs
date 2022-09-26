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
    public class GetTranslationSetByTranslationSetIdQueryHandler : ProjectDbContextQueryHandler<
        GetTranslationSetByTranslationSetIdQuery,
        RequestResult<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId)>,
        (TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId)>
    {

        public GetTranslationSetByTranslationSetIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetTranslationSetByTranslationSetIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId)>> GetDataAsync(GetTranslationSetByTranslationSetIdQuery request, CancellationToken cancellationToken)
        {
            var translationSet = ProjectDbContext.TranslationSets
                .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc!.SourceTokenizedCorpus)
                        .ThenInclude(tc => tc!.User)
                .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc!.TargetTokenizedCorpus)
                        .ThenInclude(tc => tc!.User)
                .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc.User)
                .Include(ts => ts.User)
                .Where(ts => ts.Id == request.TranslationSetId.Id)
                .FirstOrDefault();
            if (translationSet == null)
            {
                return new RequestResult<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId)>
                (
                    success: false,
                    message: $"TranslationSet not found for TranslationSetId '{request.TranslationSetId.Id}'"
                );
            }

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId)>
            ((
                ModelHelper.BuildTranslationSetId(translationSet),
                ModelHelper.BuildParallelCorpusId(translationSet.ParallelCorpus!)
            ));
        }
    }


}
