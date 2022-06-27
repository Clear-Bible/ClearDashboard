using ClearBible.Alignment.DataServices.Features.Corpora;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using MediatR;


namespace ClearBible.Alignment.DataServices.Corpora
{
    public class ParallelTokenizedCorpus : EngineParallelTextCorpus
    {
        public ParallelTokenizedCorpusId ParallelTokenizedCorpusId { get; set; }
        public ParallelCorpusVersionId ParallelCorpusVersionId { get; set; }
        public ParallelCorpusId ParallelCorpusId { get; set; }

        public static async Task<IEnumerable<(ParallelCorpusVersionId parallelCorpusVersionId, ParallelCorpusId parallelCorpusId)>> 
            GetAllParallelCorpusVersionIds(IMediator mediator)
        {
            var result = await mediator.Send(new GetAllParallelCorpusVersionIdsQuery());
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<IEnumerable<ParallelTokenizedCorpusId>> GetAllParallelTokenizedCorpusIds(IMediator mediator, ParallelCorpusVersionId parallelCorpusVersionId)
        {
            var result = await mediator.Send(new GetAllParallelTokenizedCorpusIdsByParallelCorpusVersionIdQuery(parallelCorpusVersionId));
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
        
        public static async Task<ParallelTokenizedCorpus> Get(
            IMediator mediator,
            ParallelTokenizedCorpusId parallelTokenizedCorpusId)
        {
            var command = new GetParallelTokenizedCorpusByParallelTokenizedCorpusIdQuery(parallelTokenizedCorpusId);

            var result = await mediator.Send(command);
            if (result.Success)
            {
                var data =  result.Data;
                return new ParallelTokenizedCorpus(
                    await TokenizedTextCorpus.Get(mediator, data.sourceTokenizedCorpusId), 
                    await TokenizedTextCorpus.Get(mediator, data.targetTokenizedCorpusId), 
                    data.engineVerseMappings, 
                    parallelTokenizedCorpusId,
                    data.parallelCorpusVersionId,
                    data.parallelCorpusId);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        internal ParallelTokenizedCorpus(
            TokenizedTextCorpus sourceTokenizedTextCorpus,
            TokenizedTextCorpus targetTokenizedTextCorpus,
            IEnumerable<EngineVerseMapping> engineVerseMappings,
            ParallelTokenizedCorpusId parallelTokenizedCorpusId,
            ParallelCorpusVersionId parallelCorpusVersionId,
            ParallelCorpusId parallelCorpusId)
            : base(sourceTokenizedTextCorpus, targetTokenizedTextCorpus, engineVerseMappings.ToList())
        {
            ParallelTokenizedCorpusId = parallelTokenizedCorpusId;
            ParallelCorpusVersionId = parallelCorpusVersionId;
            ParallelCorpusId = parallelCorpusId;
        }
    }
}
