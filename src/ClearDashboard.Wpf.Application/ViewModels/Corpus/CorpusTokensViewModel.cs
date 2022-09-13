using System;
using System.Collections.Generic;
using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Extensions;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.ViewModels;
using ClearDashboard.Wpf.Application.ViewModels.Project;


namespace ClearDashboard.Wpf.Application.ViewModels.Corpus
{
    public class CorpusTokensViewModel : PaneViewModel,
        IHandle<ProjectDesignSurfaceViewModel.TokenizedTextCorpusLoadedMessage>, IHandle<BackgroundTaskChangedMessage>
    {
        private CancellationTokenSource _cancellationTokenSource = null;
        private bool _handleAsyncRunning = false;


        private readonly ILogger<CorpusTokensViewModel> _logger;
        public string TokenizationName
        {
            get => _tokenizationName;
            set => Set(ref _tokenizationName, value);
        }
        public TokenizedTextCorpus CurrentTokenizedTextCorpus
        {
            get => _currentTokenizedTextCorpus;
            set => Set(ref _currentTokenizedTextCorpus, value);
        }

        private TokenizedTextCorpus _currentTokenizedTextCorpus;
        private string _tokenizationName;

        
        public CorpusTokensViewModel()
        {
            // required by design-time binding
        }

        public CorpusTokensViewModel(INavigationService navigationService, ILogger<CorpusTokensViewModel> logger,
            DashboardProjectManager projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) 
            : base(navigationService: navigationService, logger: logger, projectManager: projectManager, eventAggregator: eventAggregator, mediator: mediator, lifetimeScope: lifetimeScope)
        {
            _logger = logger;

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
            Verses = new ObservableCollection<TokensTextRow>();
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

        private ObservableCollection<TokensTextRow> _verses;
        public ObservableCollection<TokensTextRow> Verses
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

        //public async Task HandleAsync(ProjectDesignSurfaceViewModel.TokenizedTextCorpusLoadedMessage message, CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("Received TokenizedTextCorpusMessage.");

        //    _handleAsyncRunning = true;
        //    _cancellationTokenSource = new CancellationTokenSource();
        //    var localCancellationToken = _cancellationTokenSource.Token;

        //    await Task.Factory.StartNew(async () =>
        //    {
        //        try
        //        {
        //            // IMPORTANT: wait to allow the UI to catch up - otherwise toggling the progress bar visibility may fail.

        //            var corpus = message.TokenizedTextCorpus;

        //            CurrentBook = message.ProjectMetadata.AvailableBooks.First().Code;

        //            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
        //                new BackgroundTaskStatus
        //                {
        //                    Name = "Fetch Book",
        //                    Description = $"Getting book '{CurrentBook}'...",
        //                    StartTime = DateTime.Now,
        //                    TaskStatus = StatusEnum.Working
        //                }));

        //            var tokensTextRows =
        //                corpus[CurrentBook]
        //                    .GetRows()
        //                    .WithCancellation(localCancellationToken)
        //                    .Cast<TokensTextRow>()
        //                    .Where(ttr => ttr
        //                        .Tokens
        //                        .Count(t => t
        //                            .TokenId
        //                            .ChapterNumber == 1) > 0)
        //                    .ToList();

        //            OnUIThread(() =>
        //            {
        //                TokensTextRows = new ObservableCollection<TokensTextRow>(tokensTextRows);
        //                Verses = new ObservableCollection<VerseTokens>(tokensTextRows.CreateVerseTokens());
        //            });

        //            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
        //                new BackgroundTaskStatus
        //                {
        //                    Name = "Fetch Book",
        //                    Description = $"Found {tokensTextRows.Count} TokensTextRow entities.",
        //                    StartTime = DateTime.Now,
        //                    TaskStatus = StatusEnum.Completed
        //                }));
        //        }
        //        catch (Exception ex)
        //        {
        //            if (!localCancellationToken.IsCancellationRequested)
        //            {
        //                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
        //                    new BackgroundTaskStatus
        //                    {
        //                        Name = "Fetch Book",
        //                        EndTime = DateTime.Now,
        //                        ErrorMessage = $"{ex}",
        //                        TaskStatus = StatusEnum.Error
        //                    }));
        //            }
        //        }
        //        finally
        //        {
        //            _handleAsyncRunning = false;

        //            _cancellationTokenSource.Dispose();
        //        }

        //    }, cancellationToken);

        //}

        public async Task HandleAsync(ProjectDesignSurfaceViewModel.TokenizedTextCorpusLoadedMessage message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received TokenizedTextCorpusMessage.");
            _handleAsyncRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var localCancellationToken = _cancellationTokenSource.Token;
            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    CurrentTokenizedTextCorpus = message.TokenizedTextCorpus;
                    TokenizationName = message.TokenizationName;
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
                        CurrentTokenizedTextCorpus[CurrentBook]
                            .GetRows()
                            .WithCancellation(localCancellationToken)
                            .Cast<TokensTextRow>()
                            .Where(ttr => ttr
                                .Tokens
                                .Count(t => t
                                    .TokenId
                                    .ChapterNumber == 1) > 0)
                            .ToList();
                    //var tokensWithPadding = GetTokensWithPadding(tokensTextRows[0]);
                    //var tokenDisplays = tokensWithPadding.Select(t => new TokenDisplay
                    //    { Token = t.token, PaddingAfter = t.paddingAfter, PaddingBefore = t.paddingBefore });
                    OnUIThread(() =>
                    {
                        // TokensDisplays = new ObservableCollection<TokenDisplay>(tokenDisplays);
                        Verses = new ObservableCollection<TokensTextRow>(tokensTextRows);
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
                }
                finally
                {
                    _handleAsyncRunning = false;
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


        public async Task ShowCorpusTokens(ShowTokenizationWindowMessage message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received TokenizedTextCorpusMessage.");
            _handleAsyncRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var localCancellationToken = _cancellationTokenSource.Token;
            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    ParatextProjectMetadata metadata;

                    if (message.ParatextProjectId == ProjectManager.ManuscriptGuid.ToString())
                    {
                        // our fake Manuscript corpus
                        BookInfo bookInfo = new BookInfo();
                        var books = bookInfo.GenerateScriptureBookList();

                        metadata = new ParatextProjectMetadata
                        {
                            Id = ProjectManager.ManuscriptGuid.ToString(),
                            CorpusType = CorpusType.Manuscript,
                            Name = "Manuscript",
                            AvailableBooks = books,
                        };

                    }
                    else
                    {
                        // regular Paratext corpus
                        var result = await ProjectManager.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
                        metadata = result.Data.FirstOrDefault(b => b.Id == message.ParatextProjectId.Replace("-", ""));
                    }

                    CurrentTokenizedTextCorpus = await TokenizedTextCorpus.Get(
                        ProjectManager.Mediator,
                        new TokenizedTextCorpusId(message.tokenizedTextCorpusId));
                    TokenizationName = message.TokenizationType;
                    CurrentBook = metadata.AvailableBooks.First().Code;
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Getting book '{CurrentBook}'...",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Working
                        }));
                    var tokensTextRows =
                        CurrentTokenizedTextCorpus[CurrentBook]
                            .GetRows()
                            .WithCancellation(localCancellationToken)
                            .Cast<TokensTextRow>()
                            .Where(ttr => ttr
                                .Tokens
                                .Count(t => t
                                    .TokenId
                                    .ChapterNumber == 1) > 0)
                            .ToList();
                    //var tokensWithPadding = GetTokensWithPadding(tokensTextRows[0]);
                    //var tokenDisplays = tokensWithPadding.Select(t => new TokenDisplay
                    //    { Token = t.token, PaddingAfter = t.paddingAfter, PaddingBefore = t.paddingBefore });
                    OnUIThread(() =>
                    {
                        // TokensDisplays = new ObservableCollection<TokenDisplay>(tokenDisplays);
                        Verses = new ObservableCollection<TokensTextRow>(tokensTextRows);
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
                }
                finally
                {
                    _handleAsyncRunning = false;
                    _cancellationTokenSource.Dispose();
                }
            }, cancellationToken);

        }
    }

    static class CancelExtention
    {
        public static IEnumerable<T> WithCancellation<T>(this IEnumerable<T> en, CancellationToken token)
        {
            foreach (var item in en)
            {
                token.ThrowIfCancellationRequested();
                yield return item;
            }
        }
    }
}
