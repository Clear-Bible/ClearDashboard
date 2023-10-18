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
    public class InterlinearEnhancedViewItemViewModel : VerseAwareEnhancedViewItemViewModel
    {
        public InterlinearEnhancedViewItemViewModel(
            DashboardProjectManager? projectManager, 
            IEnhancedViewManager enhancedViewManager,  
            INavigationService? navigationService, 
            ILogger<VerseAwareEnhancedViewItemViewModel>? logger, 
            IEventAggregator? eventAggregator, 
            IMediator? mediator, ILifetimeScope? 
            lifetimeScope, 
            IWindowManager windowManager, 
            ILocalizationService localizationService,
            NoteManager noteManager) : 
            base(
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

        public void EnterPressed()
        {
           Logger.LogInformation("InterlinearEnhancedViewItemViewModel - Enter has been pressed");;
        }

        public void CtrlEnterPressed()
        {
            Logger.LogInformation("InterlinearEnhancedViewItemViewModel - Ctrl+Enter has been pressed"); 
        }

        public void ShiftEnterPressed()
        {
            Logger.LogInformation("InterlinearEnhancedViewItemViewModel - Shift+Enter has been pressed");
          
        }

        public void Save()
        {
            Logger.LogInformation("InterlinearEnhancedViewItemViewModel - Ctrl+S has been pressed");
        }

        public void AltEnterPressed()
        {
            Logger.LogInformation("InterlinearEnhancedViewItemViewModel - Alt+Enter has been pressed");
        }

        protected override IEnumerable<IRow> Rows
        {
            get
            {
                var verses = (EnhancedViewItemMetadatum as InterlinearEnhancedViewItemMetadatum)
                    ?.ParallelCorpus
                    ?.GetByVerseRange(
                        new VerseRef(ParentViewModel.CurrentBcv.GetBBBCCCVVV()),
                        (ushort)ParentViewModel.VerseOffsetRange,
                        (ushort)ParentViewModel.VerseOffsetRange)
                    ?? throw new InvalidDataEngineException(name: "metadata or parallelcorpus", value: "null");
                return verses.parallelTextRows;
            }
        }
        protected override List<TokenizedTextCorpusId> TokenizedTextCorpusIds
        {
            get
            {
                var metadatum = EnhancedViewItemMetadatum as InterlinearEnhancedViewItemMetadatum;
                return new List<TokenizedTextCorpusId>()
                {
                    metadatum?.ParallelCorpus?.ParallelCorpusId?.SourceTokenizedCorpusId
                        ?? throw new InvalidStateEngineException(name: "metadatum, metadatum.ParallelCorpus, or ParallelCorpusId", value: "null"),
                    metadatum.ParallelCorpus.ParallelCorpusId.TargetTokenizedCorpusId
                        ?? throw new InvalidStateEngineException(name: "metadatum, metadatum.ParallelCorpus, or ParallelCorpusId", value: "null")
                };
            }
        }

    }
}
