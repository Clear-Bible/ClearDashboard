using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.ViewModels.Display;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.Views.ParatextViews;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Notes;
using ClearDashboard.DAL.Alignment.Features.Translation;
using ClearDashboard.DAL.Alignment.Translation;
using EngineToken = ClearBible.Engine.Corpora.Token;
using Label = ClearDashboard.DAL.Alignment.Notes.Label;
using Note = ClearDashboard.DAL.Alignment.Notes.Note;
using ParallelCorpus = ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;
using TranslationSet = ClearDashboard.DataAccessLayer.Models.TranslationSet;

namespace ClearDashboard.Wpf.Application.ViewModels.Project
{
    public class EnhancedViewModel : PaneViewModel,
        IHandle<ProjectDesignSurfaceViewModel.TokenizedTextCorpusLoadedMessage>,
        IHandle<BackgroundTaskChangedMessage>,
        IHandle<VerseChangedMessage>,
        IHandle<ProjectChangedMessage>,
        IHandle<BCVLoadedMessage>
    {

        #region Commands

        public ICommand MoveCorpusDownRow0 { get; set; }
        public ICommand MoveCorpusDownRow1 { get; set; }
        public ICommand MoveCorpusDownRow2 { get; set; }

        public ICommand MoveCorpusUpRow1 { get; set; }
        public ICommand MoveCorpusUpRow2 { get; set; }
        public ICommand MoveCorpusUpRow3 { get; set; }



        #endregion


        #region Member Variables
        private readonly ILogger<EnhancedViewModel> _logger;
        private readonly DashboardProjectManager _projectManager;

        private CancellationTokenSource? _cancellationTokenSource;
        private bool? _handleAsyncRunning;
        private string? _tokenizationType;
        private TokenizedTextCorpus? _currentTokenizedTextCorpus;
        private Visibility? _progressBarVisibility = Visibility.Visible;
        private string? _message;
        private BookInfo? _currentBook;

        private bool InComingChangesStarted { get; set; }

        private string CurrentBookDisplay => string.IsNullOrEmpty(CurrentBook?.Code) ? string.Empty : $"<{CurrentBook.Code}>";


        public List<TokenProject> _tokenProjects = new();
        public List<ParallelCorpus> _parallelProjects = new();
        public List<ShowTokenizationWindowMessage> _projectMessages = new();

        public Dictionary<IId, IEnumerable<Note>> NotesDictionary { get; set; }
        public DAL.Alignment.Translation.TranslationSet CurrentTranslationSet { get; set; }
        public IDetokenizer<string, string>? Detokenizer { get; set; } = new LatinWordDetokenizer();
        public IEnumerable<Translation> CurrentTranslations { get; set; }
        public IEnumerable<Label> LabelSuggestions { get; set; }

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
                    VerseChangeRerender();
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

                    VerseChangeRerender();
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

        private VersesDisplay _versesDisplay = new();
        public VersesDisplay VersesDisplay
        {
            get => _versesDisplay;
            set => Set(ref _versesDisplay, value);
        }


        private ObservableCollection<TokensTextRow>? _verses;
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

        public EnhancedViewModel()
        {
            // required by design-time binding
        }


        //private ICommand? _itemDoubleClickCommand;

        //private void ItemDoubleClickCommandHandler()
        //{
        //    MessageBox.Show("ItemDoubleClick");
        //}

        //private void ItemDoubleClickCommandHandler(object obj)
        //{
        //    MessageBox.Show("ItemDoubleClick Parameter");
        //}


        public EnhancedViewModel(INavigationService navigationService, ILogger<EnhancedViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator, IMediator mediator,
            ILifetimeScope? lifetimeScope) :
            base(navigationService: navigationService, logger: logger, projectManager: projectManager,
                eventAggregator: eventAggregator, mediator: mediator, lifetimeScope: lifetimeScope)
        {
            //_itemDoubleClickCommand = new RelayCommand(ItemDoubleClickCommandHandler);

            _logger = logger;
            _projectManager = projectManager;

            Title = "⳼ " + LocalizationStrings.Get("Windows_EnhancedView", Logger);
            this.ContentId = "ENHANCEDVIEW";

            //BcvInit(_projectManager.CurrentParatextProject.Guid);
            ProgressBarVisibility = Visibility.Collapsed;


            MoveCorpusDownRow0 = new RelayCommand(MoveCorpusDown0);
            MoveCorpusDownRow1 = new RelayCommand(MoveCorpusDown1);
            MoveCorpusDownRow2 = new RelayCommand(MoveCorpusDown2);

            MoveCorpusUpRow1 = new RelayCommand(MoveCorpusUp1);
            MoveCorpusUpRow2 = new RelayCommand(MoveCorpusUp2);
            MoveCorpusUpRow3 = new RelayCommand(MoveCorpusUp3);
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            DisplayName = "Enhanced View";
            TokensTextRows = new ObservableCollection<TokensTextRow>();
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
                    TaskLongRunningProcessStatus = LongRunningProcessStatus.Completed
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

            //return;


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
                            TaskLongRunningProcessStatus = LongRunningProcessStatus.Working
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
                            TaskLongRunningProcessStatus = LongRunningProcessStatus.Completed
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
                                TaskLongRunningProcessStatus = LongRunningProcessStatus.Error
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

            if (incomingMessage.Name == "Fetch Book" && incomingMessage.TaskLongRunningProcessStatus == LongRunningProcessStatus.CancelTaskRequested)
            {
                _cancellationTokenSource?.Cancel();

                // return that your task was cancelled
                incomingMessage.EndTime = DateTime.Now;
                incomingMessage.TaskLongRunningProcessStatus = LongRunningProcessStatus.Completed;
                incomingMessage.Description = "Task was cancelled";

                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(incomingMessage), cancellationToken);
            }
            await Task.CompletedTask;
        }

        public async Task ShowParallelTranslationTokens(ShowParallelTranslationWindowMessage message, CancellationToken cancellationToken)
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
                    //project = _tokenProjects.First(p =>
                    //{
                    //    return p.ParatextProjectId == message.ParatextProjectId
                    //           && p.TokenizationType == message.TokenizationType;
                    //}) ?? null;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            // existing project
            if (project is not null)
            {
                //await ShowExistingCorpusTokens(message, cancellationToken, project, localCancellationToken);
            }
            else
            {
                await ShowNewParallelTranslation(message, cancellationToken, localCancellationToken);
            }

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

            ProgressBarVisibility = Visibility.Visible;
            // existing project
            if (project is not null)
            {
                await ShowExistingCorpusTokens(message, cancellationToken, project, localCancellationToken);
            }
            else
            {
                await ShowNewCorpusTokens(message, cancellationToken, localCancellationToken);
            }

        }

        private async Task ShowNewParallelTranslation(ShowParallelTranslationWindowMessage message,
            CancellationToken cancellationToken, CancellationToken localCancellationToken)
        {
            // current project
            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    //await ProjectManager.LoadProject("SUR");
                    var row = await VerseTextRow(Convert.ToInt32(CurrentBcv.BBBCCCVVV), message);
                    NotesDictionary = await Note.GetAllDomainEntityIdNotes(Mediator);
                    CurrentTranslationSet = await GetTranslationSet();
                    CurrentTranslations = await CurrentTranslationSet.GetTranslations(row.SourceTokens.Select(t => t.TokenId));
                    var VerseTokens = GetTokenDisplayViewModels(row.SourceTokens);
                    LabelSuggestions = await GetLabelSuggestions();


                    //OnUIThread(() =>
                    //{
                    //    Verses = new ObservableCollection<TokensTextRow>(verseRangeRows);

                    //    UpdateVersesDisplay(message, verses, title, false);
                    //    NotifyOfPropertyChange(() => VersesDisplay);

                    //    ProgressBarVisibility = Visibility.Collapsed;
                    //});
                    //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                    //    new BackgroundTaskStatus
                    //    {
                    //        Name = "Fetch Book",
                    //        Description = $"Found {tokensTextRows.Count} TokensTextRow entities.",
                    //        StartTime = DateTime.Now,
                    //        TaskLongRunningProcessStatus = LongRunningProcessStatus.Completed
                    //    }), cancellationToken);
                }
                catch (Exception ex)
                {
                    ProgressBarVisibility = Visibility.Collapsed;
                    if (!localCancellationToken.IsCancellationRequested)
                    {
                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                            new BackgroundTaskStatus
                            {
                                Name = "Fetch Book",
                                EndTime = DateTime.Now,
                                ErrorMessage = $"{ex}",
                                TaskLongRunningProcessStatus = LongRunningProcessStatus.Error
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

        public async Task<EngineParallelTextRow?> VerseTextRow(int BBBCCCVVV, ShowParallelTranslationWindowMessage message)
        {
            try
            {
                var corpusIds = await ParallelCorpus.GetAllParallelCorpusIds(Mediator);
                var guid = Guid.Parse(message.ParallelCorpusId);
                var corpus = await ParallelCorpus.Get(Mediator, corpusIds.First(p => p.Id == guid));
                var verse = corpus.GetByVerseRange(new VerseRef(BBBCCCVVV), 0, 0);
                //var verse = corpus.FirstOrDefault(row => ((VerseRef)row.Ref).BBBCCCVVV == BBBCCCVVV) as EngineParallelTextRow;

                return verse.parallelTextRows.FirstOrDefault() as EngineParallelTextRow;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task<Dictionary<IId, IEnumerable<Note>>> GetAllDomainEntityIdNotes(
            IMediator mediator)
        {
            var command = new GetAllNotesQuery();

            var result = await mediator.Send(command);
            if (result.Success)
            {
                // result.Data is in the form:  IEnumerable<Note>
                // This linq uses SelectMany to extract out note / domain entity id pairs,
                // groups them by domain entity id (using an IIdEquatable comparer) and writes
                // the resulting IIdEquatable, INumerable<Note> groups to a dictionary:
                var idNotes = result.Data!
                    .SelectMany(note => note.DomainEntityIds, (note, iid) => new { iid, note })
                    .GroupBy(pair => pair.iid, new IIdEquatableComparer())
                    .ToDictionary(g => (g.Key as IId)!, g => g.Select(g => g.note));

                // result.Data is in the form:  Dictionary<Note, IdEquatableCollection>
                // This linq reverses that, grouping the appropriate Note references
                // under their associated IIdEquatable(s):
                //var idNotes = result.Data!
                //    .SelectMany(nd => nd.Value, (nd, iid) => new { iid, note = nd.Key })
                //    .GroupBy(pair => pair.iid)
                //    .ToDictionary(g => g.Key, g => g.Select(g => g.note) /*.ToList() as ICollection<Note>*/);
                return idNotes;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async Task<DAL.Alignment.Translation.TranslationSet?> GetTranslationSet()
        {
            try
            {
                var translationSetIds = await DAL.Alignment.Translation.TranslationSet.GetAllTranslationSetIds(Mediator);
                var translationSet = await DAL.Alignment.Translation.TranslationSet.Get(translationSetIds.First().translationSetId, Mediator);

                return translationSet;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task<IEnumerable<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, UserId userId)>>
            GetAllTranslationSetIds(IMediator mediator, ParallelCorpusId? parallelCorpusId = null, UserId? userId = null)
        {
            var result = await mediator.Send(new GetAllTranslationSetIdsQuery(parallelCorpusId, userId));
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        private List<TokenDisplayViewModel> GetTokenDisplayViewModels(IEnumerable<EngineToken> tokens)
        {
            var tokenDisplays = new List<TokenDisplayViewModel>();
            var paddedTokens = GetPaddedTokens(tokens);

            if (paddedTokens != null)
            {
                tokenDisplays.AddRange(from paddedToken in paddedTokens
                    let translation = GetTranslation(paddedToken.token)
                    let notes = GetNotes(paddedToken.token)
                    select new TokenDisplayViewModel
                    {
                        Token = paddedToken.token,
                        PaddingBefore = paddedToken.paddingBefore,
                        PaddingAfter = paddedToken.paddingAfter,
                        Translation = translation,
                        Notes = notes
                    });
            }

            return tokenDisplays;
        }

        private IEnumerable<(ClearBible.Engine.Corpora.Token token, string paddingBefore, string paddingAfter)>? GetPaddedTokens(IEnumerable<ClearBible.Engine.Corpora.Token> tokens)
        {
            var detokenizer = new EngineStringDetokenizer(Detokenizer);
            return detokenizer.Detokenize(tokens);
        }


        private async Task<ObservableCollection<DAL.Alignment.Notes.Label>> GetLabelSuggestions()
        {
            var labels = await DAL.Alignment.Notes.Label.GetAll(Mediator);
            return new ObservableCollection<Label>(labels);
        }

        private ObservableCollection<Note> GetNotes(EngineToken token)
        {
            return NotesDictionary.ContainsKey(token.TokenId) ? new ObservableCollection<Note>(NotesDictionary[token.TokenId])
                : new ObservableCollection<Note>();
        }









        private async Task ShowNewCorpusTokens(ShowTokenizationWindowMessage message, CancellationToken cancellationToken,
            CancellationToken localCancellationToken)
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

                    // get the entirety of text for this corpus
                    CurrentTokenizedTextCorpus =
                        await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(message.TokenizedTextCorpusId));

                    
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

                    // add in the message so we can get it later
                    _projectMessages.Add(message);

                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Getting book '{CurrentBook?.Code}'...",
                            StartTime = DateTime.Now,
                            TaskLongRunningProcessStatus = LongRunningProcessStatus.Working
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
                        var verseRef = (VerseRef)tokensTextRows[i].Ref;

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

                    // set the title to include the verse range
                    string title = message.ProjectName + " - " + message.TokenizationType;
                    if (verseRangeRows.Count == 1)
                    {
                        title += $" ({CurrentBcv.BookName} {CurrentBcv.ChapterNum}:{CurrentBcv.VerseNum})";
                    }
                    else
                    {
                        var startNum = (VerseRef)verseRangeRows[0].Ref;
                        var endNum = (VerseRef)verseRangeRows[^1].Ref;
                        title += $" ({CurrentBcv.BookName} {CurrentBcv.ChapterNum}:{startNum.VerseNum} - {endNum.VerseNum})";
                    }



                    // combine verse list into one TokensTextRow
                    ObservableCollection<List<TokenDisplayViewModel>> verses = new();

                    foreach (var verseRangeRow in verseRangeRows)
                    {
                        var verseRef = (VerseRef)verseRangeRow.Ref;
                        List<TokensTextRow> corpus = new List<TokensTextRow>();
                        corpus.Add(verseRangeRow);

                        // add in the gloss
                        var tokens = GetTokens(corpus, verseRef.BBBCCCVVV);
                        if (tokens != null)
                        {
                            var tokenDisplays = new List<TokenDisplayViewModel>();
                            tokenDisplays.AddRange(from token in tokens
                                                   let translation = GetTranslation(token.token)
                                                   select new TokenDisplayViewModel
                                                   {
                                                       Token = token.token,
                                                       PaddingBefore = token.paddingBefore,
                                                       PaddingAfter = token.paddingAfter,
                                                       Translation = translation
                                                   });
                            verses.Add(tokenDisplays);
                        }
                    }


                    OnUIThread(() =>
                    {
                        //Verses = new ObservableCollection<TokensTextRow>(verseRangeRows);

                        UpdateVersesDisplay(message, verses, title, false);
                        NotifyOfPropertyChange(() => VersesDisplay);

                        ProgressBarVisibility = Visibility.Collapsed;
                    });
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Found {tokensTextRows.Count} TokensTextRow entities.",
                            StartTime = DateTime.Now,
                            TaskLongRunningProcessStatus = LongRunningProcessStatus.Completed
                        }), cancellationToken);
                }
                catch (Exception ex)
                {
                    ProgressBarVisibility = Visibility.Collapsed;
                    if (!localCancellationToken.IsCancellationRequested)
                    {
                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                            new BackgroundTaskStatus
                            {
                                Name = "Fetch Book",
                                EndTime = DateTime.Now,
                                ErrorMessage = $"{ex}",
                                TaskLongRunningProcessStatus = LongRunningProcessStatus.Error
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

        private async Task ShowExistingCorpusTokens(ShowTokenizationWindowMessage message, CancellationToken cancellationToken,
            TokenProject project, CancellationToken localCancellationToken)
        {
            _logger.LogInformation("ShowExistingCorpusTokens: {0} {1}", message.CorpusId, message.ProjectName);

            await Task.Run(async () =>
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
                            TaskLongRunningProcessStatus = LongRunningProcessStatus.Working
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

                    // set the title to include the verse range
                    string title = message.ProjectName + " - " + message.TokenizationType;
                    if (verseRangeRows.Count == 1)
                    {
                        title += $" ({CurrentBcv.BookName} {CurrentBcv.ChapterNum}:{CurrentBcv.VerseNum})";
                    }
                    else
                    {
                        var startNum = (VerseRef)verseRangeRows[0].Ref;
                        var endNum = (VerseRef)verseRangeRows[^1].Ref;
                        title += $" ({CurrentBcv.BookName} {CurrentBcv.ChapterNum}:{startNum.VerseNum} - {endNum.VerseNum})";
                    }


                    // combine verse list into one TokensTextRow
                    ObservableCollection<List<TokenDisplayViewModel>> verses = new();

                    foreach (var verseRangeRow in verseRangeRows)
                    {
                        var verseRef = (VerseRef)verseRangeRow.Ref;
                        List<TokensTextRow> corpus = new List<TokensTextRow>();
                        corpus.Add(verseRangeRow);

                        var tokens = GetTokens(corpus, verseRef.BBBCCCVVV);
                        if (tokens != null)
                        {
                            var tokenDisplays = new List<TokenDisplayViewModel>();
                            tokenDisplays.AddRange(from token in tokens
                                                   let translation = GetTranslation(token.token)
                                                   select new TokenDisplayViewModel
                                                   {
                                                       Token = token.token,
                                                       PaddingBefore = token.paddingBefore,
                                                       PaddingAfter = token.paddingAfter,
                                                       Translation = translation
                                                   });
                            verses.Add(tokenDisplays);
                        }
                    }


                    OnUIThread(() =>
                    {
                        Verses = new ObservableCollection<TokensTextRow>(verseRangeRows);

                        UpdateVersesDisplay(message, verses, title, false);
                        ProgressBarVisibility = Visibility.Collapsed;
                    });
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Found {tokensTextRows.Count} TokensTextRow entities.",
                            StartTime = DateTime.Now,
                            TaskLongRunningProcessStatus = LongRunningProcessStatus.Completed
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
                                TaskLongRunningProcessStatus = LongRunningProcessStatus.Error
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

        private async Task VerseChangeRerender()
        {
            for (int i = 0; i < _tokenProjects.Count; i++)
            {
                _cancellationTokenSource = new CancellationTokenSource();

                await ShowExistingCorpusTokens(_projectMessages[i], _cancellationTokenSource.Token, _tokenProjects[i],
                    _cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        private void UpdateVersesDisplay(ShowTokenizationWindowMessage message, ObservableCollection<List<TokenDisplayViewModel>> verses, string title, bool ShowTranslations)
        {
            // same color as defined in SharedVisualTemplates.xaml
            Brush brush = Brushes.Blue;
            switch (message.CorpusType)
            {
                case CorpusType.Standard:
                    var converter = new BrushConverter();
                    brush = (Brush)converter.ConvertFromString("#7FC9FF");
                    break;
                case CorpusType.BackTranslation:
                    brush = Brushes.Orange;
                    break;
                case CorpusType.Resource:
                    brush = Brushes.PaleGoldenrod;
                    break;
                case CorpusType.Unknown:
                    brush = Brushes.Silver;
                    break;
                case CorpusType.Manuscript:
                    brush = Brushes.MediumOrchid;
                    break;
                default:
                    brush = Brushes.Blue;
                    break;
            }

            for (int i = 0; i < 4; i++)
            {
                if (VersesDisplay.Row0Verses.Count == 0 ||
                    VersesDisplay.Row0CorpusId == message.CorpusId)
                {
                    VersesDisplay.Row0CorpusId = message.CorpusId;
                    VersesDisplay.Row0Title = title;
                    VersesDisplay.Row0Verses = verses;
                    VersesDisplay.Row0Visibility = Visibility.Visible;
                    VersesDisplay.Row0ShowTranslation = ShowTranslations;
#pragma warning disable CS8601
                    VersesDisplay.Row0BorderColor = brush;
#pragma warning restore CS8601
                    break;
                } else if (VersesDisplay.Row1Verses.Count == 0 || 
                           VersesDisplay.Row1CorpusId == message.CorpusId)
                {
                    VersesDisplay.Row1CorpusId = message.CorpusId;
                    VersesDisplay.Row1Title = title;
                    VersesDisplay.Row1Verses = verses;
                    VersesDisplay.Row1Visibility = Visibility.Visible;
                    VersesDisplay.Row1ShowTranslation = ShowTranslations;
#pragma warning disable CS8601
                    VersesDisplay.Row1BorderColor = brush;
#pragma warning restore CS8601
                    break;
                }
                else if (VersesDisplay.Row2Verses.Count == 0 ||
                         VersesDisplay.Row2CorpusId == message.CorpusId )
                {
                    VersesDisplay.Row2CorpusId = message.CorpusId;
                    VersesDisplay.Row2Title = title;
                    VersesDisplay.Row2Verses = verses;
                    VersesDisplay.Row2Visibility = Visibility.Visible;
                    VersesDisplay.Row2ShowTranslation = ShowTranslations;
#pragma warning disable CS8601
                    VersesDisplay.Row2BorderColor = brush;
#pragma warning restore CS8601
                    break;
                }
                else
                {
                    VersesDisplay.Row3CorpusId = message.CorpusId;
                    VersesDisplay.Row3Title = title;
                    VersesDisplay.Row3Verses = verses;
                    VersesDisplay.Row3Visibility = Visibility.Visible;
                    VersesDisplay.Row3ShowTranslation = ShowTranslations;
#pragma warning disable CS8601
                    VersesDisplay.Row3BorderColor = brush;
#pragma warning restore CS8601
                    break;
                }
            }
            
            NotifyOfPropertyChange(() => VersesDisplay);
        }

        private IEnumerable<(EngineToken token, string paddingBefore, string paddingAfter)>? GetTokens(List<TokensTextRow> corpus, int BBBCCCVVV)
        {
            var textRow = corpus.FirstOrDefault(row => ((VerseRef)row.Ref).BBBCCCVVV == BBBCCCVVV);
            if (textRow != null)
            {
                var detokenizer = new EngineStringDetokenizer(new LatinWordDetokenizer());
                return detokenizer.Detokenize(textRow.Tokens);
            }

            return null;
        }


        private Translation GetTranslation(EngineToken token)
        {
            var translationText = (token.SurfaceText != "." && token.SurfaceText != ",")
                ? ""
                : String.Empty;
            var translation = new Translation(SourceToken: token, TargetTranslationText: translationText, TranslationOriginatedFrom: RandomTranslationOriginatedFrom());

            return translation;
        }

        private string RandomTranslationOriginatedFrom()
        {
            switch (new Random().Next(3))
            {
                case 0: return "FromTranslationModel";
                case 1: return "FromOther";
                default: return "Assigned";
            }
        }

        private void MoveCorpusDown0(object obj)
        {
            SwitchCorpusRowsDown(0);
        }
        private void MoveCorpusDown1(object obj)
        {
            SwitchCorpusRowsDown(1);
        }
        private void MoveCorpusDown2(object obj)
        {
            SwitchCorpusRowsDown(2);
        }

        private void MoveCorpusUp1(object obj)
        {
            SwitchCorpusRowsUp(1);
        }

        private void MoveCorpusUp2(object obj)
        {
            SwitchCorpusRowsUp(2);
        }

        private void MoveCorpusUp3(object obj)
        {
            SwitchCorpusRowsUp(3);
        }

        private void SwitchCorpusRowsUp(int i)
        {
            ObservableCollection<List<TokenDisplayViewModel>> temp;
            Brush tmpBrush;
            string tmpTitle;

            switch (i)
            {
                case 1:
                    temp = VersesDisplay.Row0Verses;
                    tmpBrush = VersesDisplay.Row0BorderColor;
                    tmpTitle = VersesDisplay.Row0Title;

                    VersesDisplay.Row0Verses = VersesDisplay.Row1Verses;
                    VersesDisplay.Row0BorderColor = VersesDisplay.Row1BorderColor;
                    VersesDisplay.Row0Title = VersesDisplay.Row1Title;

                    VersesDisplay.Row1Verses = temp;
                    VersesDisplay.Row1BorderColor = tmpBrush;
                    VersesDisplay.Row1Title = tmpTitle;
                    NotifyOfPropertyChange(() => VersesDisplay);
                    break;
                case 2:
                    temp = VersesDisplay.Row1Verses;
                    tmpBrush = VersesDisplay.Row1BorderColor;
                    tmpTitle = VersesDisplay.Row1Title;

                    VersesDisplay.Row1Verses = VersesDisplay.Row2Verses;
                    VersesDisplay.Row1BorderColor = VersesDisplay.Row2BorderColor;
                    VersesDisplay.Row1Title = VersesDisplay.Row2Title;

                    VersesDisplay.Row2Verses = temp;
                    VersesDisplay.Row2BorderColor = tmpBrush;
                    VersesDisplay.Row2Title = tmpTitle;
                    NotifyOfPropertyChange(() => VersesDisplay);
                    break;

                case 3:
                    temp = VersesDisplay.Row2Verses;
                    tmpBrush = VersesDisplay.Row2BorderColor;
                    tmpTitle = VersesDisplay.Row2Title;

                    VersesDisplay.Row2Verses = VersesDisplay.Row3Verses;
                    VersesDisplay.Row2BorderColor = VersesDisplay.Row3BorderColor;
                    VersesDisplay.Row2Title = VersesDisplay.Row3Title;

                    VersesDisplay.Row3Verses = temp;
                    VersesDisplay.Row3BorderColor = tmpBrush;
                    VersesDisplay.Row3Title = tmpTitle;
                    NotifyOfPropertyChange(() => VersesDisplay);
                    break;
            }
        }


        private void SwitchCorpusRowsDown(int i)
        {
            ObservableCollection<List<TokenDisplayViewModel>> temp;
            Brush tmpBrush;
            string tmpTitle;

            switch (i)
            {
                case 0:

                    if (VersesDisplay.Row1Verses.Count > 0)
                    {
                        temp = VersesDisplay.Row0Verses;
                        tmpBrush = VersesDisplay.Row0BorderColor;
                        tmpTitle = VersesDisplay.Row0Title;

                        VersesDisplay.Row0Verses = VersesDisplay.Row1Verses;
                        VersesDisplay.Row0BorderColor = VersesDisplay.Row1BorderColor;
                        VersesDisplay.Row0Title = VersesDisplay.Row1Title;

                        VersesDisplay.Row1Verses = temp;
                        VersesDisplay.Row1BorderColor = tmpBrush;
                        VersesDisplay.Row1Title = tmpTitle;
                        NotifyOfPropertyChange(() => VersesDisplay);
                    }
                    break;
                case 1:
                    if (VersesDisplay.Row2Verses.Count > 0)
                    {
                        temp = VersesDisplay.Row1Verses;
                        tmpBrush = VersesDisplay.Row1BorderColor;
                        tmpTitle = VersesDisplay.Row1Title;

                        VersesDisplay.Row1Verses = VersesDisplay.Row2Verses;
                        VersesDisplay.Row1BorderColor = VersesDisplay.Row2BorderColor;
                        VersesDisplay.Row1Title = VersesDisplay.Row2Title;

                        VersesDisplay.Row2Verses = temp;
                        VersesDisplay.Row2BorderColor = tmpBrush;
                        VersesDisplay.Row2Title = tmpTitle;
                        NotifyOfPropertyChange(() => VersesDisplay);
                    }
                    break;

                case 2:
                    if (VersesDisplay.Row3Verses.Count > 0)
                    {
                        temp = VersesDisplay.Row2Verses;
                        tmpBrush = VersesDisplay.Row2BorderColor;
                        tmpTitle = VersesDisplay.Row2Title;

                        VersesDisplay.Row2Verses = VersesDisplay.Row3Verses;
                        VersesDisplay.Row2BorderColor = VersesDisplay.Row3BorderColor;
                        VersesDisplay.Row2Title = VersesDisplay.Row3Title;

                        VersesDisplay.Row3Verses = temp;
                        VersesDisplay.Row3BorderColor = tmpBrush;
                        VersesDisplay.Row3Title = tmpTitle;
                        NotifyOfPropertyChange(() => VersesDisplay);
                    }
                    break;
            }
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
            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenDoubleClicked(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) double-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenRightButtonDown(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) right-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseEnter(TokenEventArgs e)
        {
            if (e.TokenDisplayViewModel.HasNote)
            {
                DisplayNote(e.TokenDisplayViewModel);
            }

            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) hovered";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseLeave(TokenEventArgs e)
        {
            Message = string.Empty;
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseWheel(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) mouse wheel";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationClicked(TranslationEventArgs e)
        {
            DisplayTranslation(e);

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
            //Message = $"'{e.Note.Text}' note for token ({e.EntityId}) clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteDoubleClicked(NoteEventArgs e)
        {
            //Message = $"'{e.Note.Text}' note for token ({e.EntityId}) double-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteRightButtonDown(NoteEventArgs e)
        {
            //Message = $"'{e.Note.Text}' note for token ({e.EntityId}) right-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteMouseEnter(NoteEventArgs e)
        {
            //Message = $"'{e.Note.Text}' note for token ({e.EntityId}) hovered";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteMouseLeave(NoteEventArgs e)
        {
            //Message = string.Empty;
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteMouseWheel(NoteEventArgs e)
        {
            //Message = $"'{e.Note.Text}' note for token ({e.EntityId}) mouse wheel";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteCreate(NoteEventArgs e)
        {
            //DisplayNote(e.TokenDisplayViewModel);
        }

        public void TranslationApplied(TranslationEventArgs e)
        {
            Message = $"Translation '{e.Translation.TargetTranslationText}' ({e.TranslationActionType}) applied to token '{e.TokenDisplayViewModel.SurfaceText}' ({e.TokenDisplayViewModel.Token.TokenId})";
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

        public void NoteAdded(NoteEventArgs e)
        {
            Message = $"Note '{e.Note.Text}' added to token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));

            NoteControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NoteControlVisibility));
        }

        public void NoteUpdated(NoteEventArgs e)
        {
            Message = $"Note '{e.Note.Text}' updated on token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));

            NoteControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NoteControlVisibility));
        }

        public void NoteDeleted(NoteEventArgs e)
        {
            Message = $"Note '{e.Note.Text}' deleted from token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));

            NoteControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NoteControlVisibility));
        }

        public void LabelSelected(LabelEventArgs e)
        {
            Message = $"Label '{e.Label.Text}' selected for token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void LabelAdded(LabelEventArgs e)
        {
            Message = $"Label '{e.Label.Text}' added for token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void CloseRequested(RoutedEventArgs args)
        {
            NoteControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NoteControlVisibility));
        }


        // ReSharper restore UnusedMember.Global

        #endregion

        public Visibility NoteControlVisibility { get; set; } = Visibility.Collapsed;
        private void DisplayNote(TokenDisplayViewModel tokenDisplayViewModel)
        {
            NoteControlVisibility = Visibility.Visible;
            CurrentTokenDisplayViewModel = tokenDisplayViewModel;

            NotifyOfPropertyChange(nameof(NoteControlVisibility));
            NotifyOfPropertyChange(nameof(CurrentTokenDisplayViewModel));
        }

        public Note CurrentNote { get; set; }
        public TokenDisplayViewModel CurrentTokenDisplayViewModel { get; set; }


        public IEnumerable<TranslationOption> TranslationOptions { get; set; }
        public TranslationOption CurrentTranslationOption { get; set; }
        private void DisplayTranslation(TranslationEventArgs e)
        {
            TranslationControlVisibility = Visibility.Visible;

            CurrentTokenDisplayViewModel = e.TokenDisplayViewModel;
            TranslationOptions = GetMockTranslationOptions(e.Translation.TargetTranslationText);
            CurrentTranslationOption = TranslationOptions.FirstOrDefault(to => to.Word == e.Translation.TargetTranslationText);

            NotifyOfPropertyChange(nameof(TranslationControlVisibility));
            NotifyOfPropertyChange(nameof(CurrentTokenDisplayViewModel));
            NotifyOfPropertyChange(nameof(TranslationOptions));
            NotifyOfPropertyChange(nameof(CurrentTranslationOption));
        }

        private IEnumerable<TranslationOption> GetMockTranslationOptions(string sourceTranslation)
        {
            var result = new List<TranslationOption>();

            var random = new Random();
            var optionCount = random.Next(4) + 2;     // 2-5 options
            var remainingPercentage = 100d;

            var basePercentage = random.NextDouble() * remainingPercentage;
            result.Add(new TranslationOption { Word = sourceTranslation, Probability = basePercentage });
            remainingPercentage -= basePercentage;

            for (var i = 1; i < optionCount - 1; i++)
            {
                var percentage = random.NextDouble() * remainingPercentage;
                result.Add(new TranslationOption { Word = GetMockOogaWord(), Probability = percentage });
                remainingPercentage -= percentage;
            }

            result.Add(new TranslationOption { Word = GetMockOogaWord(), Probability = remainingPercentage });

            return result.OrderByDescending(to => to.Probability);
        }

        private readonly List<string> MockOogaWords = new() { "Ooga", "booga", "bong", "biddle", "foo", "boi", "foodie", "fingle", "boing", "la" };


        private static int mockOogaWordsIndexer_;

        private string GetMockOogaWord()
        {
            var result = MockOogaWords[mockOogaWordsIndexer_++];
            if (mockOogaWordsIndexer_ == MockOogaWords.Count) mockOogaWordsIndexer_ = 0;
            return result;
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
