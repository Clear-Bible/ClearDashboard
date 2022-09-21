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
using ClearDashboard.Wpf.Application.Models;

namespace ClearDashboard.Wpf.Application.ViewModels.Project
{
    public class EnhancedCorpusViewModel : PaneViewModel,
        IHandle<ProjectDesignSurfaceViewModel.TokenizedTextCorpusLoadedMessage>,
        IHandle<BackgroundTaskChangedMessage>,
        IHandle<VerseChangedMessage>,
        IHandle<ProjectChangedMessage>,
        IHandle<BCVLoadedMessage>
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


        public List<TokenProject> _tokenProjects = new();

        
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
                if (value == true)
                {
                    // update Paratext with the verseId
                    //_ = Task.Run(() =>
                    //    ExecuteRequest(new SetCurrentVerseCommand(CurrentBcv.BBBCCCVVV), CancellationToken.None));

                    // update this window with the Paratext verse
                    CurrentBcv.SetVerseFromId(_projectManager.CurrentVerse);

                    //TODO regenerate the verses to display
                    
                }

                _paratextSync = value;
                NotifyOfPropertyChange(() => ParatextSync);
            }
        }

        private Dictionary<string, string> _bcvDictionary;
        public Dictionary<string, string> BcvDictionary
        {
            get => _bcvDictionary;
            set
            {
                _bcvDictionary = value;
                NotifyOfPropertyChange(() => BcvDictionary);
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

        private int _verseOffsetRange = 0;
        public int VerseOffsetRange
        {
            get => _verseOffsetRange;
            set
            {
                if (value != _verseOffsetRange)
                {
                    _verseOffsetRange = value;
                    NotifyOfPropertyChange(() => _verseOffsetRange);
                }
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

        private string _verseChange = string.Empty;
        public string VerseChange
        {
            get => _verseChange;
            set
            {
                if (_verseChange == "")
                {
                    _verseChange = value;
                    NotifyOfPropertyChange(() => VerseChange);
                }
                else if (_verseChange != value)
                {
                    // push to Paratext
                    if (ParatextSync)
                    {
                        _ = Task.Run(() =>
                            ExecuteRequest(new SetCurrentVerseCommand(CurrentBcv.BBBCCCVVV), CancellationToken.None));
                    }

                    _verseChange = value;
                    NotifyOfPropertyChange(() => VerseChange);
                }
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

            //BcvInit(_projectManager.CurrentParatextProject.Guid);
            ProgressBarVisibility = Visibility.Collapsed;
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            DisplayName = "Enhanced View";
            TokensTextRows = new ObservableCollection<TokensTextRow>();
            Verses = new ObservableCollection<TokensTextRow>();
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            BcvDictionary = _projectManager.CurrentParatextProject.BcvDictionary;
            CurrentBcv.SetVerseFromId(_projectManager.CurrentVerse);
            NotifyOfPropertyChange(() => CurrentBcv);
            VerseChange = _projectManager.CurrentVerse;

            base.OnViewAttached(view, context);
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

                //CalculateChapters();
                //CalculateVerses();
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

                InComingChangesStarted = true;

                // set the CurrentBcv prior to listening to the event
                CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);

                NotifyOfPropertyChange(() => CurrentBcv);
                InComingChangesStarted = false;
            }
            else
            {
                BcvDictionary = new Dictionary<string, string>();
            }
        }

        public async Task HandleAsync(ProjectDesignSurfaceViewModel.TokenizedTextCorpusLoadedMessage message, CancellationToken cancellationToken)
        {
            // we don't want this as it was for demonstration
            
            return;


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
            _logger?.LogInformation("Received TokenizedTextCorpusMessage.");
            _handleAsyncRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var localCancellationToken = _cancellationTokenSource.Token;
            
            ProgressBarVisibility = Visibility.Visible;

            TokenProject? project = null;
            // check if we have this already
            try
            {
                if (_tokenProjects.Count > 0)
                {
                    project = _tokenProjects.First(p =>
                    {
                        return p.ParatextProjectId == message.ParatextProjectId
                               && p.TokenizationType == message.TokenizationType;
                    }) ?? null;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            
            // existing project
            if (project is not null)
            {
                await Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        var metadata = project.Metadata;

                        CurrentTokenizedTextCorpus = project.TokenizedTextCorpus;
                        TokenizationType = project.TokenizationType;
                        CurrentBook = metadata?.AvailableBooks.First(b => b.Code == CurrentBcv.BookName);

                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                            new BackgroundTaskStatus
                            {
                                Name = "Fetch Book",
                                Description = $"Getting book '{CurrentBook?.Code}'...",
                                StartTime = DateTime.Now,
                                TaskStatus = StatusEnum.Working
                            }), cancellationToken);

                        // get the rows for the current book and chapter
                        var tokensTextRows = CurrentTokenizedTextCorpus[CurrentBook?.Code]
                            .GetRows()
                            .WithCancellation(localCancellationToken)
                            .Cast<TokensTextRow>()
                            .Where(ttr => ttr
                                .Tokens
                                .Count(t => t
                                    .TokenId
                                    .ChapterNumber == CurrentBcv.ChapterNum) > 0)
                            .ToList();


                        // get the row for the current verse
                        int index = 0;
                        for (int i = 0; i < tokensTextRows.Count; i++)
                        {
                            var verseRef = (SIL.Scripture.VerseRef)tokensTextRows[i].Ref;

                            if (verseRef.VerseNum == CurrentBcv.VerseNum)
                            {
                                index = i;
                                break;
                            }
                        }

                        var lowEnd = index - VerseOffsetRange;
                        if (lowEnd < 0)
                            lowEnd = 0;

                        var upperEnd = index + VerseOffsetRange;

                        // filter down to only these verses
                        var offset = upperEnd - lowEnd + 1;
                        var verseRangeRows = tokensTextRows.Skip(lowEnd).Take(offset).ToList();


                        OnUIThread(() =>
                        {
                            Verses = new ObservableCollection<TokensTextRow>(verseRangeRows);
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
            else
            {
                // current project
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
                                metadata = result.Data.FirstOrDefault(b =>
                                               b.Id == message.ParatextProjectId.Replace("-", "")) ??
                                           throw new InvalidOperationException();
                            }
                            else
                            {
                                throw new InvalidOperationException(result.Message);
                            }

                        }

                        CurrentTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(message.TokenizedTextCorpusId));

                        TokenizationType = message.TokenizationType;

                        CurrentBook = metadata?.AvailableBooks.First(b => b.Code == CurrentBcv.BookName);

                        // add this corpus to our master list
                        _tokenProjects.Add(new TokenProject
                        {
                            ParatextProjectId = message.ParatextProjectId,
                            ProjectName = message.ProjectName,
                            TokenizationType = message.TokenizationType,
                            CorpusId = message.CorpusId,
                            TokenizedTextCorpusId = message.TokenizedTextCorpusId,
                            Metadata = metadata,
                            TokenizedTextCorpus = CurrentTokenizedTextCorpus,
                        });

                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                            new BackgroundTaskStatus
                            {
                                Name = "Fetch Book",
                                Description = $"Getting book '{CurrentBook?.Code}'...",
                                StartTime = DateTime.Now,
                                TaskStatus = StatusEnum.Working
                            }), cancellationToken);

                        // get the rows for the current book and chapter
                        var tokensTextRows = CurrentTokenizedTextCorpus[CurrentBook?.Code]
                            .GetRows()
                            .WithCancellation(localCancellationToken)
                            .Cast<TokensTextRow>()
                            .Where(ttr => ttr
                                .Tokens
                                .Count(t => t
                                    .TokenId
                                    .ChapterNumber == CurrentBcv.ChapterNum) > 0)
                            .ToList();


                        // get the row for the current verse
                        int index = 0;
                        for (int i = 0; i < tokensTextRows.Count; i++)
                        {
                            var verseRef = (SIL.Scripture.VerseRef)tokensTextRows[i].Ref;

                            if (verseRef.VerseNum == CurrentBcv.VerseNum)
                            {
                                index = i;
                                break;
                            }
                        }

                        var lowEnd = index - VerseOffsetRange;
                        if (lowEnd < 0)
                            lowEnd = 0;

                        var upperEnd = index + VerseOffsetRange;

                        // filter down to only these verses
                        var offset = upperEnd - lowEnd + 1;
                        var verseRangeRows = tokensTextRows.Skip(lowEnd).Take(offset).ToList();


                        OnUIThread(() =>
                        {
                            Verses = new ObservableCollection<TokensTextRow>(verseRangeRows);
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
                            metadata = result.Data.FirstOrDefault(b =>
                                           b.Id == message.ParatextProjectId.Replace("-", "")) ??
                                       throw new InvalidOperationException();
                        }
                        else
                        {
                            throw new InvalidOperationException(result.Message);
                        }

                    }

                    CurrentTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(message.TokenizedTextCorpusId));

                    TokenizationType = message.TokenizationType;

                    CurrentBook = metadata?.AvailableBooks.First(b => b.Code == CurrentBcv.BookName);

                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Getting book '{CurrentBook?.Code}'...",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Working
                        }), cancellationToken);

                    // get the rows for the current book and chapter
                    var tokensTextRows = CurrentTokenizedTextCorpus[CurrentBook?.Code]
                        .GetRows()
                        .WithCancellation(localCancellationToken)
                        .Cast<TokensTextRow>()
                        .Where(ttr => ttr
                            .Tokens
                            .Count(t => t
                                .TokenId
                                .ChapterNumber == CurrentBcv.ChapterNum) > 0)
                        .ToList();


                    // get the row for the current verse
                    int index = 0;
                    for (int i = 0; i < tokensTextRows.Count; i++)
                    {
                        var verseRef = (SIL.Scripture.VerseRef)tokensTextRows[i].Ref;

                        if (verseRef.VerseNum == CurrentBcv.VerseNum)
                        {
                            index = i;
                            break;
                        }
                    }

                    var lowEnd = index - VerseOffsetRange;
                    if (lowEnd < 0)
                        lowEnd = 0;

                    var upperEnd = index + VerseOffsetRange;

                    // filter down to only these verses
                    var offset = upperEnd - lowEnd + 1;
                    var verseRangeRows = tokensTextRows.Skip(lowEnd).Take(offset).ToList();


                    OnUIThread(() =>
                    {
                        Verses = new ObservableCollection<TokensTextRow>(verseRangeRows);
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

        public Task HandleAsync(BCVLoadedMessage message, CancellationToken cancellationToken)
        {
            this.BcvDictionary = _projectManager.CurrentParatextProject.BcvDictionary;

            return Task.CompletedTask;
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
