using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class ParallelCorpus : EngineParallelTextCorpus
    {
        public ParallelCorpusId ParallelCorpusId { get; set; }

        public static async Task<IEnumerable<ParallelCorpusId>> 
            GetAllParallelCorpusIds( IMediator mediator)
        {
            var result = await mediator.Send(new GetAllParallelCorpusIdsQuery());
            if (result.Success && result.Data != null)
            {
                return result.Data;
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

        public static async Task<ParallelCorpus> Get(
            IMediator mediator,
            ParallelCorpusId parallelCorpusId)
        {
            var command = new GetParallelCorpusByParallelCorpusIdQuery(parallelCorpusId);

            var result = await mediator.Send(command);
            if (result.Success)
            {
                var data =  result.Data;
                return new ParallelCorpus(
                    await TokenizedTextCorpus.Get(mediator, data.sourceTokenizedCorpusId), 
                    await TokenizedTextCorpus.Get(mediator, data.targetTokenizedCorpusId), 
                    data.verseMappings, 
                    data.parallelCorpusId);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        internal ParallelCorpus(
            TokenizedTextCorpus sourceTokenizedTextCorpus,
            TokenizedTextCorpus targetTokenizedTextCorpus,
            IEnumerable<VerseMapping> verseMappings,
            ParallelCorpusId parallelCorpusId)
            : base(sourceTokenizedTextCorpus, targetTokenizedTextCorpus, verseMappings.ToList())
        {
            ParallelCorpusId = parallelCorpusId;
        }
    }
}
