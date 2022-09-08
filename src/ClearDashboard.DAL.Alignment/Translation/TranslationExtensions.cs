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
            var createTranslationSetCommandResult = await mediator.Send(new CreateTranslationSetCommand(
                translationModel,
                displayName,
                smtModel,
                metadata,
                parallelCorpusId));

            if (createTranslationSetCommandResult.Success && createTranslationSetCommandResult.Data != null)
            {
                return createTranslationSetCommandResult.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(createTranslationSetCommandResult.Message);
            }
        }
    }
}
