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

namespace ClearDashboard.Wpf.Application.Services
{
    public sealed class VerseManager : PropertyChangedBase
    {
        private IEventAggregator EventAggregator { get; }
        private ILogger<VerseManager> Logger { get; }
        private IMediator Mediator { get; }

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
        public async Task UnjoinTokenAsync(CompositeToken compositeToken, ParallelCorpusId? parallelCorpusId)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var tokens = new TokenCollection(compositeToken.Tokens);
                compositeToken.Tokens = new List<Token>();
                await TokenizedTextCorpus.PutCompositeToken(Mediator, compositeToken, parallelCorpusId);

                stopwatch.Stop();
                Logger.LogInformation($"Unjoined composite token {compositeToken.TokenId.Id} into {tokens.Count} in {stopwatch.ElapsedMilliseconds} ms");

                await EventAggregator.PublishOnUIThreadAsync(new TokenUnjoinedMessage(compositeToken, tokens));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public VerseManager(IEventAggregator eventAggregator, ILogger<VerseManager> logger, IMediator mediator)
        {
            EventAggregator = eventAggregator;
            Logger = logger;
            Mediator = mediator;
        }
    }
}
