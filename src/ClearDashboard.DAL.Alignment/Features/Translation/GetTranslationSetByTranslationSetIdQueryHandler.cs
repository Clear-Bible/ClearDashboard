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
        RequestResult<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, AlignmentSetId alignmentSetId, bool usingTranslationModel)>,
        (TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, AlignmentSetId alignmentSetId, bool usingTranslationModel)>
    {

        public GetTranslationSetByTranslationSetIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetTranslationSetByTranslationSetIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, AlignmentSetId alignmentSetId, bool usingTranslationModel)>> GetDataAsync(GetTranslationSetByTranslationSetIdQuery request, CancellationToken cancellationToken)
        {
            var translationSet = ModelHelper.AddIdIncludesTranslationSetsQuery(ProjectDbContext)
                .Include(ts => ts.AlignmentSet)
                    .ThenInclude(ast => ast!.User)
                .Where(ts => ts.Id == request.TranslationSetId.Id)
                .FirstOrDefault();

            var usingTranslationModel = (ProjectDbContext!.TranslationModelEntries
                .Where(tme => tme.TranslationSetId == request.TranslationSetId.Id).Count() > 0);

                if (translationSet == null)
            {
                return new RequestResult<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, AlignmentSetId alignmentSetId, bool UsingTranslationModel)>
                (
                    success: false,
                    message: $"TranslationSet not found for TranslationSetId '{request.TranslationSetId.Id}'"
                );
            }

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            var parallelCorpusId = ModelHelper.BuildParallelCorpusId(translationSet.ParallelCorpus!);

            return new RequestResult<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, AlignmentSetId alignmentSetId, bool UsingTranslationModel)>
            ((
                ModelHelper.BuildTranslationSetId(translationSet, parallelCorpusId, translationSet.User!),
                parallelCorpusId,
                ModelHelper.BuildAlignmentSetId(translationSet.AlignmentSet!, parallelCorpusId, translationSet.AlignmentSet!.User!),
                usingTranslationModel
            ));
        }
    }


}
