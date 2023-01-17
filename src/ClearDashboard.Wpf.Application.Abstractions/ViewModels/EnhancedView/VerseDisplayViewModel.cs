using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using MediatR;
using Microsoft.Extensions.Logging;

// These need to be specified explicitly to resolve ambiguity with ClearDashboard.DataAccessLayer.Models.
using Token = ClearBible.Engine.Corpora.Token;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    /// <summary>
    /// A abstract view model class containing the needed information to render a verse of <see cref="Token"/>s in the UI.
    /// </summary>
    /// <remarks>
    /// Instantiate a concrete <see cref="CorpusDisplayViewModel"/>, <see cref="InterlinearDisplayViewModel"/>,
    /// or <see cref="AlignmentDisplayViewModel"/> depending on the functionality desired.
    /// </remarks>
    public abstract class VerseDisplayViewModel : PropertyChangedBase, 
        IHandle<SelectionUpdatedMessage>,
        IHandle<NoteAddedMessage>,
        IHandle<NoteDeletedMessage>,
        IHandle<NoteMouseEnterMessage>,
        IHandle<NoteMouseLeaveMessage>,
        IHandle<TokensJoinedMessage>,
        IHandle<TokenUnjoinedMessage>
    {
        protected NoteManager NoteManager { get; }
        protected IMediator Mediator { get; }
        protected IEventAggregator EventAggregator { get; }
        protected ILifetimeScope LifetimeScope { get; }
        protected ILogger Logger { get; }

        private TokenMap? _sourceTokenMap;
        protected TokenMap? SourceTokenMap
        {
            get => _sourceTokenMap;
            set
            {
                Set(ref _sourceTokenMap, value);
                NotifyOfPropertyChange(nameof(IsSourceRtl));
            }
        }

        private TokenMap? _targetTokenMap;
        protected TokenMap? TargetTokenMap
        {
            private get { return _targetTokenMap; }
            set
            {
                Set(ref _targetTokenMap, value);
                NotifyOfPropertyChange(nameof(IsTargetRtl));
            }
        }

        /// <summary>
        /// Gets the <see cref="ParallelCorpusId"/> for the verse.
        /// </summary>
        public ParallelCorpusId? ParallelCorpusId { get; protected set; }


        /// <summary>
        /// Gets a collection of source <see cref="TokenDisplayViewModel"/>s to be rendered.
        /// </summary>
        public TokenDisplayViewModelCollection SourceTokenDisplayViewModels { get; private set; } = new();

        /// <summary>
        /// Gets a collection of target <see cref="TokenDisplayViewModel"/>s to be rendered.
        /// </summary>
        public TokenDisplayViewModelCollection TargetTokenDisplayViewModels { get; private set; } = new();

        /// <summary>
        /// Gets a collection of alignments to be rendered.
        /// </summary>
        /// <remarks>
        /// Alignment information is only available when this is a <see cref="AlignmentDisplayViewModel"/> derived type.
        /// </remarks>
        public virtual AlignmentCollection? Alignments => null;

        public bool IsSourceRtl => SourceTokenMap?.IsRtl ?? false;
        public bool IsTargetRtl => TargetTokenMap?.IsRtl ?? false;

        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Get the <see cref="Translation"/> for a specified token.
        /// </summary>
        /// <remarks>
        /// This will be null unless called from an <see cref="InterlinearDisplayViewModel"/> containing a valid <see cref="TranslationSet"/>.
        /// </remarks>
        /// <param name="token">The <see cref="Token"/> for which to obtain a translation.</param>
        /// <param name="compositeToken">An optional <see cref="CompositeToken"/> that <paramref name="token"/> is a constituent of.</param>
        /// <returns>A <see cref="Translation"/> for the token if a valid <see cref="TranslationSet"/> is known; null otherwise.</returns>
        protected virtual Translation? GetTranslationForToken(Token token, CompositeToken? compositeToken)
        {
            return null;
        }

        protected async Task BuildTokenDisplayViewModelsAsync()
        {
            if (SourceTokenMap != null)
            {
                SourceTokenDisplayViewModels = await BuildTokenDisplayViewModelsAsync(SourceTokenMap, true);
                NotifyOfPropertyChange(nameof(SourceTokenDisplayViewModels));
            }

            if (TargetTokenMap != null)
            {
                TargetTokenDisplayViewModels = await BuildTokenDisplayViewModelsAsync(TargetTokenMap, false);
                NotifyOfPropertyChange(nameof(SourceTokenDisplayViewModels));
            }
        }
        
        private async Task<TokenDisplayViewModelCollection> BuildTokenDisplayViewModelsAsync(TokenMap tokenMap, bool isSource)
        {
            var result = new TokenDisplayViewModelCollection();
            
            foreach (var (token, paddingBefore, paddingAfter) in tokenMap.PaddedTokens)
            {
                var compositeToken = tokenMap.GetCompositeToken(token);
                result.Add(new TokenDisplayViewModel(token)
                {
                    VerseDisplay = this,
                    CompositeToken = compositeToken,
                    PaddingBefore = paddingBefore,
                    PaddingAfter = paddingAfter,
                    Translation = GetTranslationForToken(token, compositeToken),
                    NoteIds = await NoteManager.GetNoteIdsAsync(token.TokenId),
                    IsSource = isSource,
                });
            }
            return result;
        }

        public void MatchingTokenAction(IEnumerable<IId> entityIds, Action<TokenDisplayViewModel> action)
        {
            var entityIdList = entityIds.ToList();
            SourceTokenDisplayViewModels.MatchingTokenAction(entityIdList, action);
            TargetTokenDisplayViewModels.MatchingTokenAction(entityIdList, action);
        }

        public void MatchingTokenAction(Func<TokenDisplayViewModel, bool> conditional, Action<TokenDisplayViewModel> action)
        {
            SourceTokenDisplayViewModels.MatchingTokenAction(conditional, action);
            TargetTokenDisplayViewModels.MatchingTokenAction(conditional, action);
        }

        public void NonMatchingTokenAction(IEnumerable<IId> entityIds, Action<TokenDisplayViewModel> action)
        {
            var entityIdList = entityIds.ToList();
            SourceTokenDisplayViewModels.NonMatchingTokenAction(entityIdList, action);
            TargetTokenDisplayViewModels.NonMatchingTokenAction(entityIdList, action);
        }

        public async Task HandleAsync(SelectionUpdatedMessage message, CancellationToken cancellationToken)
        {
            NonMatchingTokenAction(message.SelectedTokens.TokenIds, t => t.IsTokenSelected = false);
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteAddedMessage message, CancellationToken cancellationToken)
        {
            MatchingTokenAction(message.EntityIds, t => t.NoteAdded(message.Note));
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteDeletedMessage message, CancellationToken cancellationToken)
        {
            MatchingTokenAction(message.EntityIds, t => t.NoteDeleted(message.Note));
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteMouseEnterMessage message, CancellationToken cancellationToken)
        {
            MatchingTokenAction(t => message.Note.Associations.Any(a => a.AssociatedEntityId.IdEquals(t.Token.TokenId)), t => t.IsNoteHovered = true);
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteMouseLeaveMessage message, CancellationToken cancellationToken)
        {
            MatchingTokenAction(t => message.Note.Associations.Any(a => a.AssociatedEntityId.IdEquals(t.Token.TokenId)), t => t.IsNoteHovered = false);
            await Task.CompletedTask;
        }

        protected virtual async Task InitializeAsync()
        {
            await BuildTokenDisplayViewModelsAsync();
        }

        protected VerseDisplayViewModel(NoteManager noteManager, IMediator mediator, IEventAggregator eventAggregator, ILifetimeScope lifetimeScope, ILogger logger)
        {
            NoteManager = noteManager;
            Mediator = mediator;
            EventAggregator = eventAggregator;
            LifetimeScope = lifetimeScope;
            Logger = logger;

            EventAggregator.SubscribeOnUIThread(this);
        }

        public async Task HandleAsync(TokensJoinedMessage message, CancellationToken cancellationToken)
        {
            MatchingTokenAction(message.Tokens.TokenIds, t => t.CompositeToken = message.CompositeToken);
            await RefreshTranslationsAsync(message.Tokens, message.CompositeToken);
            //Task.Run(() => RefreshTranslationsAsync(message.CompositeToken, message.Tokens).GetAwaiter(), cancellationToken);
            //return Task.CompletedTask;
        }

        public async Task HandleAsync(TokenUnjoinedMessage message, CancellationToken cancellationToken)
        {
            MatchingTokenAction(message.Tokens.TokenIds, t => t.CompositeToken = null);
            await RefreshTranslationsAsync(message.Tokens);
            //Task.Run(() => RefreshTranslationsAsync(message.CompositeToken, message.Tokens).GetAwaiter(), cancellationToken);
            //return Task.CompletedTask;
        }

        protected virtual async Task RefreshTranslationsAsync(TokenCollection tokens, CompositeToken? compositeToken = null)
        {
            await Task.CompletedTask;
        }
    }
}
