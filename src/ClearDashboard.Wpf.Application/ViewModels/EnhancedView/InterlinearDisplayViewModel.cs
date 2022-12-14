using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Translation;
using System.Linq;
using Autofac;
using MediatR;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    /// <summary>
    /// A specialization of <see cref="VerseDisplayViewModel"/> for displaying tokens and their translations.
    /// </summary>
    public class InterlinearDisplayViewModel : VerseDisplayViewModel
    {
        private EngineParallelTextRow ParallelTextRow { get; }
        private TranslationSetId TranslationSetId { get; }
        private TranslationManager? TranslationManager { get; set; }

        protected override Translation? GetTranslationForToken(Token token)
        {
            return TranslationManager?.GetTranslationForToken(token);
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

        public override async Task InitializeAsync()
        {
            TranslationManager = await TranslationManager.CreateAsync(LifetimeScope, ParallelTextRow, TranslationSetId);

            await base.InitializeAsync();
        }

        public InterlinearDisplayViewModel(EngineParallelTextRow parallelTextRow,
                                      EngineStringDetokenizer sourceDetokenizer,
                                      bool isSourceRtl,
                                      TranslationSetId translationSetId,
                                      NoteManager noteManager,
                                      IMediator mediator,
                                      IEventAggregator eventAggregator, 
                                      ILifetimeScope lifetimeScope,
                                      ILogger<VerseDisplayViewModel> logger)
            : base(noteManager, mediator, eventAggregator, lifetimeScope, logger)
        {
            ParallelTextRow = parallelTextRow;
            if (parallelTextRow.SourceTokens != null)
            {
                SourceTokenMap = new TokenMap(parallelTextRow.SourceTokens!, sourceDetokenizer, isSourceRtl);
            }

            TranslationSetId = translationSetId;
        }


        public static async Task<VerseDisplayViewModel> CreateAsync(IComponentContext componentContext, EngineParallelTextRow parallelTextRow, EngineStringDetokenizer detokenizer, bool isRtl, TranslationSetId translationSetId)
        {
            var verseDisplayViewModel = componentContext.Resolve<InterlinearDisplayViewModel>(
                new NamedParameter("parallelTextRow", parallelTextRow),
                new NamedParameter("sourceDetokenizer", detokenizer),
                new NamedParameter("isSourceRtl", isRtl),
                new NamedParameter("translationSetId", translationSetId)
            );
            await verseDisplayViewModel.InitializeAsync();
            return verseDisplayViewModel;
        }
    }
}
