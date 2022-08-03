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
                NotifyOfPropertyChange(() => DatabaseVerseTokensText);
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
            Logger.LogInformation("Received TokenizedTextCorpusMessage.");

            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    // wait to allow the UI to catch up.
                    await Task.Delay(250, cancellationToken);

                    await SendProgressBarVisibilityMessage(true);

                    await SendProgressBarMessage("Retrieving verse tokens.");
                    var corpus = message.TokenizedTextCorpus;

                    await SendProgressBarMessage("Getting book '40'");
                    var book = corpus.Where(row => ((VerseRef)row.Ref).BookNum == 40);
                    await SendProgressBarMessage("Got book '40'");
                    await SendProgressBarMessage("Getting Chapter '1'");
                    var chapter = book.Where(row => ((VerseRef)row.Ref).ChapterNum == 1);
                    await SendProgressBarMessage("Got Chapter '1'");
                    await SendProgressBarMessage("Getting Verse '1'");
                    var verse = chapter.First(row => ((VerseRef)row.Ref).VerseNum == 1) as TokensTextRow;
                    await SendProgressBarMessage("Got Verse '1'");
                    OnUIThread(() =>
                        {
                            VerseTokens = new VerseTokens("40", "1",
                                verse.Tokens.Where(t => t.TokenId.BookNumber == 40), true);
                        });

                    await SendProgressBarMessage("Completed retrieving verse tokens.");


                }
                finally
                {
                    await SendProgressBarVisibilityMessage(false);
                }

            }, cancellationToken);

        }
    }
}
