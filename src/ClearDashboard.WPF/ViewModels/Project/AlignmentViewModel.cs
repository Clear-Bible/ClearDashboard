using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using SIL.Scripture;

namespace ClearDashboard.Wpf.ViewModels.Project
{
    public class AlignmentViewModel : PaneViewModel, IHandle<TokenizedTextCorpusLoadedMessage>
    {
        private VerseTokens _verseTokens;
        private string _message;

        public AlignmentViewModel()
        {

        }

        public AlignmentViewModel(INavigationService navigationService, ILogger<AlignmentViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            Title = "⳼ ALIGNMENT TOOL";
            ContentId = "ALIGNMENTTOOL";
            DisplayName = "⳼ ALIGNMENT TOOL";
        }

        public void TokenBubbleLeftClicked(string target)
        {
            Message = $"'{target}' left-clicked";
        }

        public void TokenBubbleRightClicked(string target)
        {
            Message = $"'{target}' right-clicked";
  }

        public void TokenBubbleMouseEntered(string target)
        {
            Message = $"Hovering over '{target}'";
        }

        public void TokenBubbleMouseLeft(string target)
        {
            Message = string.Empty;
        }

        public VerseTokens VerseTokens
        {
            get => _verseTokens;
            set
            {
                Set(ref _verseTokens, value);
                NotifyOfPropertyChange(()=> DatabaseVerseTokensText);
            }
        }

        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        public List<string> DatabaseVerseTokensText => VerseTokens != null ? VerseTokens.Tokens.Select(t => t.SurfaceText).ToList() : new List<string>();

        public async Task HandleAsync(TokenizedTextCorpusLoadedMessage message, CancellationToken cancellationToken)
        {

            var corpus = message.TokenizedTextCorpus;

            Logger.LogInformation("Getting book '40'");
            var book = corpus.Where(row => ((VerseRef)row.Ref).BookNum == 40);
            Logger.LogInformation("Got book '40'");
            Logger.LogInformation("Getting Chapter '1'");
            var chapter = book.Where(row => ((VerseRef)row.Ref).ChapterNum == 1);
            Logger.LogInformation("Got Chapter '1'");
            Logger.LogInformation("Getting Verse '1'");
            var verse = chapter.First(row => ((VerseRef)row.Ref).VerseNum == 1) as TokensTextRow;
            Logger.LogInformation("Got Verse '1'");
           // OnUIThread(()=>{
                VerseTokens = new VerseTokens("40", "1", verse.Tokens.Where(t => t.TokenId.BookNumber == 40), true);
           // });

            await Task.CompletedTask;
        }
    }
}
