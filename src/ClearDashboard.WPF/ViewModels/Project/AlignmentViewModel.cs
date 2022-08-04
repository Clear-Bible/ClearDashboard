using System;
using System.Collections.Generic;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.Wpf.ViewModels.Project
{
    public class AlignmentViewModel : PaneViewModel, IHandle<TokenizedTextCorpusLoadedMessage>
    {
       

        public AlignmentViewModel()
        {
            // required by design-time binding
        }

        public AlignmentViewModel(INavigationService navigationService, ILogger<AlignmentViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            Title = "⳼ ALIGNMENT TOOL";
            ContentId = "ALIGNMENTTOOL";
            
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

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            DisplayName = "⳼ ALIGNMENT TOOL";
            TokensTextRows = new ObservableCollection<TokensTextRow>();
            Verses = new ObservableCollection<VerseTokens>();
            return base.OnInitializeAsync(cancellationToken);
        }

        private ObservableCollection<VerseTokens> _verses;
        public ObservableCollection<VerseTokens> Verses
        {
            get => _verses;
            set => Set(ref _verses, value);
        }

        private string _message;
       
        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        private ObservableCollection<TokensTextRow> _tokensTextRows;
        public ObservableCollection<TokensTextRow> TokensTextRows
        {
            get => _tokensTextRows;
            set => Set( ref _tokensTextRows, value);
        }

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

                    var corpus = message.TokenizedTextCorpus;
                    await SendProgressBarMessage("Getting book '40', Chapter '1'");

                    //var tokensTextRows = corpus["MAL"].GetRows().Cast<TokensTextRow>().ToList() ;

                    // var tokensTextRows = corpus["MAL"].GetRows().Cast<TokensTextRow>()
                    //.Where(ttr =>ttr.Tokens.Count(t => t.TokenId.ChapterNumber == 1 && t.TokenId.VerseNumber == 1) > 0).ToList();

                    var tokensTextRows = corpus["MAL"].GetRows().Cast<TokensTextRow>()
                   .Where(ttr => ttr.Tokens.Count(t => t.TokenId.ChapterNumber == 1) > 0).ToList();

                    var verseTokensList = CreateVerseTokensList(tokensTextRows);

                    await SendProgressBarMessage($"Found {tokensTextRows.Count} TokensTextRow entities.");

                    OnUIThread(()=>
                    {
                        
                        TokensTextRows = new ObservableCollection<TokensTextRow>(tokensTextRows);
                        Verses = new ObservableCollection<VerseTokens>(verseTokensList);
                    });
                     
                    await SendProgressBarMessage("Completed retrieving verse tokens.");
                }
                finally
                {
                    await SendProgressBarVisibilityMessage(false);
                }

            }, cancellationToken);

        }

        private List<VerseTokens> CreateVerseTokensList(List<TokensTextRow> tokensTextRows)
        {
            return (from row in tokensTextRows let firstToken = row.Tokens.FirstOrDefault() 
                where firstToken != null 
                let tokenId = firstToken.TokenId 
                select 
                    new VerseTokens(tokenId.ChapterNumber.ToString(), tokenId.VerseNumber.ToString(), row.Tokens, row.IsSentenceStart))
                .ToList();
        }
    }
}
