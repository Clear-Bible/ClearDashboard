using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Translation;
using System.Linq;
using Autofac;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class InterlinearDisplayViewModel : VerseDisplayViewModel
    {
        private readonly EngineParallelTextRow _parallelTextRow;

        public override async Task InitializeAsync()
        {
            Translations = await GetTranslations(TranslationSet, SourceTokenMap.Tokens.Select(t => t.TokenId));

            await base.InitializeAsync();
        }

        public InterlinearDisplayViewModel(EngineParallelTextRow parallelTextRow,
                                      EngineStringDetokenizer sourceDetokenizer,
                                      bool isSourceRtl,
                                      TranslationSet translationSet,
                                      NoteManager noteManager, 
                                      IEventAggregator eventAggregator, 
                                      ILifetimeScope lifetimeScope,
                                      ILogger<VerseDisplayViewModel> logger)
            : base(noteManager, eventAggregator, lifetimeScope, logger)
        {
            _parallelTextRow = parallelTextRow;
            if (parallelTextRow.SourceTokens != null)
            {
                SourceTokenMap = new TokenMap(parallelTextRow.SourceTokens!, sourceDetokenizer, isSourceRtl);
            }

            TranslationSet = translationSet;
        }

        public static async Task<VerseDisplayViewModel> CreateAsync(IComponentContext componentContext, EngineParallelTextRow parallelTextRow, EngineStringDetokenizer detokenizer, bool isRtl, TranslationSet translationSet)
        {
            var verseDisplayViewModel = componentContext.Resolve<InterlinearDisplayViewModel>(
                new NamedParameter("parallelTextRow", parallelTextRow),
                new NamedParameter("sourceDetokenizer", detokenizer),
                new NamedParameter("isSourceRtl", isRtl),
                new NamedParameter("translationSet", translationSet)
            );
            await verseDisplayViewModel.InitializeAsync();
            return verseDisplayViewModel;
        }
    }
}
