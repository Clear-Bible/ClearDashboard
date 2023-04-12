using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using System.Collections;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class ParallelCorpus : EngineParallelTextCorpus, ICache
    {
        public ParallelCorpusId ParallelCorpusId { get; set; }

        public new SourceTextIdToVerseMappings? SourceTextIdToVerseMappings
        {
            get => base.SourceTextIdToVerseMappings;
            set
            {
                if (UseCache)
                    throw new EngineException("cannot set this property because UseCache is true");
                else
                    base.SourceTextIdToVerseMappings = value;
            }
        }

        public new ScrVers? SourceCorpusVersification
        {
            get => base.SourceCorpusVersification;
            set
            {
                if (UseCache)
                    throw new EngineException("cannot set this property because UseCache is true");
                else
                    base.SourceCorpusVersification = value;
            }
        }


        public override IEnumerable<string>? LimitToSourceBooks
        {
            get => base.LimitToSourceBooks;
            set
            {
                var isChanged = 
                    (base.LimitToSourceBooks == null && value != null) ||
                    (base.LimitToSourceBooks != null && value == null) ||
                    (base.LimitToSourceBooks != null && value != null && base.LimitToSourceBooks.Intersect(value).Count() > 0);
                if (UseCache && ParallelTextRowsCache != null && isChanged)
                    ParallelTextRowsCache = null;
                else
                    base.LimitToSourceBooks = value;
            }
        }

        public new bool AllSourceRows
        {
            get => base.AllSourceRows;
            set
            {
                if (UseCache)
                    throw new EngineException("cannot set this property because UseCache is true");
                else
                    base.AllSourceRows = value;
            }
        }
        public new bool AllTargetRows
        {
            get => base.AllTargetRows;
            set
            {
                if (UseCache)
                    throw new EngineException("cannot set this property because UseCache is true");
                else
                    base.AllTargetRows = value;
            }
        }

        public new ITextCorpus SourceCorpus
        {
            get => base.SourceCorpus;
            set
            {
                if (UseCache)
                    throw new EngineException("cannot set this property because UseCache is true");
                else
                    base.SourceCorpus = value;
            }
        }
        public new ITextCorpus TargetCorpus
        {
            get => base.TargetCorpus;
            set
            {
                if (UseCache)
                    throw new EngineException("cannot set this property because UseCache is true");
                else
                    base.TargetCorpus = value;
            }
        }
        public new IAlignmentCorpus AlignmentCorpus
        {
            get => base.AlignmentCorpus;
            set
            {
                if (UseCache)
                    throw new EngineException("cannot set this property because UseCache is true");
                else
                    base.AlignmentCorpus = value;
            }
        }
        protected CachedEnumerable<ParallelTextRow>? ParallelTextRowsCache { get; set; }
        public override IEnumerator<ParallelTextRow> GetEnumerator()
        {
            if (UseCache)
            {
                ParallelTextRowsCache ??= new CachedEnumerable<ParallelTextRow>(base.GetEnumerator());
                return ParallelTextRowsCache.GetEnumerator();
            }
            else
            {
                return base.GetEnumerator();
            }
        }

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
            ParallelTextRowsCache = null;
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
            // FIXME (verify):  not sure API-wise of how the user is supposed to make changes to verse
            // mappings of a ParallelCorpus.  Since there aren't any verse mapping setters on the
            // SourceTextIdToVerseMappings interface, this assumes that in order to alter them, the UI
            // must create an instance of SourceTextIdToVerseMappingsFromVerseMappings that contains
            // the altered verse mappings.  
            var verseMappingsToUpdate = (SourceTextIdToVerseMappings is SourceTextIdToVerseMappingsFromVerseMappings)
                ? SourceTextIdToVerseMappings.GetVerseMappings()
                : Enumerable.Empty<VerseMapping>();

            var command = new UpdateParallelCorpusCommand(
                verseMappingsToUpdate,
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
                new SourceTextIdToVerseMappingsFromDatabase(mediator, data.parallelCorpusId), 
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
            SourceTextIdToVerseMappings sourceTextIdToVerseMappings,
            ParallelCorpusId parallelCorpusId,
            bool useCache )
            : base(sourceTokenizedTextCorpus, targetTokenizedTextCorpus, sourceTextIdToVerseMappings)
        {
            ParallelCorpusId = parallelCorpusId;
            UseCache = useCache;
            ParallelTextRowsCache = null;
        }

        protected class CachedEnumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerator<T> _enumerator;
            private readonly List<T> _cache = new();

            public CachedEnumerable(IEnumerator<T> enumerator) => this._enumerator = enumerator;

            public IEnumerator<T> GetEnumerator()
            {
                int index = 0;

                while (true)
                {
                    if (index < _cache.Count)
                    {
                        yield return _cache[index];
                        index++;
                    }
                    else
                    {
                        if (_enumerator.MoveNext())
                        {
                            _cache.Add(_enumerator.Current);
                        }
                        else
                        {
                            yield break;
                        }
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
