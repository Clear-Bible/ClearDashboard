using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DataAccessLayer.Features.Bcv;

namespace ClearDashboard.Wpf.Application.ViewModels.Corpus
{
    public class CorpusTokensViewModel : PaneViewModel,
        IHandle<ProjectDesignSurfaceViewModel.TokenizedTextCorpusLoadedMessage>, 
        IHandle<BackgroundTaskChangedMessage>,
        IHandle<VerseChangedMessage>,
        IHandle<ProjectChangedMessage>
    {

        #region Member Variables

        private CancellationTokenSource? _cancellationTokenSource;
        private bool? _handleAsyncRunning ;
        private string? _tokenizationType;
        private TokenizedTextCorpus? _currentTokenizedTextCorpus;
        private Visibility? _progressBarVisibility = Visibility.Visible;
        private ObservableCollection<TokensTextRow>? _verses;
        private string? _message;
        private BookInfo? _currentBook;
        private ObservableCollection<TokensTextRow>? _tokensTextRows;
        private bool InComingChangesStarted { get; set; }

        #endregion //Member Variables

        #region Public Properties

        #endregion //Public Properties

        #region Observable Properties

        private Dictionary<string, string> _bcvDictionary;

        private Dictionary<string, string> BCVDictionary
        {
            get => _bcvDictionary;
            set
            {
                _bcvDictionary = value;
                NotifyOfPropertyChange(() => BCVDictionary);
            }
        }

        private BookChapterVerseViewModel _currentBcv = new();
        public BookChapterVerseViewModel CurrentBcv
        {
            get => _currentBcv;
            set
            {
                _currentBcv = value;
                NotifyOfPropertyChange(() => CurrentBcv);
            }
        }

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

        private bool _paratextSync = true;
        public bool ParatextSync
        {
            get => _paratextSync;
            set
            {
                _paratextSync = value;
                NotifyOfPropertyChange(() => ParatextSync);
            }
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
            set
            {
                Set(ref _currentBook, value);
                NotifyOfPropertyChange<string>(() => CurrentBookDisplay);
            }
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

        public string CurrentBookDisplay => string.IsNullOrEmpty(CurrentBook?.Code) ? string.Empty : $"<{CurrentBook.Code}>";
        
        private string _currentCorpusName = string.Empty;
        public string CurrentCorpusName
        {
            get => _currentCorpusName;
            set
            {
                _currentCorpusName = value;
                NotifyOfPropertyChange(() => CurrentCorpusName);
            }
        }

        #endregion //Observable Properties

        #region Constructor

        public CorpusTokensViewModel()
        {
            // required by design-time binding
        }

        public CorpusTokensViewModel(INavigationService navigationService, ILogger<CorpusTokensViewModel> logger,
            DashboardProjectManager projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope)
            : base(navigationService: navigationService, logger: logger, projectManager: projectManager, eventAggregator: eventAggregator, mediator: mediator, lifetimeScope: lifetimeScope)
        {
            Title = "🗟 " + LocalizationStrings.Get("Windows_CorpusTokens", Logger);
            ContentId = "CORPUSTOKENS";

            BcvInit();
            ProgressBarVisibility = Visibility.Collapsed;
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

        #endregion //Constructor

        #region Methods

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


        private async void BcvInit()
        {
            // grab the dictionary of all the verse lookups
            //if (ProjectManager?.CurrentParatextProject is not null)
            //{
            //    BCVDictionary = ProjectManager.CurrentParatextProject.BcvDictionary;
            //}
            //else
            //{
            //    BCVDictionary = new Dictionary<string, string>();
            //}

            var context = await ProjectManager.ProjectNameDbContextFactory.GetDatabaseContext(ProjectManager.CurrentProject.ProjectName);
            var localCorpora = context.Corpa.Local;
            var result = await ProjectManager.Mediator.Send(new GetBcvDictionariesQuery(localCorpora));
            if (result.Success)
            {
                BCVDictionary = result.Data;
            }
            else
            {
                BCVDictionary = new Dictionary<string, string>();
            }

            InComingChangesStarted = true;

            // set the CurrentBcv prior to listening to the event
            CurrentBcv.SetVerseFromId(ProjectManager?.CurrentVerse);

            CalculateBooks();
            CalculateChapters();
            CalculateVerses();
            InComingChangesStarted = false;

            // Subscribe to changes of the Book Chapter Verse data object.
            CurrentBcv.PropertyChanged += BcvChanged;
        }

        private void BcvChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ParatextSync && InComingChangesStarted == false)
            {
                string verseId;
                bool somethingChanged = false;
                if (e.PropertyName == "BookNum")
                {
                    // book switch so find the first chapter and verse for that book
                    verseId = BCVDictionary.Values.First(b => b[..3] == CurrentBcv.Book);
                    if (verseId != "")
                    {
                        InComingChangesStarted = true;
                        CurrentBcv.SetVerseFromId(verseId);

                        CalculateChapters();
                        CalculateVerses();
                        InComingChangesStarted = false;
                        somethingChanged = true;
                    }
                }
                else if (e.PropertyName == "Chapter")
                {
                    // ReSharper disable once InconsistentNaming
                    var BBBCCC = CurrentBcv.Book + CurrentBcv.ChapterIdText;

                    // chapter switch so find the first verse for that book and chapter
                    verseId = BCVDictionary.Values.First(b => b.Substring(0, 6) == BBBCCC);
                    if (verseId != "")
                    {
                        InComingChangesStarted = true;
                        CurrentBcv.SetVerseFromId(verseId);

                        CalculateVerses();
                        InComingChangesStarted = false;
                        somethingChanged = true;
                    }
                }
                else if (e.PropertyName == "Verse")
                {
                    InComingChangesStarted = true;
                    CurrentBcv.SetVerseFromId(CurrentBcv.BBBCCCVVV);
                    InComingChangesStarted = false;
                    somethingChanged = true;
                }

                if (somethingChanged)
                {
                    // send to the event aggregator for everyone else to hear about a verse change
                    EventAggregator.PublishOnUIThreadAsync(new VerseChangedMessage(CurrentBcv.BBBCCCVVV));

                    // push to Paratext
                    if (ParatextSync)
                    {
                        _ = Task.Run(() => ExecuteRequest(new SetCurrentVerseCommand(CurrentBcv.BBBCCCVVV), CancellationToken.None));
                    }
                }

            }
        }
        private void CalculateBooks()
        {
            CurrentBcv.BibleBookList?.Clear();

            var books = BCVDictionary.Values.GroupBy(b => b.Substring(0, 3))
                .Select(g => g.First())
                .ToList();

            foreach (var book in books)
            {
                var bookId = book.Substring(0, 3);

                var bookName = BookChapterVerseViewModel.GetShortBookNameFromBookNum(bookId);

                CurrentBcv.BibleBookList?.Add(bookName);
            }

        }

        private void CalculateChapters()
        {
            // CHAPTERS
            var bookId = CurrentBcv.Book;
            var chapters = BCVDictionary.Values.Where(b => bookId != null && b.StartsWith(bookId)).ToList();
            for (int i = 0; i < chapters.Count; i++)
            {
                chapters[i] = chapters[i].Substring(3, 3);
            }

            chapters = chapters.DistinctBy(v => v).ToList().OrderBy(b => b).ToList();
            // invoke to get it to run in STA mode
            System.Windows.Application.Current.Dispatcher.Invoke(delegate
            {
                List<int> chapterNumbers = new List<int>();
                foreach (var chapter in chapters)
                {
                    chapterNumbers.Add(Convert.ToInt16(chapter));
                }

                CurrentBcv.ChapterNumbers = chapterNumbers;
            });
        }

        private void CalculateVerses()
        {
            // VERSES
            var bookId = CurrentBcv.Book;
            var chapId = CurrentBcv.ChapterIdText;
            var verses = BCVDictionary.Values.Where(b => b.StartsWith(bookId + chapId)).ToList();

            for (int i = 0; i < verses.Count; i++)
            {
                verses[i] = verses[i].Substring(6);
            }

            verses = verses.DistinctBy(v => v).ToList().OrderBy(b => b).ToList();
            // invoke to get it to run in STA mode
            System.Windows.Application.Current.Dispatcher.Invoke(delegate
            {
                List<int> verseNumbers = new List<int>();
                foreach (var verse in verses)
                {
                    verseNumbers.Add(Convert.ToInt16(verse));
                }

                CurrentBcv.VerseNumbers = verseNumbers;
            });
        }

        public ObservableCollection<TokensTextRow>? TokensTextRows
        {
            get => _tokensTextRows;
            set => Set(ref _tokensTextRows, value);
        }

        public async Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            if (CurrentBcv.BibleBookList.Count == 0)
            {
                return;
            }

            if (message.Verse != "" && CurrentBcv.BBBCCCVVV != message.Verse.PadLeft(9, '0'))
            {
                // send to log
                await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{this.DisplayName}: Project Change"), cancellationToken);

                InComingChangesStarted = true;
                CurrentBcv.SetVerseFromId(message.Verse);

                CalculateChapters();
                CalculateVerses();
                InComingChangesStarted = false;
            }

            return;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task HandleAsync(ProjectChangedMessage message, CancellationToken cancellationToken)
        {
            if (ProjectManager?.CurrentParatextProject is not null)
            {
                // send to log
                await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{this.DisplayName}: Project Change"), cancellationToken);


                //BCVDictionary = ProjectManager.CurrentParatextProject.BcvDictionary;
                InComingChangesStarted = true;

                // add in the books to the dropdown list
                CalculateBooks();

                // set the CurrentBcv prior to listening to the event
                CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);

                CalculateChapters();
                CalculateVerses();

                NotifyOfPropertyChange(() => CurrentBcv);
                InComingChangesStarted = false;
            }
            else
            {
                BCVDictionary = new Dictionary<string, string>();
            }

            return;
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
                    CurrentBook = message.ProjectMetadata.AvailableBooks.First();
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

                    CurrentTokenizedTextCorpus = await TokenizedTextCorpus.Get(ProjectManager.Mediator, new TokenizedTextCorpusId(message.TokenizedTextCorpusId));

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

        #endregion // Methods


        // ReSharper disable once UnusedMember.Global
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
