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
        private Dictionary<string, Dictionary<string, double>> TranslationModel { get; }

        public Dictionary<string, Dictionary<string, double>> GetTranslationModel()
        {
            return TranslationModel;
        }

        public async void PutTranslationModelEntry(string sourceText, Dictionary<string, double> targetTranslationTextScores)
        {
            TranslationModel[sourceText] = targetTranslationTextScores;

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

        public async Task<IEnumerable<Translation>> GetTranslations(TokenId firstTokenId, TokenId lastTokenId)
        {
            var result = await mediator_.Send(new GetTranslationsByTranslationSetIdAndTokenIdRangeQuery(TranslationSetId, firstTokenId, lastTokenId));
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async void PutTranslation(Translation translation, string translationActionType)
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
                    data.translationModel,
                    mediator);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        internal TranslationSet(
            TranslationSetId translationSetId,
            ParallelCorpusId parallelCorpusId,
            Dictionary<string, Dictionary<string, double>> translationModel,
            IMediator mediator)
        {
            mediator_ = mediator;

            TranslationSetId = translationSetId;
            ParallelCorpusId = parallelCorpusId;
            TranslationModel = translationModel;
        }
    }
}
