using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using Microsoft.Extensions.Logging;
using SIL.Machine.Tokenization;

// These need to be specified explicitly to resolve ambiguity with ClearDashboard.DataAccessLayer.Models.
using Alignment = ClearDashboard.DAL.Alignment.Translation.Alignment;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using Token = ClearBible.Engine.Corpora.Token;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    /// <summary>
    /// A class containing the needed information to render a verse of <see cref="Token"/>s in the UI.
    /// </summary>
    public class VerseDisplayViewModel : PropertyChangedBase, 
        IHandle<SelectionUpdatedMessage>,
        IHandle<NoteAddedMessage>,
        IHandle<NoteDeletedMessage>,
        IHandle<NoteMouseEnterMessage>,
        IHandle<NoteMouseLeaveMessage>
    {
        private NoteManager? NoteManager { get; }
        private IEventAggregator? EventAggregator { get; }

        private ILogger<VerseDisplayViewModel>? Logger { get; }

        private IReadOnlyList<Token>? SourceTokens { get; set; }
        private EngineStringDetokenizer SourceDetokenizer { get; set; } = new(new LatinWordDetokenizer());
        public bool IsSourceRtl { get; set; }
        private IReadOnlyList<Token>? TargetTokens { get; set; }
        private EngineStringDetokenizer? TargetDetokenizer { get; set; } = new(new LatinWordDetokenizer());
        public bool IsTargetRtl { get; set; }

        private TranslationSet? TranslationSet { get; set; }
        private IEnumerable<Translation>? Translations { get; set; }

        private AlignmentSet? AlignmentSet { get; set; }
        public IEnumerable<Alignment>? Alignments { get; set; }

        #region Public Properties

        /// <summary>
        /// Gets a collection of source <see cref="TokenDisplayViewModel"/>s to be rendered.
        /// </summary>
        public TokenDisplayViewModelCollection SourceTokenDisplayViewModels { get; private set; } = new();

        /// <summary>
        /// Gets a collection of target <see cref="TokenDisplayViewModel"/>s to be rendered.
        /// </summary>
        public TokenDisplayViewModelCollection TargetTokenDisplayViewModels { get; private set; } = new();

        public Guid Id { get; set; } = Guid.NewGuid();

        #endregion Public Properties

        #region Private methods
        private IEnumerable<(Token token, string paddingBefore, string paddingAfter)> GetPaddedTokens(IEnumerable<Token> tokens, EngineStringDetokenizer detokenizer)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var result = detokenizer.Detokenize(tokens);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved padded tokens from {detokenizer.GetType().Name} detokenizer in {stopwatch.ElapsedMilliseconds} ms");
#endif
                return result;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        private Translation? GetTranslationForToken(Token token)
        {
            return Translations?.FirstOrDefault(t => t.SourceToken.TokenId.Id == token.TokenId.Id) ?? null;
        }

        private async Task BuildTokenDisplayViewModelsAsync()
        {
            SourceTokenDisplayViewModels = await BuildTokenDisplayViewModelsAsync(SourceTokens!, SourceDetokenizer, IsSourceRtl, true);
            NotifyOfPropertyChange(nameof(SourceTokenDisplayViewModels));
            
            if (TargetTokens != null)
            {
                TargetTokenDisplayViewModels = await BuildTokenDisplayViewModelsAsync(TargetTokens, TargetDetokenizer!, IsSourceRtl, true);
                NotifyOfPropertyChange(nameof(TargetTokenDisplayViewModels));
            }

        }

        private async Task<TokenDisplayViewModelCollection> BuildTokenDisplayViewModelsAsync(IEnumerable<Token> tokens, EngineStringDetokenizer detokenizer, bool isRtl, bool isSource)
        {
            var result = new TokenDisplayViewModelCollection();
            
            var paddedTokens = GetPaddedTokens(tokens, detokenizer);
            foreach (var paddedToken in paddedTokens)
            {
                result.Add(new TokenDisplayViewModel(paddedToken.token)
                {
                    PaddingBefore = paddedToken.paddingBefore,
                    PaddingAfter = paddedToken.paddingAfter,
                    Translation = GetTranslationForToken(paddedToken.token),
                    NoteIds = await NoteManager!.GetNoteIdsAsync(paddedToken.token.TokenId),
                    IsSource = isSource,
                    
                });
            }
            return result;
        }

        private async Task<IEnumerable<Translation>> GetTranslations(TranslationSet translationSet, IEnumerable<TokenId> tokens)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var result = await translationSet.GetTranslations(tokens);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved translations in {stopwatch.ElapsedMilliseconds} ms");
#endif
                return result;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        #endregion

        #region Event Handlers
        public async Task HandleAsync(SelectionUpdatedMessage message, CancellationToken cancellationToken)
        {
            var selectedTokens = SourceTokenDisplayViewModels.Where(t => t.IsSelected);
            foreach (var token in selectedTokens)
            {
                if (!message.SelectedTokens.Contains(token))
                {
                    token.IsSelected = false;
                }
            }
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteAddedMessage message, CancellationToken cancellationToken)
        {
            foreach (var token in SourceTokenDisplayViewModels.Where(t => message.Entities.Contains(t.Token.TokenId)))
            {
                token.NoteAdded(message.Note);
            }

            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteDeletedMessage message, CancellationToken cancellationToken)
        {
            foreach (var token in SourceTokenDisplayViewModels.Where(t => message.Entities.Contains(t.Token.TokenId)))
            {
                token.NoteDeleted(message.Note);
            }
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteMouseEnterMessage message, CancellationToken cancellationToken)
        {
            foreach (var token in SourceTokenDisplayViewModels.Where(t => message.Note.Associations.Any(a => a.AssociatedEntityId.Equals(t.Token.TokenId))))
            {
                token.IsNoteHovered = true;
            }
            await Task.CompletedTask;
        }

        public async Task HandleAsync(NoteMouseLeaveMessage message, CancellationToken cancellationToken)
        {
            foreach (var token in SourceTokenDisplayViewModels.Where(t => message.Note.Associations.Any(a => a.AssociatedEntityId.Equals(t.Token.TokenId))))
            {
                token.IsNoteHovered = false;
            }
            await Task.CompletedTask;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets a collection of <see cref="TranslationOption"/>s for a given translation.
        /// </summary>
        /// <param name="token">The <see cref="Token"/> for which to provide options.</param>
        /// <returns>An awaitable <see cref="Task{T}"/> containing a <see cref="IEnumerable{T}"/> of <see cref="TranslationOption"/>s.</returns>
        public async Task<IEnumerable<TranslationOption>> GetTranslationOptionsAsync(Token token)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                if (TranslationSet == null)
                {
                    throw new InvalidOperationException("Cannot retrieve translation options because the translation set is null.  Ensure that you have called ShowTranslationAsync() with the current translation set.");
                }

                var translationModelEntry = await TranslationSet.GetTranslationModelEntryForToken(token);
                if (translationModelEntry == null)
                {
                    Logger?.LogCritical($"Cannot find translation options for {token.SurfaceText}");
                    return new List<TranslationOption>();
                }

                var translationOptions = translationModelEntry.OrderByDescending(option => option.Value)
                    .Select(option => new TranslationOption { Word = option.Key, Count = option.Value })
                    .Take(4)        // For now, we'll just return the top four options; may be configurable in the future
                    .ToList();
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved translation options for {token.SurfaceText} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                return translationOptions;

            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Saves a selected translation for a token to the database.
        /// </summary>
        /// <param name="translation">The <see cref="Translation"/> to save to the database.</param>
        /// <param name="translationActionType"></param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task PutTranslationAsync(Translation translation, string translationActionType)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                if (TranslationSet == null)
                {
                    throw new InvalidOperationException("Cannot save translation because the translation set is null.  Ensure that you have called ShowTranslationAsync() with the current translation set.");
                }

                await TranslationSet.PutTranslation(translation, translationActionType);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Saved translation options for {translation.SourceToken.SurfaceText} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                // If translation propagates to other translations, then we need a fresh call to PopulateTranslations() and to rebuild the token displays.
                if (translationActionType == TranslationActionTypes.PutPropagate)
                {
                    Translations = await GetTranslations(TranslationSet, SourceTokens!.Select(t => t.TokenId));
                    await BuildTokenDisplayViewModelsAsync();
                    await EventAggregator.PublishOnUIThreadAsync(new TokensUpdatedMessage());
                }
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task ShowCorpusAsync(
            TokensTextRow textRow, 
            EngineStringDetokenizer sourceDetokenizer, 
            bool isRtl)
        {
            SourceTokens = textRow.Tokens.GetPositionalSortedBaseTokens().ToList();
            SourceDetokenizer = sourceDetokenizer;
            IsSourceRtl = isRtl;
            IsTargetRtl = false;

            TranslationSet = null;
            
            AlignmentSet = null;

            await BuildTokenDisplayViewModelsAsync();
        }

        public async Task ShowTranslationAsync(
            EngineParallelTextRow engineParallelTextRow,
            TranslationSet translationSet,
            EngineStringDetokenizer sourceDetokenizer,
            bool isSourceRtl)
        {
            SourceTokens = engineParallelTextRow.SourceTokens?.GetPositionalSortedBaseTokens().ToList() ?? throw new InvalidOperationException("Text row has no source tokens");
            SourceDetokenizer = sourceDetokenizer;
            IsSourceRtl = isSourceRtl;
            IsTargetRtl = false;

            TranslationSet = translationSet;
            Translations = await GetTranslations(TranslationSet, SourceTokens.Select(t => t.TokenId));
            
            AlignmentSet = null;

            await BuildTokenDisplayViewModelsAsync();
        }

        public async Task ShowAlignmentsAsync(
            EngineParallelTextRow engineParallelTextRow,
            AlignmentSet alignmentSet,
            EngineStringDetokenizer sourceDetokenizer,
            bool isSourceRtl,
            EngineStringDetokenizer targetDetokenizer,
            bool isTargetRtl)
        {
            SourceTokens = engineParallelTextRow.SourceTokens?.GetPositionalSortedBaseTokens().ToList() ?? throw new InvalidOperationException("Text row has no source tokens");
            SourceDetokenizer = sourceDetokenizer;
            IsSourceRtl = isSourceRtl;

            TargetTokens = engineParallelTextRow.TargetTokens?.GetPositionalSortedBaseTokens().ToList() ?? throw new InvalidOperationException("Text row has no target tokens");
            TargetDetokenizer = targetDetokenizer;
            IsTargetRtl = isTargetRtl;

            TranslationSet = null;

            AlignmentSet = alignmentSet;
            Alignments = await alignmentSet.GetAlignments(new List<EngineParallelTextRow>() { engineParallelTextRow });

            await BuildTokenDisplayViewModelsAsync();
        }

        #endregion

        public VerseDisplayViewModel()
        {
            
        }

        /// <summary>
        /// Constructor used via dependency injection.
        /// </summary>
        /// <param name="noteManager"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="logger"></param>
        // ReSharper disable once UnusedMember.Global
        public VerseDisplayViewModel(NoteManager noteManager, IEventAggregator eventAggregator, ILogger<VerseDisplayViewModel>? logger)
        {
            NoteManager = noteManager;
            EventAggregator = eventAggregator;
            Logger = logger;

            EventAggregator.SubscribeOnUIThread(this);
        }
    }
}
