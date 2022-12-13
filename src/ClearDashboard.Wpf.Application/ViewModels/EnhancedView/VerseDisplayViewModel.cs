using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
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
                _sourceTokenMap = value;
                NotifyOfPropertyChange(nameof(IsSourceRtl));
            }
        }

        private TokenMap? _targetTokenMap;
        protected TokenMap? TargetTokenMap
        {
            private get { return _targetTokenMap; }
            set
            {
                _targetTokenMap = value;
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

        private static void UnselectTokens(TokenDisplayViewModelCollection collection, TokenDisplayViewModelCollection selectedTokens)
        {
            var currentlySelectedTokens = collection.Where(t => t.IsTokenSelected);
            foreach (var token in currentlySelectedTokens)
            {
                if (!selectedTokens.Contains(token))
                {
                    token.IsTokenSelected = false;
                }
            }
        }

        public async Task HandleAsync(SelectionUpdatedMessage message, CancellationToken cancellationToken)
        {
            UnselectTokens(SourceTokenDisplayViewModels, message.SelectedTokens);
            UnselectTokens(TargetTokenDisplayViewModels, message.SelectedTokens);
            
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteAddedMessage message, CancellationToken cancellationToken)
        {
            foreach (var token in SourceTokenDisplayViewModels.Where(t => message.Entities.Contains(t.Token.TokenId, new IIdEqualityComparer())))
            {
                token.NoteAdded(message.Note);
            }
            foreach (var token in TargetTokenDisplayViewModels.Where(t => message.Entities.Contains(t.Token.TokenId, new IIdEqualityComparer())))
            {
                token.NoteAdded(message.Note);
            }

            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteDeletedMessage message, CancellationToken cancellationToken)
        {
            foreach (var token in SourceTokenDisplayViewModels.Where(t => message.Entities.Contains(t.Token.TokenId, new IIdEqualityComparer())))
            {
                token.NoteDeleted(message.Note);
            }
            foreach (var token in TargetTokenDisplayViewModels.Where(t => message.Entities.Contains(t.Token.TokenId, new IIdEqualityComparer())))
            {
                token.NoteDeleted(message.Note);
            }
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteMouseEnterMessage message, CancellationToken cancellationToken)
        {
            foreach (var token in SourceTokenDisplayViewModels.Where(t => message.Note.Associations.Any(a => a.AssociatedEntityId.IdEquals(t.Token.TokenId))))
            {
                token.IsNoteHovered = true;
            }
            foreach (var token in TargetTokenDisplayViewModels.Where(t => message.Note.Associations.Any(a => a.AssociatedEntityId.IdEquals(t.Token.TokenId))))
            {
                token.IsNoteHovered = true;
            }
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteMouseLeaveMessage message, CancellationToken cancellationToken)
        {
            foreach (var token in SourceTokenDisplayViewModels.Where(t => message.Note.Associations.Any(a => a.AssociatedEntityId.IdEquals(t.Token.TokenId))))
            {
                token.IsNoteHovered = false;
            }
            foreach (var token in TargetTokenDisplayViewModels.Where(t => message.Note.Associations.Any(a => a.AssociatedEntityId.IdEquals(t.Token.TokenId))))
            {
                token.IsNoteHovered = false;
            }
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
