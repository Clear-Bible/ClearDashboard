using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    internal class CorpusDisplayViewModel : VerseDisplayViewModel
    {
        public CorpusDisplayViewModel(TokensTextRow textRow,
                                      EngineStringDetokenizer detokenizer,
                                      bool isRtl,
                                      NoteManager noteManager, 
                                      IEventAggregator eventAggregator, 
                                      ILogger<VerseDisplayViewModel>? logger)
            : base(noteManager, eventAggregator, logger)
        {
            SourceTokenMap = new TokenMap(textRow.Tokens, detokenizer);
            IsSourceRtl = isRtl;
        }
    }
}
