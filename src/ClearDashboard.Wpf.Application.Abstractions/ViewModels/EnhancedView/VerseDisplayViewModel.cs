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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.Collections.Notes;

// These need to be specified explicitly to resolve ambiguity with ClearDashboard.DataAccessLayer.Models.
using Token = ClearBible.Engine.Corpora.Token;
using TokenId = ClearBible.Engine.Corpora.TokenId;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using SIL.Scripture;

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
        IHandle<TokenUnjoinedMessage>,
        IHandle<TokenSplitMessage>
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
            get => _targetTokenMap;
            set
            {
                Set(ref _targetTokenMap, value);
                NotifyOfPropertyChange(nameof(IsTargetRtl));
            }
        }

        public TokenizedTextCorpus? SourceCorpus => SourceTokenMap?.Corpus;
        public TokenizedTextCorpus? TargetCorpus => TargetTokenMap?.Corpus;

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

       
        public AlignmentManager? AlignmentManager { get; set; } = null;

        /// <summary>
        /// Gets a enumerable of source tokens.
        /// </summary>
        /// <remarks>
        /// Alignment information is only available when this is a <see cref="AlignmentDisplayViewModel"/> derived type.
        /// </remarks>
        protected virtual IEnumerable<Token>? GetSourceTokens(bool isSource, TokenId tokenId)
        {
            return null;
        }
        /// <summary>
        /// Gets a enumerable of target tokens.
        /// </summary>
        /// <remarks>
        /// Alignment information is only available when this is a <see cref="AlignmentDisplayViewModel"/> derived type.
        /// </remarks>
        protected virtual IEnumerable<Token>? GetTargetTokens(bool isSource, TokenId tokenId)
        {
            return null;
        }

        public virtual void SetExternalNotes(List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)> sourceTokenizedCorpusNotes,
            List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>? targetTokenizedCorpusNotes)
        {
        }
        protected void SetExternalNotesOnTokenDisplayViewModels(TokenDisplayViewModelCollection tokenDisplayViewModels, List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)> noteInfos)
        {
            foreach( var tokenDisplayViewModel in tokenDisplayViewModels)
            {
                var externalNotes = noteInfos
                    .Where(noteInfo => noteInfo.tokenIds?.Contains(tokenDisplayViewModel.Token.TokenId) ?? false)
                    .Select(noteInfo => noteInfo.externalNote)
                    .ToList();
                if (externalNotes != null && externalNotes.Count() > 0)
                {
                    tokenDisplayViewModel.ExternalNotes = externalNotes;
                }
            }
        }
        protected virtual void HighlightSourceTokens(bool isSource, TokenId tokenId)
        {
            var sourceTokens = GetSourceTokens(isSource, tokenId);
            if (sourceTokens == null)
            {
                return;
            }
            _ = SourceTokenDisplayViewModels
                .Select(tdm =>
                {
                    tdm.IsHighlighted = sourceTokens
                        .Select(t => t.TokenId)
                        .Contains(tdm.Token.TokenId, new IIdEqualityComparer());

                    return tdm;
                })
                .ToList();
        }

        protected virtual void HighlightTargetTokens(bool isSource, TokenId tokenId)
        {
             var targetTokens = GetTargetTokens(isSource, tokenId);
             if (targetTokens == null)
             {
                 return;
             }
             _ =  TargetTokenDisplayViewModels
                .Select(tdm =>
                {
                    tdm.IsHighlighted = targetTokens
                        .Select(t => t.TokenId)
                        .Contains(tdm.Token.TokenId, new IIdEqualityComparer());

                    return tdm;
                })
                .ToList();
        }


        public virtual async Task HighlightTokens(bool isSource, TokenId tokenId)
        {
            await EventAggregator.PublishOnUIThreadAsync(new HighlightTokensMessage(isSource, tokenId));
        }

        public async Task HighlightTokensAsync(HighlightTokensMessage message, CancellationToken cancellationToken)
        {
            HighlightSourceTokens(message.IsSource, message.TokenId);
            HighlightTargetTokens(message.IsSource, message.TokenId);
            await Task.CompletedTask;
        }

        public virtual async Task UnhighlightTokens()
        {
            await EventAggregator.PublishOnUIThreadAsync(new UnhighlightTokensMessage());
        }

        public virtual async Task InternalUnhighlightTokens()
        {

            _ = SourceTokenDisplayViewModels
                .Select(tdm =>
                {
                    tdm.IsHighlighted = false;
                    return tdm;
                })
                .ToList();

            _ = TargetTokenDisplayViewModels
                .Select(tdm =>
                {
                    tdm.IsHighlighted = false;
                    return tdm;
                })
                .ToList();

            await Task.CompletedTask;
        }

        public async Task UnhighlightTokensAsync(UnhighlightTokensMessage message, CancellationToken cancellationToken)
        {
            await InternalUnhighlightTokens();
        }

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

        /// <summary>
        /// Get the aligned token for a specified token.
        /// </summary>
        /// <remarks>
        /// This will be null unless called from an <see cref="BulkAlignmentDisplayViewModel"/> instance.
        /// </remarks>
        /// <param name="token">The <see cref="Token"/> for which to obtain the aligned token.</param>
        /// <param name="compositeToken">An optional <see cref="CompositeToken"/> that <paramref name="token"/> is a constituent of.</param>
        /// <returns>A <see cref="Token"/> for the aligned token if known; null otherwise.</returns>
        protected virtual Token? GetAlignedToken(Token token, CompositeToken? compositeToken)
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
                NotifyOfPropertyChange(nameof(TargetTokenDisplayViewModels));
            }
        }
        
        private async Task<TokenDisplayViewModelCollection> BuildTokenDisplayViewModelsAsync(TokenMap tokenMap, bool isSource)
        {
            var result = new TokenDisplayViewModelCollection();
            
            foreach (var (token, paddingBefore, paddingAfter) in tokenMap.PaddedTokens)
            {
                var compositeToken = tokenMap.GetCompositeToken(token);
                var tokenDisplayViewModel = new TokenDisplayViewModel(token)
                {
                    VerseDisplay = this,
                    CompositeToken = compositeToken,
                    PaddingBefore = paddingBefore,
                    PaddingAfter = paddingAfter,
                    Translation = GetTranslationForToken(token, compositeToken),
                    AlignedToken = GetAlignedToken(token, compositeToken),
                    TokenNoteIds = await NoteManager.GetNoteIdsAsync(token.TokenId),
                    IsSource = isSource,
                };
                if (tokenDisplayViewModel.Translation?.TranslationId != null)
                {
                    tokenDisplayViewModel.TranslationNoteIds = await NoteManager.GetNoteIdsAsync(tokenDisplayViewModel.Translation.TranslationId);
                }
                result.Add(tokenDisplayViewModel);
            }
            return result;
        }

        protected virtual async Task<NoteIdCollection> GetNoteIdsForToken(TokenId tokenId)
        {
            return await NoteManager.GetNoteIdsAsync(tokenId);
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
            foreach (var token in SourceTokenDisplayViewModels.Union(TargetTokenDisplayViewModels))
            {
                var matchingToken = message.SelectedTokens.FirstOrDefault(t => t.Token.TokenId.IdEquals(token.Token.TokenId));
                if (matchingToken == null)
                {
                    token.IsTokenSelected = false;
                    token.IsTranslationSelected = false;
                }
                else
                {
                    token.IsTokenSelected = matchingToken.IsTokenSelected;
                    token.IsTranslationSelected = matchingToken.IsTranslationSelected;
                }
            }
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteAddedMessage message, CancellationToken cancellationToken)
        {
            MatchingTokenAction(message.EntityIds.Where(e => e.GetType() == typeof(TokenId)), t => t.TokenNoteAdded(message.Note));
            MatchingTokenAction(message.EntityIds.Where(e => e.GetType() == typeof(TranslationId)), t => t.TranslationNoteAdded(message.Note));
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteDeletedMessage message, CancellationToken cancellationToken)
        {
            MatchingTokenAction(message.EntityIds.Where(e => e.GetType() == typeof(TokenId)), t => t.TokenNoteDeleted(message.Note));
            MatchingTokenAction(message.EntityIds.Where(e => e.GetType() == typeof(TranslationId)), t => t.TranslationNoteDeleted(message.Note));
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteMouseEnterMessage message, CancellationToken cancellationToken)
        {
            MatchingTokenAction(t => message.Note.Associations.Any(a => a.AssociatedEntityId.IdEquals(t.Token.TokenId) || a.AssociatedEntityId.IdEquals(t.Translation?.TranslationId)), t => t.IsNoteHovered = true);
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteMouseLeaveMessage message, CancellationToken cancellationToken)
        {
            MatchingTokenAction(t => message.Note.Associations.Any(a => a.AssociatedEntityId.IdEquals(t.Token.TokenId) || a.AssociatedEntityId.IdEquals(t.Translation?.TranslationId)), t => t.IsNoteHovered = false);
            await Task.CompletedTask;
        }

        public async Task HandleAsync(TokensJoinedMessage message, CancellationToken cancellationToken)
        {
            MatchingTokenAction(message.Tokens.TokenIds, t => t.CompositeToken = message.CompositeToken);
            SourceTokenMap?.AddCompositeToken(message.CompositeToken);
            TargetTokenMap?.AddCompositeToken(message.CompositeToken);
            await RefreshTranslationsAsync(message.Tokens, message.CompositeToken);
        }        
        
        public async Task HandleAsync(TokenSplitMessage message, CancellationToken cancellationToken)
        {
            foreach (var kvp in message.SplitCompositeTokensByIncomingTokenId)
            {
                var compositeToken = kvp.Value.FirstOrDefault();    // For now, the user can only split one token at a time.
                if (compositeToken != null)
                {
                    SourceTokenMap?.ReplaceToken(kvp.Key, compositeToken);
                    TargetTokenMap?.ReplaceToken(kvp.Key, compositeToken);
                }
            }
            await BuildTokenDisplayViewModelsAsync();
            await EventAggregator.PublishOnUIThreadAsync(new TokensUpdatedMessage(), cancellationToken);

            await Task.CompletedTask;
        }

        public async Task HandleAsync(TokenUnjoinedMessage message, CancellationToken cancellationToken)
        {
            MatchingTokenAction(message.Tokens.TokenIds, t => t.CompositeToken = null);
            SourceTokenMap?.RemoveCompositeToken(message.CompositeToken, message.Tokens);
            TargetTokenMap?.RemoveCompositeToken(message.CompositeToken, message.Tokens);
            await RefreshTranslationsAsync(message.Tokens);
        }

        protected virtual async Task RefreshTranslationsAsync(TokenCollection tokens, CompositeToken? compositeToken = null)
        {
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
    }
}
