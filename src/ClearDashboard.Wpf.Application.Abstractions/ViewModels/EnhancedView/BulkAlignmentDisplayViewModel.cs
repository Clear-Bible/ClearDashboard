using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public class BulkAlignmentDisplayViewModel : VerseDisplayViewModel
{
    private readonly BulkAlignment _bulkAlignment;


    public BulkAlignmentDisplayViewModel(BulkAlignment bulkAlignment,
        EngineStringDetokenizer sourceDetokenizer,
        bool isSourceRtl,
        NoteManager noteManager, 
        IMediator mediator, 
        IEventAggregator eventAggregator, 
        ILifetimeScope lifetimeScope, 
        ILogger<BulkAlignmentDisplayViewModel> logger) : base(noteManager, mediator, eventAggregator, lifetimeScope, logger)
    {
        _bulkAlignment = bulkAlignment;
        SourceTokenMap = new TokenMap(bulkAlignment.SourceVerseTokens, sourceDetokenizer, isSourceRtl);
    }

    public static async Task<BulkAlignmentDisplayViewModel> CreateAsync(IComponentContext componentContext, BulkAlignment bulkAlignment, EngineStringDetokenizer sourceDetokenizer, bool isSourceRtl)
    {
        var viewModel = componentContext.Resolve<BulkAlignmentDisplayViewModel>(
            new NamedParameter("bulkAlignment", bulkAlignment),
                            new NamedParameter("sourceDetokenizer", sourceDetokenizer),
                            new NamedParameter("isSourceRtl", isSourceRtl));
        await viewModel.InitializeAsync();
        return viewModel;
    }


}