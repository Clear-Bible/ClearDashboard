using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Features.Bcv;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.Views.ParatextViews;
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

namespace ClearDashboard.Wpf.Application.ViewModels.Project
{
    public class EnhancedCorpusViewModel : PaneViewModel,
        IHandle<ProjectDesignSurfaceViewModel.TokenizedTextCorpusLoadedMessage>,
        IHandle<BackgroundTaskChangedMessage>,
        IHandle<VerseChangedMessage>,
        IHandle<ProjectChangedMessage>
    {

        #region Member Variables
        private readonly ILogger<EnhancedCorpusViewModel> _logger;
        private readonly DashboardProjectManager _projectManager;

        private CancellationTokenSource? _cancellationTokenSource;
        private bool? _handleAsyncRunning;
        private string? _tokenizationType;
        private TokenizedTextCorpus? _currentTokenizedTextCorpus;
        private Visibility? _progressBarVisibility = Visibility.Visible;
        private ObservableCollection<TokensTextRow>? _verses;
        private string? _message;
        private BookInfo? _currentBook;
        
        private bool InComingChangesStarted { get; set; }

        private string CurrentBookDisplay => string.IsNullOrEmpty(CurrentBook?.Code) ? string.Empty : $"<{CurrentBook.Code}>";

        #endregion //Member Variables

        #region Public Properties

        private string ContentID => this.ContentID;

        public bool IsRtl { get; set; }

        public Visibility TranslationControlVisibility { get; set; } = Visibility.Collapsed;

        #region BCV
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

        private int _verseRange = 1;
        public int VerseRange
        {
            get => _verseRange;
            set
            {
                _verseRange = value;
                NotifyOfPropertyChange(() => _verseRange);
            }
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


        #endregion BCV


        #endregion //Public Properties

        #region Observable Properties

        private ObservableCollection<TokensTextRow>? _tokensTextRows;
        public ObservableCollection<TokensTextRow>? TokensTextRows
        {
            get => _tokensTextRows;
            set => Set(ref _tokensTextRows, value);
        }
        
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

        #endregion //Observable Properties

        
        #region Constructor

        public EnhancedCorpusViewModel()
        {
            // required by design-time binding
        }

        public EnhancedCorpusViewModel(INavigationService navigationService, ILogger<EnhancedCorpusViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope) :
            base(navigationService: navigationService, logger: logger, projectManager: projectManager, eventAggregator: eventAggregator, mediator: mediator, lifetimeScope: lifetimeScope)
        {
            _logger = logger;
            _projectManager = projectManager;

            Title = "⳼ " + LocalizationStrings.Get("Windows_EnhancedCorpus", Logger);
            this.ContentId = "ENHANCEDCORPUS";

            BcvInit(_projectManager.CurrentParatextProject.Guid);
            ProgressBarVisibility = Visibility.Collapsed;
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            DisplayName = "Enhanced View";
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

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<TextCollectionsView>.Show(this, actualWidth, actualHeight);
        }

        private async void BcvInit(string paratextProjectId = "")
        {

            var result = await ProjectManager.Mediator.Send(new GetBcvDictionariesQuery(paratextProjectId));
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

            _logger?.LogInformation("Received TokenizedTextCorpusMessage.");
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
            BcvInit(message.ParatextProjectId);

            _logger?.LogInformation("Received TokenizedTextCorpusMessage.");
            _handleAsyncRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var localCancellationToken = _cancellationTokenSource.Token;

            ProgressBarVisibility = Visibility.Visible;

            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    ParatextProjectMetadata metadata;

                    if (message.ParatextProjectId == _projectManager?.ManuscriptGuid.ToString())
                    {
                        // our fake Manuscript corpus
                        var bookInfo = new BookInfo();
                        var books = bookInfo.GenerateScriptureBookList();

                        metadata = new ParatextProjectMetadata
                        {
                            Id = _projectManager.ManuscriptGuid.ToString(),
                            CorpusType = CorpusType.Manuscript,
                            Name = "Manuscript",
                            AvailableBooks = books,
                        };

                    }
                    else
                    {
                        // regular Paratext corpus
                        var result = await _projectManager?.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
                        if (result.Success && result.HasData)
                        {
                            metadata = result.Data.FirstOrDefault(b => b.Id == message.ParatextProjectId.Replace("-", "")) ?? throw new InvalidOperationException();
                        }
                        else
                        {
                            throw new InvalidOperationException(result.Message);
                        }

                    }

                    CurrentTokenizedTextCorpus = await TokenizedTextCorpus.Get(_projectManager.Mediator, new TokenizedTextCorpusId(message.TokenizedTextCorpusId));

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

        #region Event Handlers

        public void TokenClicked(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenDoubleClicked(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) double-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenRightButtonDown(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) right-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseEnter(TokenEventArgs e)
        {
            if (e.TokenDisplay.HasNote)
            {
                // DisplayNote(e);
            }

            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) hovered";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseLeave(TokenEventArgs e)
        {
            Message = string.Empty;
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseWheel(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) mouse wheel";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationClicked(TranslationEventArgs e)
        {
            // DisplayTranslation(e);

            Message = $"'{e.Translation.TargetTranslationText}' translation for token ({e.Translation.SourceToken.TokenId}) clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationDoubleClicked(TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token ({e.Translation.SourceToken.TokenId}) double-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationRightButtonDown(TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token ({e.Translation.SourceToken.TokenId}) right-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationMouseEnter(TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token ({e.Translation.SourceToken.TokenId}) hovered";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationMouseLeave(TranslationEventArgs e)
        {
            Message = string.Empty;
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationMouseWheel(TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token ({e.Translation.SourceToken.TokenId}) mouse wheel";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteLeftButtonDown(NoteEventArgs e)
        {
            Message = $"'{e.TokenDisplay.Note}' note for token ({e.TokenDisplay.Token.TokenId}) clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteDoubleClicked(NoteEventArgs e)
        {
            Message = $"'{e.TokenDisplay.Note}' note for token ({e.TokenDisplay.Token.TokenId}) double-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteRightButtonDown(NoteEventArgs e)
        {
            Message = $"'{e.TokenDisplay.Note}' note for token ({e.TokenDisplay.Token.TokenId}) right-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteMouseEnter(NoteEventArgs e)
        {
            Message = $"'{e.TokenDisplay.Note}' note for token ({e.TokenDisplay.Token.TokenId}) hovered";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteMouseLeave(NoteEventArgs e)
        {
            Message = string.Empty;
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteMouseWheel(NoteEventArgs e)
        {
            Message = $"'{e.TokenDisplay.Note}' note for token ({e.TokenDisplay.Token.TokenId}) mouse wheel";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationApplied(TranslationEventArgs e)
        {
            Message = $"Translation '{e.Translation.TargetTranslationText}' ({e.TranslationActionType}) applied to token '{e.TokenDisplay.SurfaceText}' ({e.TokenDisplay.Token.TokenId})";
            NotifyOfPropertyChange(nameof(Message));

            TranslationControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(TranslationControlVisibility));
        }

        public void TranslationCancelled(RoutedEventArgs e)
        {
            Message = "Translation cancelled.";
            NotifyOfPropertyChange(nameof(Message));

            TranslationControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(TranslationControlVisibility));
        }

        public void NoteApplied(NoteEventArgs e)
        {
            Message = $"Note '{e.TokenDisplay.Note}' applied to token '{e.TokenDisplay.SurfaceText}' ({e.TokenDisplay.Token.TokenId})";
            NotifyOfPropertyChange(nameof(Message));

            // NoteControlVisibility = Visibility.Hidden;
            // NotifyOfPropertyChange(nameof(NoteControlVisibility));
        }

        public void NoteCancelled(RoutedEventArgs e)
        {
            Message = "Note cancelled.";
            NotifyOfPropertyChange(nameof(Message));

            // NoteControlVisibility = Visibility.Hidden;
            // NotifyOfPropertyChange(nameof(NoteControlVisibility));
        }
        // ReSharper restore UnusedMember.Global

        #endregion
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
