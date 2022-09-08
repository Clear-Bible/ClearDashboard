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

        public async Task<IEnumerable<Alignment>> GetAlignments(IEnumerable<EngineParallelTextRow> engineParallelTextRow)
        {
            var sourceTokenIds = engineParallelTextRow.SelectMany(e => e.SourceTokens!.Select(st => st.TokenId));
            var result = await mediator_.Send(new GetAlignmentsByAlignmentSetIdAndTokenIdsQuery(AlignmentSetId, sourceTokenIds));
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

        public async void Update()
        {
            // call the update handler to update the r/w metadata on the TokenizedTextCorpusId
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
