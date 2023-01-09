using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Tokenization;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class ParallelCorpus : EngineParallelTextCorpus, ICache
    {
        public ParallelCorpusId ParallelCorpusId { get; set; }

        public static async Task<IEnumerable<ParallelCorpusId>> 
            GetAllParallelCorpusIds( IMediator mediator)
        {
            var result = await mediator.Send(new GetAllParallelCorpusIdsQuery());
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }

        public EngineStringDetokenizer Detokenizer => ParallelCorpusId.SourceTokenizedCorpusId?.Detokenizer ?? new EngineStringDetokenizer(new LatinWordDetokenizer());

        public bool InvalidateSourceCache(string bookId)
        {
            return ((TokenizedTextCorpus)SourceCorpus).InvalidateCache(bookId);
        }

        public bool InvalidateTargetCache(string bookId)
        {
            return ((TokenizedTextCorpus)TargetCorpus).InvalidateCache(bookId);
        }

        /// <summary>
        /// Invalidates the cache on SourceCorpus and TargetCorpus.
        /// </summary>
        public void InvalidateCache()
        {
            ((TokenizedTextCorpus)SourceCorpus).InvalidateCache();
            ((TokenizedTextCorpus)TargetCorpus).InvalidateCache();
        }

        private bool useCache_;
        /// <summary>
        /// Gets use cache setting. 
        /// Set's UseCache property on both SourceCorpus and TargetCorpus.
        /// </summary>
        public bool UseCache
        {
            get
            {
                return useCache_;
            }
            set
            {
                useCache_ = value;
                ((TokenizedTextCorpus)SourceCorpus)
                    .UseCache = useCache_;
                ((TokenizedTextCorpus)TargetCorpus)
                    .UseCache = useCache_;
            }
        }

        public async Task Update(IMediator mediator, CancellationToken token = default)
        {
            var command = new UpdateParallelCorpusCommand(
                VerseMappingList ?? throw new InvalidParameterEngineException(name: "engineParallelTextCorpus.VerseMappingList", value: "null"),
                ParallelCorpusId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (ParallelCorpusId == null)
            {
                return;
            }

            await Delete(mediator, ParallelCorpusId, token);
        }

        public static async Task<ParallelCorpus> Get(
            IMediator mediator,
            ParallelCorpusId parallelCorpusId, 
            CancellationToken token = default,
            bool useCache = false)
        {
            var command = new GetParallelCorpusByParallelCorpusIdQuery(parallelCorpusId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            var data =  result.Data;
            return new ParallelCorpus(
                await TokenizedTextCorpus.Get(mediator, data.sourceTokenizedCorpusId, useCache), 
                await TokenizedTextCorpus.Get(mediator, data.targetTokenizedCorpusId, useCache), 
                data.verseMappings, 
                data.parallelCorpusId,
                useCache);
        }

        public static async Task Delete(
            IMediator mediator,
            ParallelCorpusId parallelCorpusId,
            CancellationToken token = default)
        {
            var command = new DeleteParallelCorpusByParallelCorpusIdCommand(parallelCorpusId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);
        }
        internal ParallelCorpus(
            TokenizedTextCorpus sourceTokenizedTextCorpus,
            TokenizedTextCorpus targetTokenizedTextCorpus,
            IEnumerable<VerseMapping> verseMappings,
            ParallelCorpusId parallelCorpusId,
            bool useCache )
            : base(sourceTokenizedTextCorpus, targetTokenizedTextCorpus, verseMappings.ToList())
        {
            ParallelCorpusId = parallelCorpusId;
            useCache_ = useCache;
        }
    }
}
