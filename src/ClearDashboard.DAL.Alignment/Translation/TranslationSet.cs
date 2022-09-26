using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Translation;
using MediatR;
using System.Collections;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public class TranslationSet
    {
        private readonly IMediator mediator_;

        public TranslationSetId TranslationSetId { get; }
        public ParallelCorpusId ParallelCorpusId { get; }

        private async void PutTranslationModel(Dictionary<string, Dictionary<string, double>> translationModel, string smtModel)
        {
            // Put the model (save to db and set the TranslationModel property)
            // Put the smtModel property on the ID and update
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, double>?> GetTranslationModelEntryForToken(Token token)
        {
            var result = await mediator_.Send(new GetTranslationSetModelEntryQuery(TranslationSetId, token.TrainingText));
            if (result.Success)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async void PutTranslationModelEntry(string sourceText, Dictionary<string, double> targetTranslationTextScores)
        {
            var result = await mediator_.Send(new PutTranslationSetModelEntryCommand(TranslationSetId, sourceText, targetTranslationTextScores));
            if (result.Success)
            {
                return;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async Task<IEnumerable<Translation>> GetTranslations(IEnumerable<EngineParallelTextRow> engineParallelTextRow)
        {
            return await GetTranslations(engineParallelTextRow.SelectMany(e => e.SourceTokens!.Select(st => st.TokenId)));
        }

        public async Task<IEnumerable<Translation>> GetTranslations(IEnumerable<TokenId> sourceTokenIds)
        {
            var result = await mediator_.Send(new GetTranslationsByTranslationSetIdAndTokenIdsQuery(TranslationSetId, sourceTokenIds));
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="translationActionType">Valid values are:  "PutPropagate", "PutNoPropagate"</param>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public async Task PutTranslation(Translation translation, string translationActionType)
        {
            var result = await mediator_.Send(new PutTranslationSetTranslationCommand(TranslationSetId, translation, translationActionType));
            if (result.Success)
            {
                return;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async void Update()
        {
            // call the update handler to update the r/w metadata on the TokenizedTextCorpusId
        }

        public static async Task<IEnumerable<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, UserId userId)>> 
            GetAllTranslationSetIds(IMediator mediator, ParallelCorpusId? parallelCorpusId = null, UserId? userId = null)
        {
            var result = await mediator.Send(new GetAllTranslationSetIdsQuery(parallelCorpusId, userId));
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<TranslationSet> Get(
            TranslationSetId translationSetId,
            IMediator mediator)
        {
            var command = new GetTranslationSetByTranslationSetIdQuery(translationSetId);

            var result = await mediator.Send(command);
            if (result.Success)
            {
                var data = result.Data;
                return new TranslationSet(
                    data.translationSetId,
                    data.parallelCorpusId,
                    mediator);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="translationModel"></param>
        /// <param name="parallelCorpusId"></param>
        /// <param name="mediator"></param>
        /// <returns></returns>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public static async Task<TranslationSet> Create(
            Dictionary<string, Dictionary<string, double>> translationModel,
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

        internal TranslationSet(
            TranslationSetId translationSetId,
            ParallelCorpusId parallelCorpusId,
            IMediator mediator)
        {
            mediator_ = mediator;

            TranslationSetId = translationSetId;
            ParallelCorpusId = parallelCorpusId;
        }
    }
}
