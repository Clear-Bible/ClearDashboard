
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
using ClearDashboard.DAL.Alignment.Extensions;

namespace ClearDashboard.Wpf.ViewModels.Project
{
    public class CorpusTokensViewModel : PaneViewModel, IHandle<TokenizedTextCorpusLoadedMessage>, IHandle<BackgroundTaskChangedMessage>
    {
        private CancellationTokenSource _cancellationTokenSource = null;
        private bool _handleAsyncRunning = false;

       public CorpusTokensViewModel()
        {
            // required by design-time binding
        }

        public CorpusTokensViewModel(INavigationService navigationService, ILogger<CorpusTokensViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            Title = "🗟 CORPUS TOKENS";
            ContentId = "CORPUSTOKENS";
            _cancellationTokenSource = new CancellationTokenSource();
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

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            //we need to cancel this process here
            //check a bool to see if it already cancelled or already completed
            if (_handleAsyncRunning)
            {
                _cancellationTokenSource.Cancel();
                EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                {
                    Name = "Fetch Book",
                    Description = "Task was cancelled",
                    EndTime = DateTime.Now,
                    TaskStatus = StatusEnum.Completed
                }));
            }
            return base.OnDeactivateAsync(close, cancellationToken);
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
                NotifyOfPropertyChange(()=> CurrentBookDisplay);
            }
        }

        public string CurrentBookDisplay => string.IsNullOrEmpty(CurrentBook) ? string.Empty : $"Book: {CurrentBook}";

        private ObservableCollection<TokensTextRow> _tokensTextRows;
        public ObservableCollection<TokensTextRow> TokensTextRows
        {
            get => _tokensTextRows;
            set => Set( ref _tokensTextRows, value);
        }

        public async Task HandleAsync(TokenizedTextCorpusLoadedMessage message, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Received TokenizedTextCorpusMessage.");
            _handleAsyncRunning = true;
            var localCancellationToken = _cancellationTokenSource.Token;

            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    // IMPORTANT: wait to allow the UI to catch up - otherwise toggling the progress bar visibility may fail.
                    //await SendProgressBarVisibilityMessage(true, 250);

                    var corpus = message.TokenizedTextCorpus;

                    CurrentBook = message.ProjectMetadata.AvailableBooks.First().Code;
                    
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Getting book '{CurrentBook}'...",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Working
                        }));
                    
                    var tokensTextRows = 
                        corpus[CurrentBook]
                            .GetRows()
                            .WithCancellation(localCancellationToken)
                            .Cast<TokensTextRow>()
                            .Where(ttr => ttr
                                .Tokens
                                .Count(t => t
                                    .TokenId
                                    .ChapterNumber == 1) > 0)
                            .ToList();

                    OnUIThread(() =>
                    {
                        TokensTextRows = new ObservableCollection<TokensTextRow>(tokensTextRows);
                        Verses = new ObservableCollection<VerseTokens>(tokensTextRows.CreateVerseTokens());
                    });
                    
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Found {tokensTextRows.Count} TokensTextRow entities.",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Completed
                        }));
                }
                catch (Exception ex)
                {
                    if (!localCancellationToken.IsCancellationRequested)
                    {
                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            EndTime = DateTime.Now,
                            ErrorMessage = $"{ex}",
                            TaskStatus = StatusEnum.Error
                        }));
                    }

                    _handleAsyncRunning = false;
                }
                finally
                {
                    _cancellationTokenSource.Dispose();
                }
            }, cancellationToken);

        }
        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            var incomingMessage = message.Status;

            if (incomingMessage.Name == "Fetch Book" && incomingMessage.TaskStatus == StatusEnum.CancelTaskRequested)
            {
                _cancellationTokenSource.Cancel();

                // return that your task was cancelled
                incomingMessage.EndTime = DateTime.Now;
                incomingMessage.TaskStatus = StatusEnum.Completed;
                incomingMessage.Description = "Task was cancelled";

                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(incomingMessage));
            }
            await Task.CompletedTask;
        }
    }

    static class CancelExtention
    {
        public static IEnumerable<T> WithCancellation<T>(this IEnumerable<T> en, CancellationToken token)
        {
            foreach (var item in en)
            {
                Thread.Sleep(5000);
                token.ThrowIfCancellationRequested();
                yield return item;
            }
        }
    }
}
