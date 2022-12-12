using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Translation;
using System.Linq;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    internal class InterlinearDisplayViewModel : VerseDisplayViewModel
    {
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
                                      ILogger<VerseDisplayViewModel>? logger)
            : base(noteManager, eventAggregator, logger)
        {
            SourceTokenMap = new TokenMap(parallelTextRow.SourceTokens!, sourceDetokenizer, isSourceRtl);
            TranslationSet = translationSet;
        }
    }
}
