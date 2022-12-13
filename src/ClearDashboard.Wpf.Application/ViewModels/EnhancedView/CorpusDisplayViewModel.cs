using System;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    internal class CorpusDisplayViewModel : VerseDisplayViewModel
    {
        public CorpusDisplayViewModel(TokensTextRow textRow,
                                      EngineStringDetokenizer detokenizer,
                                      bool isRtl,
                                      NoteManager noteManager, 
                                      IEventAggregator eventAggregator,
                                      ILifetimeScope lifetimeScope,
                                      ILogger<VerseDisplayViewModel> logger)
            : base(noteManager, eventAggregator, lifetimeScope, logger)
        {
            SourceTokenMap = new TokenMap(textRow.Tokens, detokenizer, isRtl);
        }

        public static async Task<VerseDisplayViewModel> CreateAsync(IComponentContext componentContext, TokensTextRow textRow, EngineStringDetokenizer detokenizer, bool isRtl)
        {
            var verseDisplayViewModel = componentContext!.Resolve<CorpusDisplayViewModel>(
                new NamedParameter("textRow", textRow),
                new NamedParameter("detokenizer", detokenizer),
                new NamedParameter("isRtl", isRtl)
            );
            await verseDisplayViewModel.InitializeAsync();
            return verseDisplayViewModel;
        }
    }
}
