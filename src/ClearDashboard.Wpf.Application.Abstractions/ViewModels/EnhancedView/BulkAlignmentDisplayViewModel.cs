using System.Collections.Generic;
using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public class BulkAlignmentDisplayViewModel : VerseDisplayViewModel
{
    private readonly BulkAlignment _bulkAlignment;

    protected override Translation? GetTranslationForToken(Token token, CompositeToken? compositeToken)
    {
        return base.GetTranslationForToken(token, compositeToken);
    }

    public BulkAlignmentDisplayViewModel(BulkAlignment bulkAlignment,
        EngineStringDetokenizer sourceDetokenizer,
        bool isSourceRtl,
        NoteManager noteManager, 
        IMediator mediator, 
        IEventAggregator eventAggregator, 
        ILifetimeScope lifetimeScope, 
        ILogger logger) : base(noteManager, mediator, eventAggregator, lifetimeScope, logger)
    {
        _bulkAlignment = bulkAlignment;
        SourceTokenMap = new TokenMap(bulkAlignment.SourceVerseTokens, sourceDetokenizer, isSourceRtl);
    }

   
}