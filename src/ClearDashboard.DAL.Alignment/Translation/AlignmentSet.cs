using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Translation;
using MediatR;
using System.Collections;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public class AlignmentSet
    {
        private readonly IMediator mediator_;

        public AlignmentSetId AlignmentSetId { get; }
        public ParallelCorpusId ParallelCorpusId { get; }

        public async Task<IEnumerable<Alignment>> GetAlignments(IEnumerable<EngineParallelTextRow> engineParallelTextRows)
        {
            var result = await mediator_.Send(new GetAlignmentsByAlignmentSetIdAndTokenIdsQuery(AlignmentSetId, engineParallelTextRows));
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async void PutAlignment(Alignment alignment)
        {
            var result = await mediator_.Send(new PutAlignmentSetAlignmentCommand(AlignmentSetId, alignment));
            if (result.Success)
            {
                return;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceTrainingText"></param>
        /// <returns>an enumerable of 
        /// unique trainingTargetText surfaceTargetText combinations representing one of the target words sourceTrainingText aligned to, and for each
        /// (A) an enumerable of tokens that comprise the targetVerseMap and (B) the indexes of the tokens with the trainingTargetText and surfaceTargetText</returns>
        public async Task<IEnumerable<(string trainingTargetText, string surfaceTargetText, IEnumerable<(IEnumerable<Token>, IEnumerable<int>)>)>> GetAlignedTokensAndContext(string sourceTrainingText)
        {
            var result = await mediator_.Send(new GetAlignedTokensAndContextQuery(sourceTrainingText));
            if (result.Success)
            {
                return result.Data!;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public IEnumerable<Token> GetTargetTokensBySourceTrainingText(string sourceTrainingText)
        { //CHRIS
            throw new NotImplementedException();
        }
        public async Task Update()
        {
            // call the update handler to update the r/w metadata on the TokenizedTextCorpusId
            throw new NotImplementedException();
        }

        public static async Task<IEnumerable<(AlignmentSetId alignmentSetId, ParallelCorpusId parallelCorpusId, UserId userId)>> 
            GetAllAlignmentSetIds(IMediator mediator, ParallelCorpusId? parallelCorpusId = null, UserId? userId = null)
        {
            var result = await mediator.Send(new GetAllAlignmentSetIdsQuery(parallelCorpusId, userId));
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<AlignmentSet> Get(
            AlignmentSetId alignmentSetId,
            IMediator mediator)
        {
            var command = new GetAlignmentSetByAlignmentSetIdQuery(alignmentSetId);

            var result = await mediator.Send(command);
            if (result.Success)
            {
                var data = result.Data;
                return new AlignmentSet(
                    data.alignmentSetId,
                    data.parallelCorpusId,
                    mediator);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<AlignmentSet> Create(
                IEnumerable<AlignedTokenPairs> alignedTokenPairs,
                string? displayName,
                string smtModel,
                bool isSyntaxTreeAlignerRefined,
                Dictionary<string, object> metadata,
                ParallelCorpusId parallelCorpusId,
                IMediator mediator)
        {
            var createTranslationSetCommandResult = await mediator.Send(new CreateAlignmentSetCommand(
                alignedTokenPairs.Select(a => new Alignment(a, "Unverified", "FromAlignmentModel")),
                displayName,
                smtModel,
                isSyntaxTreeAlignerRefined,
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

        internal AlignmentSet(
            AlignmentSetId alignmentSetId,
            ParallelCorpusId parallelCorpusId,
            IMediator mediator)
        {
            mediator_ = mediator;

            AlignmentSetId = alignmentSetId;
            ParallelCorpusId = parallelCorpusId;
        }
    }
}
