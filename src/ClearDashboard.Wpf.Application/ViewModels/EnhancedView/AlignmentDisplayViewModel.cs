using System;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Translation;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Autofac;
using Autofac.Core.Lifetime;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class AlignmentDisplayViewModel : VerseDisplayViewModel
    {
        private readonly EngineParallelTextRow _parallelTextRow;
        private readonly AlignmentSet _alignmentSet;

        public override async Task InitializeAsync()
        {
            Alignments = await _alignmentSet.GetAlignments(new List<EngineParallelTextRow>() { _parallelTextRow });

            await base.InitializeAsync();
        }

        public AlignmentDisplayViewModel(EngineParallelTextRow parallelTextRow,
                                      EngineStringDetokenizer sourceDetokenizer,
                                      bool isSourceRtl,
                                      EngineStringDetokenizer targetDetokenizer,
                                      bool isTargetRtl,
                                      AlignmentSet alignmentSet,
                                      NoteManager noteManager, 
                                      IEventAggregator eventAggregator, 
                                      ILogger<VerseDisplayViewModel>? logger)
            : base(noteManager, eventAggregator, logger)
        {
            _parallelTextRow = parallelTextRow;
            if (parallelTextRow.SourceTokens != null)
            {
                SourceTokenMap = new TokenMap(parallelTextRow.SourceTokens, sourceDetokenizer, isSourceRtl);
            }
            if (parallelTextRow.TargetTokens != null)
            {
                TargetTokenMap = new TokenMap(parallelTextRow.TargetTokens, targetDetokenizer, isTargetRtl);
            }

            _alignmentSet = alignmentSet;
        }

        public static async Task<VerseDisplayViewModel> CreateAsync(IComponentContext componentContext, EngineParallelTextRow parallelTextRow, EngineStringDetokenizer sourceDetokenizer, bool isSourceRtl, EngineStringDetokenizer targetDetokenizer, bool isTargetRtl, AlignmentSet alignmentSet)
        {
            var verseDisplayViewModel = componentContext.Resolve<AlignmentDisplayViewModel>(
                new NamedParameter("parallelTextRow", parallelTextRow),
                new NamedParameter("sourceDetokenizer", sourceDetokenizer),
                new NamedParameter("isSourceRtl", isSourceRtl),
                new NamedParameter("targetDetokenizer", targetDetokenizer),
                new NamedParameter("isTargetRtl", isTargetRtl),
                new NamedParameter("alignmentSet", alignmentSet)
            );
            await verseDisplayViewModel.InitializeAsync();
            return verseDisplayViewModel;
        }
    }
}
