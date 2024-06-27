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
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using SIL.Extensions;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Threading;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Messages;

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

        #region Member Variables   

        protected NoteManager NoteManager { get; }
        protected IMediator Mediator { get; }
        protected IEventAggregator EventAggregator { get; }
        protected ILifetimeScope LifetimeScope { get; }
        protected ILogger Logger { get; }

        #endregion //Member Variables


        #region Public Properties

        public bool HasExternalNotes => ExternalNotes.Count() > 0 && AbstractionsSettingsHelper.GetShowExternalNotes() && AbstractionsSettingsHelper.GetExternalNotesEnabled();
        public TokenizedTextCorpus? SourceCorpus => SourceTokenMap?.Corpus;
        public TokenizedTextCorpus? TargetCorpus => TargetTokenMap?.Corpus;

        /// <summary>
        /// Gets the <see cref="ParallelCorpusId"/> for the verse.
        /// </summary>
        public ParallelCorpusId? ParallelCorpusId { get; protected set; }


        /// <summary>
        /// Gets a collection of source <see cref="TokenDisplayViewModel"/>s to be rendered.
        /// </summary>
        public TokenDisplayViewModelCollection SourceTokenDisplayViewModels = new();


        /// <summary>
        /// Gets a collection of target <see cref="TokenDisplayViewModel"/>s to be rendered.
        /// </summary>
        public TokenDisplayViewModelCollection TargetTokenDisplayViewModels { get; private set; } = new();

       
        public AlignmentManager? AlignmentManager { get; set; } = null;
        public bool IsSourceRtl => SourceTokenMap?.IsRtl ?? false;
        public bool IsTargetRtl => TargetTokenMap?.IsRtl ?? false;

        public Guid Id { get; set; } = Guid.NewGuid();

        #endregion //Public Properties


        #region Observable Properties

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

        private BindableCollection<ExternalNote> _externalNotes = new();
        public BindableCollection<ExternalNote> ExternalNotes
        {
            get => _externalNotes;
            set
            {
                Set(ref _externalNotes, value);
                NotifyOfPropertyChange(nameof(HasExternalNotes));
            }
        }

        private string _notesCount = string.Empty;
        public string NotesCount
        {
            get => _notesCount;
            set 
            { 
                Set(ref _notesCount, value);
            }
        }

        private bool _multipleExternalNotes = false;
        public bool MultipleExternalNotes
        {
            get => _multipleExternalNotes && AbstractionsSettingsHelper.GetShowExternalNotes() && AbstractionsSettingsHelper.GetExternalNotesEnabled();
            set
            {
                _multipleExternalNotes = value;
                NotifyOfPropertyChange(nameof(MultipleExternalNotes));
            }
        }

        #endregion //Observable Properties


        #region Constructor

        protected VerseDisplayViewModel(NoteManager noteManager, IMediator mediator, IEventAggregator eventAggregator, ILifetimeScope lifetimeScope, ILogger logger)
        {
            NotifyExternalNotesItemsChanged();

            NoteManager = noteManager;
            Mediator = mediator;
            EventAggregator = eventAggregator;
            LifetimeScope = lifetimeScope;
            Logger = logger;

            EventAggregator.SubscribeOnUIThread(this);
        }

        #endregion //Constructor


        #region Methods

        protected virtual async Task InitializeAsync()
        {
            await BuildTokenDisplayViewModelsAsync();
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
            
            bool firstToken = true;

            Dictionary<Guid,Guid> noteGuids = new();

            foreach (var (token, paddingBefore, paddingAfter) in tokenMap.PaddedTokens)
            {
                // TODO808: Completed - reviewed with Andy
                // 1. a Composite(parallel) that overlaps with Composite(null) in parallel view will hide Composite(null) (but show its child tokens).
                //
                // The GetCompositeToken() call should prioritize Composite(parallel) over Composite(null).  Requires a property on CompositeToken to indicate whether
                // it is parallel or not.
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

                // check to see if this is the first jot note in a series of notes
                if (tokenDisplayViewModel.TokenNoteIds.Count > 0)
                {
                    foreach (var noteId in tokenDisplayViewModel.TokenNoteIds)
                    {
                        if (!noteGuids.ContainsKey(noteId.Id))
                        {
                            noteGuids.Add(noteId.Id, Guid.NewGuid());
                            tokenDisplayViewModel.IsFirstJotsNoteToken = true;
                        }
                    }
                }

                //Debug.WriteLine($"TokenDisplayViewModel: {tokenDisplayViewModel.SurfaceText} {tokenDisplayViewModel.TokenHasNote} {tokenDisplayViewModel.IsFirstJotsNoteToken}");

                if (tokenDisplayViewModel.Translation?.TranslationId != null)
                {
                    tokenDisplayViewModel.TranslationNoteIds = await NoteManager.GetNoteIdsAsync(tokenDisplayViewModel.Translation.TranslationId);
                }

                // pad out extra space on the first token for the external notes flag
                if (firstToken && tokenDisplayViewModel.PaddingBefore == "")
                {
                    firstToken = false;
                    tokenDisplayViewModel.PaddingBefore = "6";
                }

                result.Add(tokenDisplayViewModel);
            }
            return result;
        }

        public void NotifyExternalNotesItemsChanged()
        {
            NotifyOfPropertyChange(nameof(ExternalNotes));
            NotifyOfPropertyChange(nameof(HasExternalNotes));
        }

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

        public virtual void SetExternalNotes(List<List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>> tokenizedCorpusNotes)
        {
            //update tokens explicitly associated with external notes
            SetExternalNotesOnTokenDisplayViewModels(SourceTokenDisplayViewModels, tokenizedCorpusNotes.First());
            if (tokenizedCorpusNotes.Count() > 1)
                SetExternalNotesOnTokenDisplayViewModels(TargetTokenDisplayViewModels, tokenizedCorpusNotes.Skip(1).First());

            // update verse with external notes associated with verse but not explicitly with tokens in verse.
            TokenIdVerseEqualityComparer tokenIdVerseEqualityComparer = new();
            var uniqueVersesRepresentedByTokens = SourceTokenDisplayViewModels
                .Select(tdvm => tdvm.Token.TokenId)
                .Distinct(tokenIdVerseEqualityComparer)
                .Select(tid => new VerseRef(tid.BookNumber, tid.ChapterNumber, tid.VerseNumber))
                .ToList();

            uniqueVersesRepresentedByTokens.AddRange(TargetTokenDisplayViewModels
                .Select(tdvm => tdvm.Token.TokenId)
                .Distinct(tokenIdVerseEqualityComparer)
                .Select(tid => new VerseRef(tid.BookNumber, tid.ChapterNumber, tid.VerseNumber)));


            VerseRefVerseNoVersificationEqualityComparer verseRefNoVersificationComparer = new();
            var externalNotes = tokenizedCorpusNotes
                .SelectMany(tcn => tcn)
                .Where(noteInfos => uniqueVersesRepresentedByTokens.Contains(noteInfos.verseRef))
                .Where(noteInfos => noteInfos.tokenIds == null || noteInfos.tokenIds.Count() == 0)
                .Select(noteInfo => noteInfo.externalNote)
                .ToList();

            bool notifyVerseDisplayViewModelExternalNotesItemsChanged = false;
            if (ExternalNotes.Count() > 0)
            {
                ExternalNotes.Clear();
                notifyVerseDisplayViewModelExternalNotesItemsChanged = true;
            }

            if (externalNotes != null && externalNotes.Count() > 0)
            {
                ExternalNotes.AddRange(externalNotes);
                NotesCount = $"Count: {externalNotes.Count()}";
                notifyVerseDisplayViewModelExternalNotesItemsChanged = true;

                if (externalNotes.Count > 1)
                {
                    MultipleExternalNotes = true;
                }
            }

            if (notifyVerseDisplayViewModelExternalNotesItemsChanged)
                NotifyExternalNotesItemsChanged();

        }
        protected void SetExternalNotesOnTokenDisplayViewModels(TokenDisplayViewModelCollection tokenDisplayViewModels, List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)> noteInfos)
        {
            foreach ( var tokenDisplayViewModel in tokenDisplayViewModels)
            {
                tokenDisplayViewModel.MultipleExternalNotes = false;
                tokenDisplayViewModel.IsFirstExternalNoteToken = false;

                 var externalNotes = noteInfos
                    .Where(noteInfo => noteInfo.tokenIds?.Contains(tokenDisplayViewModel.Token.TokenId) ?? false)
                    .Select(noteInfo => noteInfo.externalNote)
                    .ToList();

                bool notifyTokenViewModelExternalNotesItemsChanged = false;
                if (tokenDisplayViewModel.ExternalNotes.Count() > 0)
                {
                    tokenDisplayViewModel.ExternalNotes.Clear();
                    notifyTokenViewModelExternalNotesItemsChanged = true;
                }

                if (externalNotes != null && externalNotes.Count() > 0)
                {
                    tokenDisplayViewModel.ExternalNotes.AddRange(externalNotes);
                    notifyTokenViewModelExternalNotesItemsChanged = true;
                }

                if (notifyTokenViewModelExternalNotesItemsChanged)
                    tokenDisplayViewModel.NotifyExternalNotesItemsChanged();

                // separate out the first tokenId of the note series
                foreach (var note in noteInfos)
                {
                    var firstTokenId = note.tokenIds?.FirstOrDefault();

                    if (firstTokenId != null && firstTokenId.IdEquals(tokenDisplayViewModel.Token.TokenId))
                    {
                        // look for multiple external notes on the same token
                        List<string> tokenIdList = new();
                        foreach (var noteInfo in noteInfos)
                        {
                            foreach (var tokenId in noteInfo.tokenIds)
                            {
                                tokenIdList.Add(tokenId.ToString());
                            }
                        }

                        var count = tokenIdList.Where(x => x == tokenDisplayViewModel.Token.TokenId.ToString()).Count();
                        if (count > 1)
                        {
                            tokenDisplayViewModel.MultipleExternalNotes = true;
                        }

                        // set the first external note token
                        tokenDisplayViewModel.IsFirstExternalNoteToken = true;
                    }
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

        protected virtual async Task RefreshTranslationsAsync(TokenCollection tokens, CompositeToken? compositeToken = null)
        {
            await Task.CompletedTask;
        }

        #endregion // Methods

        #region IHandle

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

            Dictionary<Guid, Guid> noteGuids = new();

            foreach (var model in SourceTokenDisplayViewModels)
            {
                // check to see if this is the first jot note in a series of notes
                if (model.TokenNoteIds.Count > 0)
                {
                    foreach (var noteId in model.TokenNoteIds)
                    {
                        if (!noteGuids.ContainsKey(noteId.Id))
                        {
                            noteGuids.Add(noteId.Id, Guid.NewGuid());
                            model.IsFirstJotsNoteToken = true;
                        }
                    }
                }
            }

            noteGuids = new();
            foreach (var model in TargetTokenDisplayViewModels)
            {
                // check to see if this is the first jot note in a series of notes
                if (model.TokenNoteIds.Count > 0)
                {
                    foreach (var noteId in model.TokenNoteIds)
                    {
                        if (!noteGuids.ContainsKey(noteId.Id))
                        {
                            noteGuids.Add(noteId.Id, Guid.NewGuid());
                            model.IsFirstJotsNoteToken = true;
                        }
                    }
                }
            }
        }

        public async Task HandleAsync(NoteDeletedMessage message, CancellationToken cancellationToken)
        {
            MatchingTokenAction(message.EntityIds.Where(e => e.GetType() == typeof(TokenId)), t => t.TokenNoteDeleted(message.Note));
            MatchingTokenAction(message.EntityIds.Where(e => e.GetType() == typeof(TranslationId)), t => t.TranslationNoteDeleted(message.Note));
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteMouseEnterMessage message, CancellationToken cancellationToken)
        {
            if (message is { Note.Associations: not null })
            {
                MatchingTokenAction(
                    t => message.Note.Associations.Any(a =>
                        a.AssociatedEntityId.IdEquals(t.Token.TokenId) ||
                        a.AssociatedEntityId.IdEquals(t.Translation?.TranslationId)), t =>
                    {
                        t.IsNoteHovered = true;
                        t.NoteIndicatorBrush = message.NewNote ? Brushes.Orange : Brushes.MediumPurple;
                    });
            }

            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteMouseLeaveMessage message, CancellationToken cancellationToken)
        {
            if (message is { Note.Associations: not null })
            {
                MatchingTokenAction(
                    t => message.Note.Associations.Any(a =>
                        a.AssociatedEntityId.IdEquals(t.Token.TokenId) ||
                        a.AssociatedEntityId.IdEquals(t.Translation?.TranslationId)), t => t.IsNoteHovered = false);

            }

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
            // TODO808:  Need some input from Andy/Chris
            // 3. for token splitting api return value, for corpus view needs to look in the composite dict for a Composite(null),
            // and if none use the dict of individual non-composite tokens.
            //
            // This method is currently replacing the token using the dictionary of composites; it needs to be updated with the above logic.

            // 1. Composite where children are changing
            // 2. Token where it becomes a composite
            foreach (var kvp in message.SplitCompositeTokensByIncomingTokenId)
            {
                var compositeToken = kvp.Value.FirstOrDefault();    // For now, the user can only split one token at a time.
                var isParallel = compositeToken?.HasMetadatum("IsParallelCompositeToken");
                if (compositeToken != null && isParallel != null && compositeToken.GetMetadatum<bool>("IsParallelCompositeToken"))
                {
                    SourceTokenMap?.ReplaceToken(kvp.Key, compositeToken);
                    TargetTokenMap?.ReplaceToken(kvp.Key, compositeToken);
                }
                else if (compositeToken != null)
                {
                    // For Chris - this
                    var childTokens  = message.SplitChildTokensByIncomingTokenId[kvp.Key].ToList();
                  
                    SourceTokenMap?.RemoveCompositeToken(compositeToken, new TokenCollection(childTokens));
                    TargetTokenMap?.RemoveCompositeToken(compositeToken, new TokenCollection(childTokens));
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

        #endregion // IHandle


        private class VerseRefVerseNoVersificationEqualityComparer : IEqualityComparer<VerseRef>
        {
            public bool Equals(VerseRef x, VerseRef y)
            {
                return x.BookNum == y.BookNum
                    && x.ChapterNum == y.ChapterNum
                    && x.VerseNum == y.VerseNum;
            }
            public int GetHashCode([DisallowNull] VerseRef obj) => obj.BookNum ^ obj.ChapterNum ^ obj.VerseNum;
        }
        private class TokenIdVerseEqualityComparer : IEqualityComparer<TokenId>
        {
            public bool Equals(TokenId? x, TokenId? y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (y is null || x is null)
                    return false;

                return x.BookNumber == y.BookNumber
                    && x.ChapterNumber == y.ChapterNumber
                    && x.VerseNumber == y.VerseNumber;
            }

            public int GetHashCode([DisallowNull] TokenId obj) => obj.BookNumber ^ obj.ChapterNumber ^ obj.VerseNumber;
        }
    }
}
