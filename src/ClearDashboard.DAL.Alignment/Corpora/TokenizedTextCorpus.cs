using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using Models = ClearDashboard.DataAccessLayer.Models;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;
using ClearDashboard.DAL.Alignment.Features.Translation;
using Autofac;
using ClearApi.Command.CQRS.Commands;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearApi.Command.Commands;
using ClearApi.Command.CQRS.CommandReceivers;
using ClearDashboard.DAL.CQRS.Features;
using System.Threading;
using ClearDashboard.DAL.CQRS;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class TokenizedTextCorpus : ScriptureTextCorpus, ICache
    {
        public readonly static Dictionary<Models.CorpusType, Guid> FixedTokenizedCorpusIdsByCorpusType = new() {
            { Models.CorpusType.ManuscriptHebrew, Guid.Parse("3D275D10-5374-4649-8D0D-9E69281E5B83") },
            { Models.CorpusType.ManuscriptGreek, Guid.Parse("3D275D10-5374-4649-8D0D-9E69281E5B84") }
        };

        public TokenizedTextCorpusId TokenizedTextCorpusId { get; set; }
        public bool NonTokenized { get; }

        private bool useCache_;

        /// <summary>
        /// Gets use cache setting. 
        /// Set's UseCache property on all TokenizedText.
        /// </summary>
        public bool UseCache { 
            get
            {
                return useCache_;
            }
            set
            {
                useCache_ = value;
                TextDictionary
                    .Select(kvp => ((TokenizedText)kvp.Value).UseCache = useCache_);
            }
        }

        /// <summary>
        /// Invalidates the cache on the TokenizedText for a specific bookId.
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public bool InvalidateCache(string bookId)
        {
            IText? text;
            var success = TextDictionary.TryGetValue(bookId, out text);
            if (success && text != null)
                ((TokenizedText)text).InvalidateCache();
            return success;
        }

        /// <summary>
        /// Invalidates the cache on all TokenizedText.
        /// </summary>
        public void InvalidateCache()
        {
            TextDictionary
                .Select(kvp =>
                {
                    ((TokenizedText)kvp.Value).InvalidateCache();
                    return true; //make linq happy
                });
        }

        public TokenizedTextCorpus(
            TokenizedTextCorpusId tokenizedCorpusId, 
            IComponentContext context, 
            IEnumerable<string> bookAbbreviations, 
            ScrVers versification, 
            bool useCache,
            bool nonTokenized)
        {
            useCache_ = useCache;
            NonTokenized = nonTokenized;
            TokenizedTextCorpusId = tokenizedCorpusId;
            Versification = versification;

            foreach (var bookAbbreviation in bookAbbreviations)
            {
                AddText(new TokenizedText(TokenizedTextCorpusId, context, Versification, bookAbbreviation, useCache, NonTokenized));
            }
        }

        public async Task UpdateOrAddVersesAsync(IComponentContext context, ITextCorpus textCorpus, CancellationToken token = default)
        {
            try
            {
                _ = textCorpus.Cast<TokensTextRow>();
            }
            catch (InvalidCastException)
            {
                throw new InvalidTypeEngineException(message: $"Corpus must be tokenized and transformed into TokensTextRows, e.g. corpus.Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");
            }

            var bookIdInfo = await new GetBookIdsByTokenizedCorpusIdQuery(TokenizedTextCorpusId)
                .ExecuteAsProjectCommandAsync(context, token);

            var bookAbbreviations = await new UpdateOrAddVersesInTokenizedCorpusCommand(TokenizedTextCorpusId, textCorpus, bookIdInfo.bookIds)
                .ExecuteAsProjectCommandAsync(context, token);

            foreach (var bookAbbreviation in bookAbbreviations)
            {
                AddText(new TokenizedText(TokenizedTextCorpusId, context, Versification, bookAbbreviation, UseCache, NonTokenized));
            }
        }

        public async Task DeleteVersesAsync(IComponentContext context, IEnumerable<VerseRef> verseRefs)
        {
            await Task.FromException(new NotImplementedException());
        }

        public async Task DeleteAsync(IComponentContext context, IEnumerable<string>? books = null)
        {
            await Task.FromException(new NotImplementedException());
        }

        public async Task UpdateAsync(IComponentContext context, CancellationToken token = default)
        {
            await new UpdateTokenizedCorpusCommand(TokenizedTextCorpusId)
                .ExecuteAsProjectCommandAsync(context, token);
        }

        public async Task ImportWordAnalysesAsync(IComponentContext context, CancellationToken token = default)
        {
            var wordAnalyses = await GetExternalWordAnalysesAsync(context, token);
            if (wordAnalyses.Any())
            {
                await new ImportWordAnalysesCommand(wordAnalyses, TokenizedTextCorpusId)
                    .ExecuteAsProjectCommandAsync(context, token);
			}

            if (UseCache)
            {
                InvalidateCache();
            }
		}

        public async Task<IEnumerable<WordAnalysis>> GetExternalWordAnalysesAsync(IComponentContext context, CancellationToken token = default)
        {
            var paratextProjectId = TokenizedTextCorpusId?.CorpusId?.ParatextGuid;
            if (paratextProjectId is not null)
            {
				var receiver = context.Resolve<IMediatorCommandReceiver<IRequest<RequestResult<IEnumerable<WordAnalysis>>>, IEnumerable<WordAnalysis>>>();
				var analyses = await receiver.RequestAsync(new GetExternalWordAnalysesQuery(paratextProjectId), token);

                return analyses;
            }

            return Enumerable.Empty<WordAnalysis>();
        }

		public static async Task<IEnumerable<TokenizedTextCorpusId>> GetAllTokenizedCorpusIdsAsync(IComponentContext context, CorpusId? corpusId, CancellationToken cancellationToken)
		{
            return await new GetAllTokenizedCorpusIdsByCorpusIdQuery(corpusId)
                .ExecuteAsProjectCommandAsync(context, cancellationToken);
		}

		public static async Task<TokenizedTextCorpusLocal> Get(
			IMediator mediator,
            TokenizedTextCorpusId tokenizedTextCorpusId,
            bool useCache = false,
            CancellationToken token = default)
        {
            var command = new GetBookIdsByTokenizedCorpusIdQuery(tokenizedTextCorpusId);

			var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            return new TokenizedTextCorpusLocal(result.Data.tokenizedTextCorpusId, mediator, result.Data.bookIds, result.Data.versification, useCache, false);
        }

		public static async Task<TokenizedTextCorpus> GetAsync(
			IComponentContext context,
			TokenizedTextCorpusId tokenizedTextCorpusId,
			bool useCache = false,
			CancellationToken token = default)
		{
			var result = await new GetBookIdsByTokenizedCorpusIdQuery(tokenizedTextCorpusId)
                .ExecuteAsProjectCommandAsync(context, token);

			return new TokenizedTextCorpus(result.tokenizedTextCorpusId, context, result.bookIds, result.versification, useCache, false);
		}

		public static async Task<TokenizedTextCorpus> GetNonTokenizedAsync(
			IComponentContext context,
            TokenizedTextCorpusId tokenizedTextCorpusId,
            bool useCache = false,
            bool nonTokenized = false)
        {
            var command = new GetBookIdsByTokenizedCorpusIdQuery(tokenizedTextCorpusId);

			var mediator = context.Resolve<IMediator>();
			var result = await mediator.Send(command);
            result.ThrowIfCanceledOrFailed(true);

            return new TokenizedTextCorpus(result.Data.tokenizedTextCorpusId, context, result.Data.bookIds, result.Data.versification, useCache, true);
        }

        public static async Task<(IEnumerable<Token> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex)> GetTokenVerseContext(ParallelCorpusId? parallelCorpusId, Token token, IMediator mediator, CancellationToken cancellationToken = default)
        {
            var command = new GetTokenVerseContextQuery(parallelCorpusId, token);

            var result = await mediator.Send(command, cancellationToken);
            result.ThrowIfCanceledOrFailed();

            return result.Data!;
        }

        /// <summary>
        /// If the incoming CompositeToken is new (not in the database yet) this method creates it
        /// 
        /// If the incoming CompositeToken already exists in the database and tokens have been added
        /// or removed from it, this saves those changes.  If it has any tokens that are part of 
        /// another Composite in the database, this method removes them from the other/previous one.  
        /// 
        /// If the incoming CompositeToken is empty and is already in the database, it is removed from
        /// the database.  
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="compositeToken"></param>
        /// <param name="parallelCorpusId"></param>
        /// <returns></returns>
        public static async Task PutCompositeToken(IMediator mediator, CompositeToken compositeToken, ParallelCorpusId? parallelCorpusId)
        {
            var command = new PutCompositeTokenCommand(compositeToken, parallelCorpusId);

            var result = await mediator.Send(command);
            result.ThrowIfCanceledOrFailed(true);
        }

        /// <summary>
        /// Returns a tuple of (1) CompositeTokens that group all of the newly created Tokens and (2) the newly created token. 
        /// Andy's corpus ui would substitute a CompositeToken(parallel=null) if it exists, or the returned Tokens if it doesn't 
        /// exist, for T1. Andy's parallel corpus UI would substitute CompositeToken(parallel=parallelId) for T1.
        /// </summary>
        /// <param name="tokenIdsWithSameSurfaceText">Split all occurrences of that same word (same surface text) throughout the corpus</param>
        /// <param name="surfaceTextIndex"></param>
        /// <param name="surfaceTextLength"></param>
        /// <param name="trainingText1"></param>
        /// <param name="trainingText2"></param>
        /// <param name="trainingText3">Can only be null if surfaceTextIndex > 0</param>
        /// <param name="createParallelComposite">Create parallel composite when tokenId is not a member of any composite 
        /// at all (for any parallel or non-parallel composite.</param>
        /// <param name="propagateTo">A non-"None" value will only take effect if there is a single token id in TokenIdsWithSameSurfaceText.
        /// Propagates, limited to tokens within the given scope that matches the specified token,
        /// to tokens in the same tokenized corpus having the same surface text</param>
        /// <returns></returns>
        public async Task<(IDictionary<TokenId, IEnumerable<CompositeToken>> SplitCompositeTokensByIncomingTokenId, IDictionary<TokenId, IEnumerable<Token>> SplitChildTokensByIncomingTokenId)> SplitTokens(
            IMediator mediator,
            IEnumerable<TokenId> tokenIdsWithSameSurfaceText,
            int surfaceTextIndex,
            int surfaceTextLength,
            string trainingText1,
            string trainingText2,
            string? trainingText3, 
            bool createParallelComposite = true,
            SplitTokenPropagationScope propagateTo = SplitTokenPropagationScope.None,
            CancellationToken cancellationToken = default
        )
        {
            var command = new SplitTokensCommand(
                TokenizedTextCorpusId,
                tokenIdsWithSameSurfaceText, 
                surfaceTextIndex,
                surfaceTextLength,
                trainingText1,
                trainingText2,
                trainingText3,
                createParallelComposite,
                propagateTo);

            var result = await mediator.Send(command, cancellationToken);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }

        public async Task<(IDictionary<TokenId, IEnumerable<CompositeToken>> SplitCompositeTokensByIncomingTokenId, IDictionary<TokenId, IEnumerable<Token>> SplitChildTokensByIncomingTokenId)> SplitTokensViaSplitInstructions(
            IMediator mediator,
            IEnumerable<TokenId> tokenIdsWithSameSurfaceText,
           SplitInstructions splitInstructions,
            bool createParallelComposite = true,
            SplitTokenPropagationScope propagateTo = SplitTokenPropagationScope.None,
            CancellationToken cancellationToken = default
        )
        {
            var command = new SplitTokensViaSplitInstructionsCommand(
                TokenizedTextCorpusId,
                tokenIdsWithSameSurfaceText,
                splitInstructions,
                createParallelComposite,
                propagateTo);

            var result = await mediator.Send(command, cancellationToken);
            result.ThrowIfCanceledOrFailed(true);

            if (UseCache)
            {
                InvalidateCache();
            }

            return result.Data!;
        }

        public async Task<IEnumerable<Token>> FindTokensBySurfaceTextAsync(
            IComponentContext context, 
            string searchString,
            WordPart wordPart = WordPart.Full, // or regex find predicate?
            bool ignoreCase = true,
            CancellationToken cancellationToken = default
        )
        {
            return await new FindTokensBySurfaceTextQuery(
				TokenizedTextCorpusId,
				searchString,
				wordPart,
				ignoreCase)
            .ExecuteAsProjectCommandAsync(context, cancellationToken);
        }
    }
}
