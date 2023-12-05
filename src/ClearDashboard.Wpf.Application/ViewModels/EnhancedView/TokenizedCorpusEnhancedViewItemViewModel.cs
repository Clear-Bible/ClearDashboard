using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Scripture;
using System.Collections.Generic;


// ReSharper disable InconsistentNaming

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class TokenizedCorpusEnhancedViewItemViewModel : VerseAwareEnhancedViewItemViewModel
    {
        public TokenizedCorpusEnhancedViewItemViewModel(
            DashboardProjectManager? projectManager,
            IEnhancedViewManager enhancedViewManager,
            INavigationService? navigationService,
            ILogger<VerseAwareEnhancedViewItemViewModel>? logger,
            IEventAggregator? eventAggregator,
            IMediator? mediator, ILifetimeScope? lifetimeScope,
            IWindowManager windowManager,
            ILocalizationService localizationService,
            NoteManager noteManager) : base(
                projectManager,
                enhancedViewManager,
                navigationService,
                logger,
                eventAggregator,
                mediator,
                lifetimeScope,
                windowManager,
                localizationService,
                noteManager)
        {
        }

        protected override IEnumerable<IRow> Rows
        {
            get
            {
                var verses = (EnhancedViewItemMetadatum as TokenizedCorpusEnhancedViewItemMetadatum)
                    ?.TokenizedTextCorpus
                    ?.GetByVerseRange(new VerseRef(
                        ParentViewModel.CurrentBcv.GetBBBCCCVVV()),
                        (ushort)ParentViewModel.VerseOffsetRange,
                        (ushort)ParentViewModel.VerseOffsetRange)
                    ?? throw new InvalidDataEngineException(name: "metadata or parallelcorpus", value: "null");
                return verses.textRows;
            }
        }
        protected override List<TokenizedTextCorpusId> TokenizedTextCorpusIds
        {
            get
            {
                var metadatum = EnhancedViewItemMetadatum as TokenizedCorpusEnhancedViewItemMetadatum;
                return new List<TokenizedTextCorpusId>()
                {
                    metadatum?.TokenizedTextCorpus?.TokenizedTextCorpusId
                        ?? throw new InvalidStateEngineException(name: "metadatum or TokenizedTextCorpus", value: "null"),
                };
            }
        }
    }
}
