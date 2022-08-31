using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;

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
            var translationSetIds = (request.ParallelCorpusId == null)
                ? ProjectDbContext.TranslationSets.AsEnumerable()
                    .Select(ts => (new TranslationSetId(ts.Id), new ParallelCorpusId(ts.ParallelCorpusId)))
                : ProjectDbContext.TranslationSets
                    .Where(ts => ts.ParallelCorpusId == request.ParallelCorpusId.Id).AsEnumerable()
                    .Select(ts => (new TranslationSetId(ts.Id), new ParallelCorpusId(ts.ParallelCorpusId)));

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId)>>( translationSetIds );
        }
    }


}
