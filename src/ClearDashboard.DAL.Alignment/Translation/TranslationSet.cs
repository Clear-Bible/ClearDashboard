using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.Alignment.Features.Translation;
using ClearDashboard.DAL.CQRS;
using MediatR;
using System.Collections;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public class TranslationSet
    {
        private readonly IMediator mediator_;
        public TranslationSetId TranslationSetId { get; }
        public ParallelCorpusId ParallelCorpusId { get; }

        public AlignmentSetId AlignmentSetId { get; set; }
        protected bool UsingTranslationModel { get; }

        /*
        private async void PutTranslationModel(Dictionary<string, Dictionary<string, double>> translationModel, string smtModel)
        {
            // Put the model (save to db and set the TranslationModel property)
            // Put the smtModel property on the ID and update
            throw new NotImplementedException();
        }
        */

        public async Task<Dictionary<string, double>?> GetTranslationModelEntryForToken(Token token, AlignmentTypes alignmentTypesToInclude = AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded)
        {
            if (UsingTranslationModel)
            {
                var result = await mediator_.Send(new GetTranslationSetModelEntryQuery(TranslationSetId, token.TrainingText));
                result.ThrowIfCanceledOrFailed();

                return result.Data;
            }
            else
            {
                var alignmentSet = await AlignmentSet.Get(AlignmentSetId, mediator_);
                var matchingTargetTokens = await alignmentSet.GetTargetTokensBySourceTrainingText(token.TrainingText, alignmentTypesToInclude);
                return matchingTargetTokens
                    .Select(t => t.TrainingText)
                    .GroupBy(t => t)
                    .OrderByDescending(g => g.Count())
                    .ToDictionary(g => g.Key, g => (double)g.Count());
            }
        }

        /*
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

        */
        public async Task<IEnumerable<Translation>> GetTranslations(IEnumerable<EngineParallelTextRow> engineParallelTextRow, AlignmentTypes alignmentTypesToInclude = AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded, CancellationToken token = default)
        {
            return await GetTranslations(engineParallelTextRow.SelectMany(e => e.SourceTokens!.Select(st => st.TokenId)), alignmentTypesToInclude, token);
        }

        public async Task<IEnumerable<Translation>> GetTranslations(IEnumerable<TokenId> sourceTokenIds, AlignmentTypes alignmentTypesToInclude = AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded, CancellationToken token = default)
        {
            // alignmentTypesToInclude argument is used when alignment denormalization data is not available:
            var result = await mediator_.Send(new GetTranslationsByTranslationSetIdAndTokenIdsQuery(TranslationSetId, sourceTokenIds, alignmentTypesToInclude), token);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="translationActionType">Valid values are:  "PutPropagate", "PutNoPropagate"</param>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public async Task PutTranslation(Translation translation, string translationActionType, CancellationToken token = default)
        {
            var result = await mediator_.Send(new PutTranslationSetTranslationCommand(TranslationSetId, translation, translationActionType), token);
            result.ThrowIfCanceledOrFailed();
        }

        public async void Update(CancellationToken token = default)
        {
            // call the update handler to update the r/w metadata on the TokenizedTextCorpusId and also this.AlignmentSetId
        }

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (TranslationSetId == null)
            {
                return;
            }

            await Delete(mediator, TranslationSetId, token);
        }

        public static async Task<IEnumerable<TranslationSetId>> 
            GetAllTranslationSetIds(IMediator mediator, ParallelCorpusId? parallelCorpusId = null, UserId? userId = null)
        {
            var result = await mediator.Send(new GetAllTranslationSetIdsQuery(parallelCorpusId, userId));
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!.Select(e => e.translationSetId);
        }

        public static async Task<TranslationSet> Get(
            TranslationSetId translationSetId,
            IMediator mediator)
        {
            var command = new GetTranslationSetByTranslationSetIdQuery(translationSetId);

            var result = await mediator.Send(command);
            result.ThrowIfCanceledOrFailed(true);

            var data = result.Data;
            return new TranslationSet(
                data.translationSetId,
                data.parallelCorpusId,
                data.alignmentSetId,
                data.usingTranslationModel,
                mediator);
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
            Dictionary<string, Dictionary<string, double>>? translationModel,
            AlignmentSetId alignmentSetId,
            string? displayName,
            //string smtModel,
            Dictionary<string, object> metadata,
            ParallelCorpusId parallelCorpusId,
            IMediator mediator,
            CancellationToken token = default)
        {
            var createTranslationSetCommandResult = await mediator.Send(new CreateTranslationSetCommand(
                translationModel,
                alignmentSetId,
                displayName,
                //smtModel,
                metadata,
            parallelCorpusId), token);

            createTranslationSetCommandResult.ThrowIfCanceledOrFailed(true);

            return createTranslationSetCommandResult.Data!;
        }

        public static async Task Delete(
            IMediator mediator,
            TranslationSetId translationSetId,
            CancellationToken token = default)
        {
            var command = new DeleteTranslationSetByTranslationSetIdCommand(translationSetId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);
        }

        internal TranslationSet(
            TranslationSetId translationSetId,
            ParallelCorpusId parallelCorpusId,
            AlignmentSetId alignmentSetId,
            bool usingTranslationModel,
            IMediator mediator)
        {
            mediator_ = mediator;

            TranslationSetId = translationSetId;
            ParallelCorpusId = parallelCorpusId;
            AlignmentSetId = alignmentSetId;
            UsingTranslationModel = usingTranslationModel;
        }
    }
}
