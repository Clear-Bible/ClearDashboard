using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using SIL.Scripture;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    /// <summary>
    /// A specialization of <see cref="VerseDisplayViewModel"/> for displaying tokens in a corpus view.
    /// </summary>
    public class CorpusDisplayViewModel : VerseDisplayViewModel
    {
        /// <summary>
        /// Creates and initializes an <see cref="CorpusDisplayViewModel"/> instance using the specified DI container.
        /// </summary>
        /// <param name="componentContext">A <see cref="IComponentContext"/> (i.e. LifetimeScope) with which to resolve dependencies.</param>
        /// <param name="tokens">The <see cref="TextRow"/> containing the tokens to display.</param>
        /// <param name="corpus">The <see cref="TokenizedTextCorpus"/> that the tokens are part of.</param>
        /// <returns>A constructed <see cref="CorpusDisplayViewModel"/>.</returns>
        public static async Task<VerseDisplayViewModel> CreateAsync(IComponentContext componentContext, IEnumerable<Token> tokens, TokenizedTextCorpus corpus)
        {
            var verseDisplayViewModel = componentContext.Resolve<CorpusDisplayViewModel>(new NamedParameter("tokens", tokens), new NamedParameter("corpus", corpus));
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
                                      TokenizedTextCorpus corpus,
                                      NoteManager noteManager, 
                                      IMediator mediator,
                                      IEventAggregator eventAggregator,
                                      ILifetimeScope lifetimeScope,
                                      ILogger<VerseDisplayViewModel> logger)
            : base(noteManager, mediator, eventAggregator, lifetimeScope, logger)
        {
            SourceTokenMap = new TokenMap(tokens, corpus);
        }
    }
}
