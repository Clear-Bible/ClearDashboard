using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Extensions;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace ClearDashboard.Wpf.Application.ViewModels
{
    public class CorpusTokensViewModel : PaneViewModel, IHandle<ProjectDesignSurfaceViewModel.TokenizedTextCorpusLoadedMessage>
    {


        public CorpusTokensViewModel()
        {
            // required by design-time binding
        }

        public CorpusTokensViewModel(INavigationService navigationService, ILogger logger,
            DashboardProjectManager projectManager, IEventAggregator eventAggregator, IMediator mediator) 
            : base(navigationService, logger, projectManager, eventAggregator, mediator)
        {
            Title = "🗟 CORPUS TOKENS";
            ContentId = "CORPUSTOKENS";
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
            DisplayName = "Corpus Tokens";
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

        private string _currentBook;
        public string CurrentBook
        {
            get => _currentBook;
            set
            {
                Set(ref _currentBook, value);
                NotifyOfPropertyChange(() => CurrentBookDisplay);
            }
        }

        public string CurrentBookDisplay => string.IsNullOrEmpty(CurrentBook) ? string.Empty : $"Book: {CurrentBook}";

        private ObservableCollection<TokensTextRow> _tokensTextRows;
        public ObservableCollection<TokensTextRow> TokensTextRows
        {
            get => _tokensTextRows;
            set => Set(ref _tokensTextRows, value);
        }

        public async Task HandleAsync(ProjectDesignSurfaceViewModel.TokenizedTextCorpusLoadedMessage message, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Received TokenizedTextCorpusMessage.");

            await Task.Factory.StartNew(async () =>
            {
                try
                {


                    // IMPORTANT: wait to allow the UI to catch up - otherwise toggling the progress bar visibility may fail.
                    await SendProgressBarVisibilityMessage(true, 250);

                    var corpus = message.TokenizedTextCorpus;

                    CurrentBook = message.ProjectMetadata.AvailableBooks.First().Code;

                    await SendProgressBarMessage($"Getting book '{CurrentBook}'");

                    var tokensTextRows = corpus[CurrentBook].GetRows().Cast<TokensTextRow>()
                   .Where(ttr => ttr.Tokens.Count(t => t.TokenId.ChapterNumber == 1) > 0).ToList();

                    await SendProgressBarMessage($"Found {tokensTextRows.Count} TokensTextRow entities.");

                    OnUIThread(() =>
                    {
                        TokensTextRows = new ObservableCollection<TokensTextRow>(tokensTextRows);
                        Verses = new ObservableCollection<VerseTokens>(tokensTextRows.CreateVerseTokens());
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
