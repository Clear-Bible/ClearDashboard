﻿using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.Collections.Notes;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public class BulkAlignmentDisplayViewModel : VerseDisplayViewModel
{
    private readonly BulkAlignment _bulkAlignment;
    
    public BulkAlignmentDisplayViewModel(BulkAlignment bulkAlignment,
        ParallelCorpus parallelCorpus,
        NoteManager noteManager, 
        IMediator mediator, 
        IEventAggregator eventAggregator, 
        ILifetimeScope lifetimeScope, 
        ILogger<BulkAlignmentDisplayViewModel> logger) : base(noteManager, mediator, eventAggregator, lifetimeScope, logger)
    {
        _bulkAlignment = bulkAlignment;
        SourceTokenMap = new TokenMap(bulkAlignment.SourceVerseTokens, (parallelCorpus.SourceCorpus as TokenizedTextCorpus)!);
        TargetTokenMap = new TokenMap(bulkAlignment.TargetVerseTokens, (parallelCorpus.TargetCorpus as TokenizedTextCorpus)!);
    }

    public string DisplayTokens => string.Join(" ", _bulkAlignment.SourceVerseTokens.Select(token => token.TrainingText));

    public static async Task<BulkAlignmentDisplayViewModel> CreateAsync(IComponentContext componentContext, 
        BulkAlignment bulkAlignment, 
        ParallelCorpus parallelCorpus
        )
    {
        var viewModel = componentContext.Resolve<BulkAlignmentDisplayViewModel>(
            new NamedParameter("bulkAlignment", bulkAlignment),
            new NamedParameter("parallelCorpus", parallelCorpus));
        await viewModel.InitializeAsync();
        return viewModel;
    }

    protected override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        
        var sourceToken = _bulkAlignment.SourceVerseTokens.ToArray()[(int)_bulkAlignment.SourceVerseTokensIndex];
        var targetToken = _bulkAlignment.TargetVerseTokens.ToArray()[(int)_bulkAlignment.TargetVerseTokensIndex];

        var sourceTokenDisplayViewModel =
            SourceTokenDisplayViewModels.FirstOrDefault(t => t.Token.TokenId.Equals(sourceToken.TokenId));
        if (sourceTokenDisplayViewModel != null)
        {

            sourceTokenDisplayViewModel.IsHighlighted = true;
        }

        var targetTokenDisplayViewModel =
            TargetTokenDisplayViewModels.FirstOrDefault(t => t.Token.TokenId.Equals(targetToken.TokenId));
        if (targetTokenDisplayViewModel != null)
        {
            targetTokenDisplayViewModel.IsHighlighted = true;
        }

    }

    protected override async Task<NoteIdCollection> GetNoteIdsForToken(TokenId tokenId)
    {
        return await Task.FromResult(new NoteIdCollection());
    }
}