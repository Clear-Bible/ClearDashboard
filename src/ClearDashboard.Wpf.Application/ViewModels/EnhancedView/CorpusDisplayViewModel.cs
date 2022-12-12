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
    public class CorpusDisplayViewModel : VerseDisplayViewModel
    {
        private readonly TokensTextRow _textRow;

        public override Task InitializeAsync()
        {
            SourceTokens = new TokenCollection(_textRow.Tokens.GetPositionalSortedBaseTokens());
            return base.InitializeAsync();
        }

        public CorpusDisplayViewModel(TokensTextRow textRow,
                                      EngineStringDetokenizer sourceDetokenizer,
                                      bool isRtl,
                                      NoteManager noteManager, 
                                      IEventAggregator eventAggregator, 
                                      ILogger<VerseDisplayViewModel>? logger)
            : base(noteManager, eventAggregator, logger)
        {
            _textRow = textRow;
            SourceDetokenizer = sourceDetokenizer;
            IsSourceRtl = isRtl;
        }
    }
}
