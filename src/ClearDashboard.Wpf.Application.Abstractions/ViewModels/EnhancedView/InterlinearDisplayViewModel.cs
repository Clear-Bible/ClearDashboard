﻿using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Translation;
using Autofac;
using MediatR;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using System.Collections.Generic;
using System;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    /// <summary>
    /// A specialization of <see cref="VerseDisplayViewModel"/> for displaying tokens along with their translations.
    /// </summary>
    public class InterlinearDisplayViewModel : VerseDisplayViewModel
    {
        private EngineParallelTextRow ParallelTextRow { get; }
        private TranslationSetId TranslationSetId { get; }
        private TranslationManager? TranslationManager { get; set; }

        /// <summary>
        /// Get the <see cref="Translation"/> for a specified token.
        /// </summary>
        /// <remarks>
        /// This overrides the base class implementation to determine the translation from the current <see cref="TranslationSet"/>.
        /// </remarks>
        /// <param name="token">The <see cref="Token"/> for which to obtain a translation.</param>
        /// <param name="compositeToken">An optional <see cref="CompositeToken"/> that <paramref name="token"/> is a constituent of.</param>
        /// <returns>A <see cref="Translation"/> for the token if a valid <see cref="TranslationSet"/> is known; null otherwise.</returns>

        protected override Translation? GetTranslationForToken(Token token, CompositeToken? compositeToken)
        {
            return TranslationManager?.GetTranslationForToken(token, compositeToken);
        }

        /// <summary>
        /// Refreshes the translations for the specified tokens when a composite token is joined or unjoined.
        /// </summary>
        /// <param name="tokens">The tokens to refresh.</param>
        /// <param name="compositeToken">A composite token for a join operation; null for an unjoin operation.</param>
        /// <returns>An awaitable Task.</returns>
        protected override async Task RefreshTranslationsAsync(TokenCollection tokens, CompositeToken? compositeToken = null)
        {
            if (compositeToken != null)
            {
                await TranslationManager!.AddTranslationAsync(compositeToken.TokenId);
            }
            foreach (var tokenDisplay in SourceTokenDisplayViewModels)
            {
                if (tokens.Contains(tokenDisplay.Token))
                {
                    tokenDisplay.Translation = GetTranslationForToken(tokenDisplay.Token, compositeToken);
                }
            }
            await EventAggregator.PublishOnUIThreadAsync(new TokensUpdatedMessage());
        }

        /// <summary>
        /// Gets a collection of <see cref="TranslationOption"/>s for a given translation.
        /// </summary>
        /// <param name="token">The <see cref="Token"/> for which to provide options.</param>
        /// <returns>An awaitable <see cref="Task{T}"/> containing a <see cref="IEnumerable{T}"/> of <see cref="TranslationOption"/>s.</returns>
        public async Task<TranslationOptionCollection> GetTranslationOptionsAsync(Token token)
        {
            if (TranslationManager == null) throw new InvalidOperationException($"Cannot retrieve translation options for token '{token.SurfaceText}' ({token.TokenId.Id}) because the translation manager is invalid.");
            return await TranslationManager.GetTranslationOptionsAsync(token);
        }

        /// <summary>
        /// Saves a selected translation for a token to the database.
        /// </summary>
        /// <param name="translation">The <see cref="Translation"/> to save to the database.</param>
        /// <param name="translationActionType"></param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task PutTranslationAsync(Translation translation, string translationActionType)
        {
            if (TranslationManager == null) throw new InvalidOperationException($"Cannot save translation {translation.TargetTranslationText} for token '{translation.SourceToken.SurfaceText}' ({translation.SourceToken.TokenId.Id}) because the translation manager is invalid.");

            await TranslationManager.PutTranslationAsync(translation, translationActionType);

            await BuildTokenDisplayViewModelsAsync();
            await EventAggregator.PublishOnUIThreadAsync(new TokensUpdatedMessage());
        }

        /// <summary>
        /// Initializes the view model with the translations for the verse.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        protected override async Task InitializeAsync()
        {
            TranslationManager = await TranslationManager.CreateAsync(LifetimeScope, ParallelTextRow, TranslationSetId);

            await base.InitializeAsync();
        }

        /// <summary>
        /// Creates an <see cref="InterlinearDisplayViewModel"/> instance using the specified DI container.
        /// </summary>
        /// <param name="componentContext">A <see cref="IComponentContext"/> (i.e. LifetimeScope) with which to resolve dependencies.</param>
        /// <param name="parallelTextRow">The <see cref="EngineParallelTextRow"/> containing the tokens to align.</param>
        /// <param name="parallelCorpusId">The <see cref="ParallelCorpusId"/> of the parallel corpus.</param>
        /// <param name="detokenizer">The detokenizer to use for the source tokens.</param>
        /// <param name="isRtl">True if the source tokens should be displayed right-to-left (RTL); false otherwise.</param>
        /// <param name="translationSetId">The ID of the translation set to use.</param>
        /// <returns>A constructed <see cref="InterlinearDisplayViewModel"/>.</returns>
        public static async Task<VerseDisplayViewModel> CreateAsync(IComponentContext componentContext, EngineParallelTextRow parallelTextRow, ParallelCorpusId parallelCorpusId, EngineStringDetokenizer detokenizer, bool isRtl, TranslationSetId translationSetId)
        {
            var verseDisplayViewModel = componentContext.Resolve<InterlinearDisplayViewModel>(
                new NamedParameter("parallelTextRow", parallelTextRow),
                new NamedParameter("parallelCorpusId", parallelCorpusId),
                new NamedParameter("sourceDetokenizer", detokenizer),
                new NamedParameter("isSourceRtl", isRtl),
                new NamedParameter("translationSetId", translationSetId)
            );
            await verseDisplayViewModel.InitializeAsync();
            return verseDisplayViewModel;
        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <remarks>
        /// This is for use by the DI container; use <see cref="CreateAsync"/> instead to create and initialize an instance of this view model.
        /// </remarks>
        public InterlinearDisplayViewModel(EngineParallelTextRow parallelTextRow,
            ParallelCorpusId parallelCorpusId,
            EngineStringDetokenizer sourceDetokenizer,
            bool isSourceRtl,
            TranslationSetId translationSetId,
            NoteManager noteManager,
            IMediator mediator,
            IEventAggregator eventAggregator,
            ILifetimeScope lifetimeScope,
            ILogger<InterlinearDisplayViewModel> logger)
            : base(noteManager, mediator, eventAggregator, lifetimeScope, logger)
        {
            ParallelTextRow = parallelTextRow;
            ParallelCorpusId = parallelCorpusId;
            if (parallelTextRow.SourceTokens != null)
            {
                SourceTokenMap = new TokenMap(parallelTextRow.SourceTokens!, sourceDetokenizer, isSourceRtl);
            }

            TranslationSetId = translationSetId;
        }
    }
}
