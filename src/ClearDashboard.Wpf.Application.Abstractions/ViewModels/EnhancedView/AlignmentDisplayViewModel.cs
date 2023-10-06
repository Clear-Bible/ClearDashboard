using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Translation;
using System.Threading.Tasks;
using Autofac;
using ClearDashboard.DAL.Alignment.Corpora;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using System.Threading;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using SIL.Scripture;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    /// <summary>
    /// A specialization of <see cref="VerseDisplayViewModel"/> for displaying alignments.
    /// </summary>
    public class AlignmentDisplayViewModel : VerseDisplayViewModel
    {
        private List<EngineParallelTextRow> ParallelTextRows { get; }
        private AlignmentSetId AlignmentSetId { get; }

        protected override IEnumerable<Token>? GetTargetTokens(bool isSource, TokenId tokenId)
        {

            switch (isSource)
            {
                case true:
                    {
                        var tokens = AlignmentManager?.Alignments?
                            .Where(a => a.AlignedTokenPair.SourceToken.TokenId.IdEquals(tokenId))
                            .SelectMany(a => a.AlignedTokenPair.TargetToken is not CompositeToken targetToken ? new List<Token>() { a.AlignedTokenPair.TargetToken } : targetToken.Tokens);
                        return tokens;
                    }
                case false:
                    {
                        var tokens = AlignmentManager?.Alignments?
                            .Where(a => a.AlignedTokenPair.TargetToken.TokenId.IdEquals(tokenId))
                            .SelectMany(a => a.AlignedTokenPair.TargetToken is not CompositeToken token ? new List<Token>() { a.AlignedTokenPair.TargetToken } : token.Tokens);

                        return tokens;
                    }
            }

        }

        public override void SetExternalNotes(List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)> sourceTokenizedCorpusNotes,
            List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>? targetTokenizedCorpusNotes)
        {
            SetExternalNotesOnTokenDisplayViewModels(SourceTokenDisplayViewModels, sourceTokenizedCorpusNotes);
            if (targetTokenizedCorpusNotes != null)
                SetExternalNotesOnTokenDisplayViewModels(TargetTokenDisplayViewModels, targetTokenizedCorpusNotes);
        }

        protected override IEnumerable<Token>? GetSourceTokens(bool isSource, TokenId tokenId)
        {

            switch (isSource)
            {
                case true:
                    {
                        var tokens = AlignmentManager?.Alignments?
                             .Where(a => a.AlignedTokenPair.SourceToken.TokenId.IdEquals(tokenId))
                             .SelectMany(a => a.AlignedTokenPair.SourceToken is not CompositeToken token ? new List<Token>() { a.AlignedTokenPair.SourceToken } : token.Tokens);
                        return tokens;
                    }
                case false:
                    {
                        var tokens = AlignmentManager?.Alignments?
                            .Where(a => a.AlignedTokenPair.TargetToken.TokenId.IdEquals(tokenId))
                            .SelectMany(a => a.AlignedTokenPair.SourceToken is not CompositeToken token ? new List<Token>() { a.AlignedTokenPair.SourceToken } : token.Tokens);
                        return tokens;
                    }
            }

        }

        /// <summary>
        /// Initializes the view model with the alignments for the verse.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        protected override async Task InitializeAsync()
        {
            AlignmentManager = await AlignmentManager.CreateAsync(LifetimeScope, ParallelTextRows, AlignmentSetId);

            await base.InitializeAsync();
        }

        /// <summary>
        /// Creates an <see cref="AlignmentDisplayViewModel"/> instance using the specified DI container.
        /// </summary>
        /// <param name="componentContext">A <see cref="IComponentContext"/> (i.e. LifetimeScope) with which to resolve dependencies.</param>
        /// <param name="parallelTextRow">The <see cref="EngineParallelTextRow"/> containing the tokens to align.</param>
        /// <param name="parallelCorpusId">The <see cref="ParallelCorpusId"/> of the parallel corpus.</param>
        /// <param name="sourceDetokenizer">The detokenizer to use for the source tokens.</param>
        /// <param name="isSourceRtl">True if the source tokens should be displayed right-to-left (RTL); false otherwise.</param>
        /// <param name="targetDetokenizer">The detokenizer to use for the target tokens.</param>
        /// <param name="isTargetRtl">True if the source tokens should be displayed right-to-left (RTL); false otherwise.</param>
        /// <param name="alignmentSetId">The ID of the alignment set to use for aligning the tokens.</param>
        /// <returns>A constructed <see cref="InterlinearDisplayViewModel"/>.</returns>
        public static async Task<AlignmentDisplayViewModel> CreateAsync(IComponentContext componentContext, List<EngineParallelTextRow> parallelTextRows, ParallelCorpusId parallelCorpusId, EngineStringDetokenizer sourceDetokenizer, bool isSourceRtl, EngineStringDetokenizer targetDetokenizer, bool isTargetRtl, AlignmentSetId alignmentSetId)
        {
            var viewModel = componentContext.Resolve<AlignmentDisplayViewModel>(
                new NamedParameter("parallelTextRows", parallelTextRows),
                new NamedParameter("parallelCorpusId", parallelCorpusId),
                new NamedParameter("sourceDetokenizer", sourceDetokenizer),
                new NamedParameter("isSourceRtl", isSourceRtl),
                new NamedParameter("targetDetokenizer", targetDetokenizer),
                new NamedParameter("isTargetRtl", isTargetRtl),
                new NamedParameter("alignmentSetId", alignmentSetId)
            );
            await viewModel.InitializeAsync();
            return viewModel;
        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <remarks>
        /// This is for use by the DI container; use <see cref="CreateAsync"/> instead to create and initialize an instance of this view model.
        /// </remarks>
        public AlignmentDisplayViewModel(List<EngineParallelTextRow> parallelTextRows,
            ParallelCorpusId parallelCorpusId,
            EngineStringDetokenizer sourceDetokenizer,
            bool isSourceRtl,
            EngineStringDetokenizer targetDetokenizer,
            bool isTargetRtl,
            AlignmentSetId alignmentSetId,
            NoteManager noteManager,
            IMediator mediator,
            IEventAggregator eventAggregator,
            ILifetimeScope lifetimeScope,
            ILogger<AlignmentDisplayViewModel> logger)
            : base(noteManager, mediator, eventAggregator, lifetimeScope, logger)
        {
            ParallelTextRows = parallelTextRows;
            ParallelCorpusId = parallelCorpusId;

            var sourceTokens = new List<Token>();
            foreach (var row in parallelTextRows)
            {
                if (row.SourceTokens != null)
                {
                    sourceTokens.AddRange(row.SourceTokens);
                }
            }
            SourceTokenMap = new TokenMap(sourceTokens, sourceDetokenizer, isSourceRtl);

            var targetTokens = new List<Token>();
            foreach (var row in parallelTextRows)
            {
                if (row.TargetTokens != null)
                {
                    targetTokens.AddRange(row.TargetTokens);
                }
            }
            TargetTokenMap = new TokenMap(targetTokens, targetDetokenizer, isTargetRtl);

            AlignmentSetId = alignmentSetId;
        }

        public async Task HandleAlignmentAddedAsync(AlignmentAddedMessage message, CancellationToken cancellationToken)
        {
            var alignment = message.Alignment;
            var alignedTokenPair = alignment.AlignedTokenPair;

            if (AlignmentManager!.Alignments!.All(a => a.AlignmentId!.Id != alignment.AlignmentId!.Id))
            {
                AlignmentManager!.Alignments!.Add(alignment);
            }

            var tokenDisplayViewModel = message.TargetTokenDisplayViewModel;
            await HighlightTokens(tokenDisplayViewModel.IsSource, tokenDisplayViewModel.AlignmentToken.TokenId);

            await Task.CompletedTask;
        }

        public async Task HandleAlignmentDeletedAsync(AlignmentDeletedMessage message, CancellationToken cancellationToken)
        {

            if (AlignmentManager!.Alignments!.Any(a => a.AlignmentId!.Id == message.Alignment.AlignmentId!.Id))
            {
                var alignment = AlignmentManager!.Alignments!.FirstOrDefault(a => a.AlignmentId == message.Alignment.AlignmentId);
                if (alignment != null)
                {
                    var alignmentRemoved = AlignmentManager!.Alignments!.Remove(alignment);
                    Logger!.LogInformation(alignmentRemoved ? 
                        $"Found and removed alignment with Id - '{message.Alignment.AlignmentId}'" : 
                        $"Did not find an alignment with Id - '{message.Alignment.AlignmentId}'");
                }
                
                await UnhighlightTokens();
            }
        }
    }
}
