using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Translation;
using System.Linq;
using Autofac;
using Autofac.Core.Lifetime;

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

        public static async Task<VerseDisplayViewModel> CreateAsync(IComponentContext componentContext, EngineParallelTextRow parallelTextRow, EngineStringDetokenizer detokenizer, bool isRtl, TranslationSet translationSet)
        {
            var verseDisplayViewModel = componentContext!.Resolve<InterlinearDisplayViewModel>(
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
