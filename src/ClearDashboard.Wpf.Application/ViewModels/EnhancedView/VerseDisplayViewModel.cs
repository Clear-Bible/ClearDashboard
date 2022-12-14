using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
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
        IHandle<NoteMouseLeaveMessage>
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
        /// <returns>A <see cref="Translation"/> for the token if a valid <see cref="TranslationSet"/> is known; null otherwise.</returns>
        protected virtual Translation? GetTranslationForToken(Token token)
        {
            return null;
        }

        protected async Task BuildTokenDisplayViewModelsAsync()
        {
            if (SourceTokenMap != null)
            {
                SourceTokenDisplayViewModels = await BuildTokenDisplayViewModelsAsync(SourceTokenMap.PaddedTokens, IsSourceRtl, true);
                NotifyOfPropertyChange(nameof(SourceTokenDisplayViewModels));
            }

            if (TargetTokenMap != null)
            {
                TargetTokenDisplayViewModels = await BuildTokenDisplayViewModelsAsync(TargetTokenMap.PaddedTokens, IsTargetRtl, false);
                NotifyOfPropertyChange(nameof(SourceTokenDisplayViewModels));
            }
        }
        
        private async Task<TokenDisplayViewModelCollection> BuildTokenDisplayViewModelsAsync(PaddedTokenCollection paddedTokens, bool isRtl, bool isSource)
        {
            var result = new TokenDisplayViewModelCollection();
            
            foreach (var (token, paddingBefore, paddingAfter) in paddedTokens)
            {
                result.Add(new TokenDisplayViewModel(token)
                {
                    PaddingBefore = paddingBefore,
                    PaddingAfter = paddingAfter,
                    Translation = GetTranslationForToken(token),
                    NoteIds = await NoteManager.GetNoteIdsAsync(token.TokenId),
                    IsSource = isSource,
                });
            }
            return result;
        }

        public async Task HandleAsync(SelectionUpdatedMessage message, CancellationToken cancellationToken)
        {
            SourceTokenDisplayViewModels.NonMatchingTokenAction(message.SelectedTokens.Select(t => t.Token.TokenId), t => t.IsTokenSelected = false);
            TargetTokenDisplayViewModels.NonMatchingTokenAction(message.SelectedTokens.Select(t => t.Token.TokenId), t => t.IsTokenSelected = false);

            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteAddedMessage message, CancellationToken cancellationToken)
        {
            SourceTokenDisplayViewModels.MatchingTokenAction(message.EntityIds, t => t.NoteAdded(message.Note));
            TargetTokenDisplayViewModels.MatchingTokenAction(message.EntityIds, t => t.NoteAdded(message.Note));

            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteDeletedMessage message, CancellationToken cancellationToken)
        {
            SourceTokenDisplayViewModels.MatchingTokenAction(message.EntityIds, t => t.NoteDeleted(message.Note));
            TargetTokenDisplayViewModels.MatchingTokenAction(message.EntityIds, t => t.NoteDeleted(message.Note));

            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteMouseEnterMessage message, CancellationToken cancellationToken)
        {
            SourceTokenDisplayViewModels.MatchingTokenAction(t => message.Note.Associations.Any(a => a.AssociatedEntityId.IdEquals(t.Token.TokenId)), t => t.IsNoteHovered = true);
            TargetTokenDisplayViewModels.MatchingTokenAction(t => message.Note.Associations.Any(a => a.AssociatedEntityId.IdEquals(t.Token.TokenId)), t => t.IsNoteHovered = true);

            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteMouseLeaveMessage message, CancellationToken cancellationToken)
        {
            SourceTokenDisplayViewModels.MatchingTokenAction(t => message.Note.Associations.Any(a => a.AssociatedEntityId.IdEquals(t.Token.TokenId)), t => t.IsNoteHovered = false);
            TargetTokenDisplayViewModels.MatchingTokenAction(t => message.Note.Associations.Any(a => a.AssociatedEntityId.IdEquals(t.Token.TokenId)), t => t.IsNoteHovered = false);

            await Task.CompletedTask;
        }

        public virtual async Task InitializeAsync()
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
