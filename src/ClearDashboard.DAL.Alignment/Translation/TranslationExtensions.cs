using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Translation;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public static class TranslationExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="translationModel"></param>
        /// <param name="parallelCorpusId"></param>
        /// <param name="mediator"></param>
        /// <returns></returns>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public static async Task<TranslationSet> Create(
            this Dictionary<string, Dictionary<string, double>> translationModel, 
                string? displayName,
                string smtModel,
                Dictionary<string, object> metadata,
                ParallelCorpusId parallelCorpusId, 
                IMediator mediator)
        {
            return await TranslationSet.Create(translationModel, displayName, smtModel, metadata, parallelCorpusId, mediator);
        }
    }
}
