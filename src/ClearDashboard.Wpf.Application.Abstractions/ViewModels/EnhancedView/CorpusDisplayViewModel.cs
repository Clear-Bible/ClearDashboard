using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using SIL.Scripture;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class CorpusDisplayViewModel : VerseDisplayViewModel
    {
        /// <summary>
        /// Creates an <see cref="InterlinearDisplayViewModel"/> instance using the specified DI container.
        /// </summary>
        /// <param name="componentContext">A <see cref="IComponentContext"/> (i.e. LifetimeScope) with which to resolve dependencies.</param>
        /// <param name="tokens">The <see cref="TextRow"/> containing the tokens to display.</param>
        /// <param name="detokenizer">The detokenizer to use for the source tokens.</param>
        /// <param name="translationSetId">The ID of the translation set to use.</param>
        /// <returns>A constructed <see cref="CorpusDisplayViewModel"/>.</returns>
        public static async Task<VerseDisplayViewModel> CreateAsync(IComponentContext componentContext, IEnumerable<Token> tokens, EngineStringDetokenizer detokenizer, bool isRtl)
        {
            var verseDisplayViewModel = componentContext!.Resolve<CorpusDisplayViewModel>(
                new NamedParameter("tokens", tokens),
                new NamedParameter("detokenizer", detokenizer),
                new NamedParameter("isRtl", isRtl)
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
        public CorpusDisplayViewModel(IEnumerable<Token> tokens,
                                      EngineStringDetokenizer detokenizer,
                                      bool isRtl,
                                      NoteManager noteManager, 
                                      IMediator mediator,
                                      IEventAggregator eventAggregator,
                                      ILifetimeScope lifetimeScope,
                                      ILogger<VerseDisplayViewModel> logger)
            : base(noteManager, mediator, eventAggregator, lifetimeScope, logger)
        {
            SourceTokenMap = new TokenMap(tokens, detokenizer, isRtl);
        }
        public override void SetExternalNotes(List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)> sourceTokenizedCorpusNotes,
            List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>? targetTokenizedCorpusNotes)
        {
            SetExternalNotesOnTokenDisplayViewModels(SourceTokenDisplayViewModels, sourceTokenizedCorpusNotes);
        }
    }
}
