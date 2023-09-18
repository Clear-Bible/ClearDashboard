using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Translation;
using ClearDashboard.DAL.CQRS;
using MediatR;
using System.Collections;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public class AlignmentSet
    {
        private readonly IMediator mediator_;

        public AlignmentSetId AlignmentSetId { get; }
        public ParallelCorpusId ParallelCorpusId { get; }

        public async Task<IEnumerable<Alignment>> GetAlignments(IEnumerable<EngineParallelTextRow> engineParallelTextRows, AlignmentTypes alignmentTypes = AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded, CancellationToken token = default)
        {
            var result = await mediator_.Send(new GetAlignmentsByAlignmentSetIdAndTokenIdsQuery(AlignmentSetId, engineParallelTextRows, alignmentTypes), token);
            result.ThrowIfCanceledOrFailed(true);
            
            return result.Data!;
        }

        public async Task PutAlignment(Alignment alignment, CancellationToken token = default)
        {
            var result = await mediator_.Send(new PutAlignmentSetAlignmentCommand(AlignmentSetId, alignment), token);
            result.ThrowIfCanceledOrFailed();

            var alignmentId = result.Data!;
            alignment.AlignmentId = alignmentId;
        }

        public async Task DeleteAlignment(AlignmentId alignmentId, CancellationToken token = default)
        {
            var result = await mediator_.Send(new DeleteAlignmentByAlignmentIdCommand(alignmentId), token);
            result.ThrowIfCanceledOrFailed();
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
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }

        public async Task<IEnumerable<Token>> GetTargetTokensBySourceTrainingText(string sourceTrainingText, AlignmentTypes alignmentTypesToInclude = AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded)
        {
            var result = await mediator_.Send(new GetAlignmentSetTargetTokensBySourceTrainingTextQuery(AlignmentSetId, sourceTrainingText, alignmentTypesToInclude));
            result.ThrowIfCanceledOrFailed(true);
 
            return result.Data!;
        }

        /// <summary>
        /// For machine generated auto alignments, only return the count of Alignments that don’t 
        /// also have alignmentpairs between the same tokens for other alignment types.
        /// </summary>
        /// <param name="sourceToTarget"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Dictionary<sourceString, Dictionary<targetString, Dictionary<statusName, count>> if 
        /// sourceToTarget == true, reverse 'sourceString' and 'targetString' if false</returns>
        public async Task<IDictionary<string, IDictionary<string, (IDictionary<string, uint> StatusCounts, string BookNumbers)>>> GetAlignmentCounts(bool sourceToTarget, bool totalsByTraining = true, bool includeBookNumbers = false, AlignmentTypes alignmentTypesToInclude = AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded, CancellationToken cancellationToken = default)
        {
            var result = await mediator_.Send(new GetAlignmentCountsByTrainingOrSurfaceTextQuery(AlignmentSetId, sourceToTarget, totalsByTraining, includeBookNumbers, alignmentTypesToInclude), cancellationToken);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }

        public async Task<(IEnumerable<(
            Alignment alignment,
            IEnumerable<Token> sourceVerseTokens,
            uint sourceVerseTokensIndex,
            IEnumerable<Token> targetVerseTokens,
            uint targetVerseTokensIndex
            )> VerseContexts, string Cursor, bool HasNextPage)> GetAlignmentVerseContexts(string sourceString, string targetString, bool stringsAreTraining = true, int? bookNumber = null, AlignmentTypes alignmentTypesToInclude = AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded, int? limit = null, string? cursor = null, CancellationToken cancellationToken = default)
        {
            var result = await mediator_.Send(new GetAlignmentVerseContextsQuery(
                AlignmentSetId,
                sourceString,
                targetString,
                stringsAreTraining,
                bookNumber,
                alignmentTypesToInclude,
                limit,
                cursor), cancellationToken);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data;
        }

        public async Task<IEnumerable<int>> GetBookNumbers(string sourceString, string targetString, bool stringsAreTraining = true, AlignmentTypes alignmentTypesToInclude = AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded, CancellationToken cancellationToken = default)
        {
            var result = await mediator_.Send(new GetBookNumbersByTrainingOrSurfaceTextQuery(
                AlignmentSetId, 
                sourceString, 
                targetString,
                stringsAreTraining,
                alignmentTypesToInclude), cancellationToken);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }

        public async Task PutAlignments(IEnumerable<Alignment> alignments, CancellationToken token = default)
        {
            var result = await mediator_.Send(new PutAlignmentSetAlignmentsCommand(AlignmentSetId, alignments), token);
            result.ThrowIfCanceledOrFailed();
        }

        public async Task Update(CancellationToken token = default)
        {
            // call the update handler to update the r/w metadata on the TokenizedTextCorpusId
            throw new NotImplementedException();
        }

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (AlignmentSetId == null)
            {
                return;
            }

            await Delete(mediator, AlignmentSetId, token);
        }

        public static async Task<IEnumerable<AlignmentSetId>> 
            GetAllAlignmentSetIds(IMediator mediator, ParallelCorpusId? parallelCorpusId = null, UserId? userId = null)
        {
            var result = await mediator.Send(new GetAllAlignmentSetIdsQuery(parallelCorpusId, userId));
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!.Select(e => e.alignmentSetId);
        }

        public static async Task<AlignmentSet> Get(
            AlignmentSetId alignmentSetId,
            IMediator mediator)
        {
            var command = new GetAlignmentSetByAlignmentSetIdQuery(alignmentSetId);

            var result = await mediator.Send(command);
            result.ThrowIfCanceledOrFailed(true);

            var data = result.Data;
            return new AlignmentSet(
                data.alignmentSetId,
                data.parallelCorpusId,
                mediator);
        }

        public static async Task<AlignmentSet> Create(
                IEnumerable<AlignedTokenPairs> alignedTokenPairs,
                string? displayName,
                string smtModel,
                bool isSyntaxTreeAlignerRefined,
                bool isSymmetrized,
                Dictionary<string, object> metadata,
                ParallelCorpusId parallelCorpusId,
                IMediator mediator,
                CancellationToken token = default)
        {
            var createTranslationSetCommandResult = await mediator.Send(new CreateAlignmentSetCommand(
                alignedTokenPairs.Select(a => new Alignment(a, "Unverified", "FromAlignmentModel")),
                displayName,
                smtModel,
                isSyntaxTreeAlignerRefined,
                isSymmetrized,
                metadata,
            parallelCorpusId), token);

            createTranslationSetCommandResult.ThrowIfCanceledOrFailed(true);

            return createTranslationSetCommandResult.Data!;
        }

        public static async Task Delete(
            IMediator mediator,
            AlignmentSetId alignmentSetId,
            CancellationToken token = default)
        {
            var command = new DeleteAlignmentSetByAlignmentSetIdCommand(alignmentSetId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);
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
