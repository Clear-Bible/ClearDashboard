using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Corpus
{
    public class CorpusTokensViewModel : PaneViewModel,
        IHandle<ProjectDesignSurfaceViewModel.TokenizedTextCorpusLoadedMessage>, IHandle<BackgroundTaskChangedMessage>
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private bool? _handleAsyncRunning ;
        private string? _tokenizationType;
        private TokenizedTextCorpus? _currentTokenizedTextCorpus;
        private Visibility? _progressBarVisibility = Visibility.Visible;
        private ObservableCollection<TokensTextRow>? _verses;
        private string? _message;
        private BookInfo? _currentBook;
        private ObservableCollection<TokensTextRow>? _tokensTextRows;


        public string? TokenizationType
        {
            get => _tokenizationType;
            set => Set(ref _tokenizationType, value);
        }

        public TokenizedTextCorpus? CurrentTokenizedTextCorpus
        {
            get => _currentTokenizedTextCorpus;
            set => Set(ref _currentTokenizedTextCorpus, value);
        }
      
        public Visibility? ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set
            {
                _progressBarVisibility = value;
                NotifyOfPropertyChange(() => ProgressBarVisibility);
            }
        }

        // ReSharper disable once UnusedMember.Global
        public CorpusTokensViewModel()
        {
            // required by design-time binding
        }

        public CorpusTokensViewModel(INavigationService navigationService, ILogger<CorpusTokensViewModel> logger,
            DashboardProjectManager projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope)
            : base(navigationService: navigationService, logger: logger, projectManager: projectManager, eventAggregator: eventAggregator, mediator: mediator, lifetimeScope: lifetimeScope)
        {
            Title = "🗟 CORPUS TOKENS";
            ContentId = "CORPUSTOKENS";
            ProgressBarVisibility = Visibility.Collapsed;
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
            if (_handleAsyncRunning.HasValue && _handleAsyncRunning.Value)
            {
                _cancellationTokenSource?.Cancel();
                EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                {
                    Name = "Fetch Book",
                    Description = "Task was cancelled",
                    EndTime = DateTime.Now,
                    TaskStatus = StatusEnum.Completed
                }), cancellationToken);
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }

     
        public ObservableCollection<TokensTextRow>? Verses
        {
            get => _verses;
            set => Set(ref _verses, value);
        }

       
        public string? Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

      

        public BookInfo? CurrentBook
        {
            get => _currentBook;
            set => Set(ref _currentBook, value);
        }

      
        //public string CurrentBookCode
        //{
        //    get => _currentBookCode;
        //    set
        //    {
        //        Set(ref _currentBookCode, value);
        //        NotifyOfPropertyChange(() => CurrentBookDisplay);
        //    }
        //}

        public string CurrentBookDisplay => string.IsNullOrEmpty(CurrentBook?.Code) ? string.Empty : $"Book: {CurrentBook.Code}";

     
        public ObservableCollection<TokensTextRow>? TokensTextRows
        {
            get => _tokensTextRows;
            set => Set(ref _tokensTextRows, value);
        }

        public async Task HandleAsync(ProjectDesignSurfaceViewModel.TokenizedTextCorpusLoadedMessage message, CancellationToken cancellationToken)
        {
            
            Logger?.LogInformation("Received TokenizedTextCorpusMessage.");
            _handleAsyncRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var localCancellationToken = _cancellationTokenSource.Token;
            ProgressBarVisibility = Visibility.Visible;
            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    CurrentTokenizedTextCorpus = message.TokenizedTextCorpus;
                    TokenizationType = message.TokenizationName;
                    var firstBook = message.ProjectMetadata.AvailableBooks.First();
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Getting book '{CurrentBook?.Code}'...",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Working
                        }), cancellationToken);

                    var tokensTextRows =
                        CurrentTokenizedTextCorpus[CurrentBook?.Code]
                            .GetRows()
                            .WithCancellation(localCancellationToken)
                            .Cast<TokensTextRow>()
                            .Where(ttr => ttr
                                .Tokens
                                .Count(t => t
                                    .TokenId
                                    .ChapterNumber == firstBook.Number) > 0)
                            .ToList();
                 
                    OnUIThread(() =>
                    {
                        Verses = new ObservableCollection<TokensTextRow>(tokensTextRows);
                        ProgressBarVisibility = Visibility.Collapsed;
                    });
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Found {tokensTextRows.Count} TokensTextRow entities.",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Completed
                        }), cancellationToken);
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
                            }), cancellationToken);
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
                _cancellationTokenSource?.Cancel();

                // return that your task was cancelled
                incomingMessage.EndTime = DateTime.Now;
                incomingMessage.TaskStatus = StatusEnum.Completed;
                incomingMessage.Description = "Task was cancelled";

                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(incomingMessage), cancellationToken);
            }
            await Task.CompletedTask;
        }


        public async Task ShowCorpusTokens(ShowTokenizationWindowMessage message, CancellationToken cancellationToken)
        {
            Logger?.LogInformation("Received TokenizedTextCorpusMessage.");
            _handleAsyncRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var localCancellationToken = _cancellationTokenSource.Token;

            ProgressBarVisibility = Visibility.Visible;

            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    ParatextProjectMetadata metadata;

                    if (message.ParatextProjectId == ProjectManager?.ManuscriptGuid.ToString())
                    {
                        // our fake Manuscript corpus
                        var bookInfo = new BookInfo();
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
                        var result = await ProjectManager?.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
                        if (result.Success && result.HasData)
                        {
                            metadata = result.Data.FirstOrDefault(b => b.Id == message.ParatextProjectId.Replace("-", "")) ?? throw new InvalidOperationException();
                        }
                        else
                        {
                            throw new InvalidOperationException(result.Message);
                        }
                        
                    }

                    CurrentTokenizedTextCorpus = await TokenizedTextCorpus.Get(ProjectManager.Mediator, new TokenizedTextCorpusId(message.tokenizedTextCorpusId));

                    TokenizationType = message.TokenizationType;

                    CurrentBook = metadata?.AvailableBooks.First();
                  
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Getting book '{CurrentBook?.Code}'...",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Working
                        }), cancellationToken);

                    var tokensTextRows =
                        CurrentTokenizedTextCorpus[CurrentBook?.Code]
                            .GetRows()
                            .WithCancellation(localCancellationToken)
                            .Cast<TokensTextRow>()
                            .Where(ttr => ttr
                                .Tokens
                                .Count(t => t
                                    .TokenId
                                    .ChapterNumber == CurrentBook?.Number) > 0)
                            .ToList();
                    
                    OnUIThread(() =>
                    {
                        Verses = new ObservableCollection<TokensTextRow>(tokensTextRows);
                        ProgressBarVisibility = Visibility.Collapsed;
                    });
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Found {tokensTextRows.Count} TokensTextRow entities.",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Completed
                        }), cancellationToken);
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
                            }), cancellationToken);
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

    static class CancelExtension
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
