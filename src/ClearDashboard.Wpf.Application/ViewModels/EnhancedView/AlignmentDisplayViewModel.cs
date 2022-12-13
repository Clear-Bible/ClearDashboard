using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Translation;
using System.Threading.Tasks;
using Autofac;
using MediatR;
using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    /// <summary>
    /// A specialization of <see cref="VerseDisplayViewModel"/> for displaying alignments.
    /// </summary>
    public class AlignmentDisplayViewModel : VerseDisplayViewModel
    {
        private EngineParallelTextRow ParallelTextRow { get; }
        private AlignmentSetId AlignmentSetId { get; }
        private AlignmentManager? AlignmentManager { get; set; }

        /// <summary>
        /// Gets the collection of alignments for the verse.
        /// </summary>
        public override AlignmentCollection? Alignments => AlignmentManager?.Alignments;

        /// <summary>
        /// Initializes the view model with the alignments for the verse.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public override async Task InitializeAsync()
        {
            AlignmentManager = await AlignmentManager.CreateAsync(LifetimeScope, ParallelTextRow, AlignmentSetId);

            await base.InitializeAsync();
        }

        public AlignmentDisplayViewModel(EngineParallelTextRow parallelTextRow,
                                      EngineStringDetokenizer sourceDetokenizer,
                                      bool isSourceRtl,
                                      EngineStringDetokenizer targetDetokenizer,
                                      bool isTargetRtl,
                                      AlignmentSetId alignmentSetId,
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
                SourceTokenMap = new TokenMap(parallelTextRow.SourceTokens, sourceDetokenizer, isSourceRtl);
            }
            if (parallelTextRow.TargetTokens != null)
            {
                TargetTokenMap = new TokenMap(parallelTextRow.TargetTokens, targetDetokenizer, isTargetRtl);
            }

            AlignmentSetId = alignmentSetId;
        }

        /// <summary>
        /// Creates an <see cref="AlignmentDisplayViewModel"/> instance using the specified DI container.
        /// </summary>
        /// <param name="componentContext">A <see cref="IComponentContext"/> (i.e. LifetimeScope) with which to resolve dependencies.</param>
        /// <param name="parallelTextRow">The <see cref="EngineParallelTextRow"/> containing the tokens to align.</param>
        /// <param name="sourceDetokenizer">The detokenizer to use for the source tokens.</param>
        /// <param name="isSourceRtl">True if the source tokens should be displayed right-to-left (RTL); false otherwise.</param>
        /// <param name="targetDetokenizer">The detokenizer to use for the target tokens.</param>
        /// <param name="isTargetRtl">True if the source tokens should be displayed right-to-left (RTL); false otherwise.</param>
        /// <param name="alignmentSetId">The ID of the alignment set to use for aligning the tokens.</param>
        /// <returns></returns>
        public static async Task<AlignmentDisplayViewModel> CreateAsync(IComponentContext componentContext, EngineParallelTextRow parallelTextRow, EngineStringDetokenizer sourceDetokenizer, bool isSourceRtl, EngineStringDetokenizer targetDetokenizer, bool isTargetRtl, AlignmentSetId alignmentSetId)
        {
            var viewModel = componentContext.Resolve<AlignmentDisplayViewModel>(
                new NamedParameter("parallelTextRow", parallelTextRow),
                new NamedParameter("sourceDetokenizer", sourceDetokenizer),
                new NamedParameter("isSourceRtl", isSourceRtl),
                new NamedParameter("targetDetokenizer", targetDetokenizer),
                new NamedParameter("isTargetRtl", isTargetRtl),
                new NamedParameter("alignmentSetId", alignmentSetId)
            );
            await viewModel.InitializeAsync();
            return viewModel;
        }
    }
}
