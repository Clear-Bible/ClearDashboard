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
        RequestResult<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, Dictionary<string, Dictionary<string, double>> translationModel)>,
        (TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, Dictionary<string, Dictionary<string, double>> translationModel)>
    {

        public GetTranslationSetByTranslationSetIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetTranslationSetByTranslationSetIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, Dictionary<string, Dictionary<string, double>> translationModel)>> GetDataAsync(GetTranslationSetByTranslationSetIdQuery request, CancellationToken cancellationToken)
        {
            var translationSet = ProjectDbContext.TranslationSets
                .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc!.SourceTokenizedCorpus)
                .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc!.TargetTokenizedCorpus)
                .Include(ts => ts.TranslationModel)
                    .ThenInclude(tme => tme.TargetTextScores)
                .Where(ts => ts.Id == request.TranslationSetId.Id)
                .FirstOrDefault();
            if (translationSet == null)
            {
                return new RequestResult<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, Dictionary<string, Dictionary<string, double>> translationModel)>
                (
                    success: false,
                    message: $"TranslationSet not found for TranslationSetId '{request.TranslationSetId.Id}'"
                );
            }

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            var translationModelData = 
                translationSet.TranslationModel
                    .ToDictionary(tme => tme.SourceText!, tme => tme.TargetTextScores
                        .ToDictionary(tts => tts.Text!, tts => tts.Score));

            return new RequestResult<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, Dictionary<string, Dictionary<string, double>> translationModel)>
            ((
                ModelHelper.BuildTranslationSetId(translationSet),
                ModelHelper.BuildParallelCorpusId(translationSet.ParallelCorpus!),
                translationModelData
            ));
        }
    }


}
