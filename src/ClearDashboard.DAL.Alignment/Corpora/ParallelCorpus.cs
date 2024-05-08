using Autofac;
using ClearApi.Command.CQRS.Commands;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using System.Threading;

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
        protected List<ParallelTextRow>? ParallelTextRowsCache { get; set; }

        public override IEnumerator<ParallelTextRow> GetEnumerator()
        {
            if (UseCache)
            {
                IEnumerable<T> EnumeratorToEnumerable<T>(IEnumerator<T> enumerator)
                {
                    while (enumerator.MoveNext())
                        yield return enumerator.Current;
                }

                if (ParallelTextRowsCache == null)
                {
                    ParallelTextRowsCache = EnumeratorToEnumerable(base.GetEnumerator()).ToList();
                }
                return ParallelTextRowsCache.GetEnumerator();
            }
            else
            {
                return base.GetEnumerator();
            }
        }

        public static async Task<IEnumerable<ParallelCorpusId>> 
            GetAllParallelCorpusIdsAsync(IComponentContext context, CancellationToken cancellationToken)
        {
			return await new GetAllParallelCorpusIdsQuery()
				.ExecuteAsProjectCommandAsync(context, cancellationToken);
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

        /// <summary>
        /// To update versemappings, property SourceTextIdToVerseMappings must be set to an 
        /// instance that is not a SourceTextIdToVerseMappingsFromDatabase and implements SourceTextIdToVerseMappings.GetVerseMappings(). If this
        /// property is set to such an instance, this method 
        /// 1. deletes all versemappings for this ParallelCorpus in the database 
        /// 2. inserts those returned by SourcetextIdToVerseMappings.GetVerseMappings(),
        /// 3. sets property SourceTextIdToVerseMappings back to a SourceTextIdToVerseMappingsFromDatabase.
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task UpdateAsync(IComponentContext context, CancellationToken token = default)
        {
            var verseMappingsToUpdate = (SourceTextIdToVerseMappings is not SourceTextIdToVerseMappingsFromDatabase)
                ? SourceTextIdToVerseMappings?.GetVerseMappings() ?? Enumerable.Empty<VerseMapping>()
                : Enumerable.Empty<VerseMapping>();

            await new UpdateParallelCorpusCommand(
                verseMappingsToUpdate,
                ParallelCorpusId)
                .ExecuteAsProjectCommandAsync(context, token);

            SourceTextIdToVerseMappings = new SourceTextIdToVerseMappingsFromDatabase(context, ParallelCorpusId);
        }

        public async Task DeleteAsync(IComponentContext context, CancellationToken token = default)
        {
            if (ParallelCorpusId == null)
            {
                return;
            }

            await DeleteAsync(context, ParallelCorpusId, token);
        }

        public static async Task<ParallelCorpus> GetAsync(
            IComponentContext context,
            ParallelCorpusId parallelCorpusId, 
            CancellationToken token = default,
            bool useCache = false)
        {
			var data = 
                await new GetParallelCorpusByParallelCorpusIdQuery(parallelCorpusId)
                    .ExecuteAsProjectCommandAsync(context, token);

            return new ParallelCorpus(
                await TokenizedTextCorpus.GetAsync(context, data.sourceTokenizedCorpusId, useCache, token), 
                await TokenizedTextCorpus.GetAsync(context, data.targetTokenizedCorpusId, useCache, token), 
                new SourceTextIdToVerseMappingsFromDatabase(context, data.parallelCorpusId), 
                data.parallelCorpusId,
                useCache);
        }

        public static async Task DeleteAsync(
            IComponentContext context,
            ParallelCorpusId parallelCorpusId,
            CancellationToken token = default)
        {
            await new DeleteParallelCorpusByParallelCorpusIdCommand(parallelCorpusId)
                .ExecuteAsProjectCommandAsync(context, token);
        }
        public ParallelCorpus(
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
    }
}
