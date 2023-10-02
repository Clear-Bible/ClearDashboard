using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Wpf.Application.Services
{
    public sealed class VerseManager : PropertyChangedBase
    {
        private IEventAggregator EventAggregator { get; }
        private ILogger<VerseManager> Logger { get; }
        private IMediator Mediator { get; }
        public SelectionManager SelectionManager { get; }

        /// <summary>
        /// Joins a sequence of tokens into a new <see cref="CompositeToken"/> instance.
        /// </summary>
        /// <param name="tokens">The sequence of tokens to join.</param>
        /// <param name="parallelCorpusId">An optional <see cref="ParallelCorpusId"/> for the CompositeToken.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task JoinTokensAsync(TokenCollection tokens, ParallelCorpusId? parallelCorpusId)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var compositeToken = new CompositeToken(tokens);
                await TokenizedTextCorpus.PutCompositeToken(Mediator, compositeToken, parallelCorpusId);

                stopwatch.Stop();
                Logger.LogInformation($"Joined {tokens.Count} tokens into composite token {compositeToken.TokenId.Id} in {stopwatch.ElapsedMilliseconds} ms");

                await EventAggregator.PublishOnUIThreadAsync(new TokensJoinedMessage(compositeToken, tokens));
                SelectionManager.SelectionUpdated();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Unjoins a <see cref="CompositeToken"/> into its constituent tokens.
        /// </summary>
        /// <param name="compositeToken">The CompositeToken to unjoin.</param>
        /// <param name="parallelCorpusId">An optional <see cref="ParallelCorpusId"/> for the CompositeToken.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task UnjoinTokenAsync(CompositeToken? compositeToken, ParallelCorpusId? parallelCorpusId)
        {
            try
            {
                if (compositeToken == null)
                {
                    Logger.LogCritical($"Cannot un-join tokens - the '{nameof(compositeToken)}' parameter cannot be null.");
                    return;
                }

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var tokens = new TokenCollection(compositeToken.Tokens);
                compositeToken.Tokens = new List<ClearBible.Engine.Corpora.Token>();
                await TokenizedTextCorpus.PutCompositeToken(Mediator, compositeToken, parallelCorpusId);

                stopwatch.Stop();
                Logger.LogInformation($"Unjoined composite token {compositeToken.TokenId.Id} into {tokens.Count} in {stopwatch.ElapsedMilliseconds} ms");

                await EventAggregator.PublishOnUIThreadAsync(new TokenUnjoinedMessage(compositeToken, tokens));
                SelectionManager.SelectionUpdated();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An unexpected error occurred while un-joining tokens.");
                throw;
            }
        }

        public async Task SplitToken(TokenizedTextCorpus corpus, TokenId tokenId, int tokenIndex1, int tokenIndex2, string trainingText1, string trainingText2, string? trainingText3, bool createParallelComposite = true)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = await corpus.SplitTokens(Mediator, new List<TokenId> { tokenId }, tokenIndex1, tokenIndex2, trainingText1, trainingText2, trainingText3, createParallelComposite);

                stopwatch.Stop();
                Logger.LogInformation($"Split token {tokenId.Id} into in {stopwatch.ElapsedMilliseconds} ms");

                //await EventAggregator.PublishOnUIThreadAsync(new TokenUnjoinedMessage(compositeToken, tokens));
                SelectionManager.SelectionUpdated();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An unexpected error occurred while un-joining tokens.");
                throw;
            }
        }

        public VerseManager(IEventAggregator eventAggregator, 
            ILogger<VerseManager> logger, 
            IMediator mediator,
            SelectionManager selectionManager)
        {
            EventAggregator = eventAggregator;
            Logger = logger;
            Mediator = mediator;
            SelectionManager = selectionManager;
        }
    }
}
