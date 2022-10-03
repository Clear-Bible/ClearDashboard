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
using SIL.Extensions;

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

        public ICommand MoveCorpusDownRowCommand { get; set; }
        public ICommand MoveCorpusUpRowCommand { get; set; }
        public ICommand DeleteCorpusRowCommand { get; set; }


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
        public List<ShowParallelTranslationWindowMessage> _parallelMessages = new();


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

        private ObservableCollection<VersesDisplay> _versesDisplay = new();
        public ObservableCollection<VersesDisplay> VersesDisplay
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
            MockData();
        }

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


            MoveCorpusDownRowCommand = new RelayCommand(MoveCorpusDown);
            MoveCorpusUpRowCommand = new RelayCommand(MoveCorpusUp);
            DeleteCorpusRowCommand = new RelayCommand(DeleteCorpusRow);


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


        private void MockData()
        {
            VersesDisplay = new();
            VersesDisplay.Add(new VersesDisplay
            {
                BorderColor = Brushes.Teal,
                CorpusId = Guid.NewGuid(),
                DisplayName = "Mock Corpus",
                RowTitle = "Mock Corpus",
                ShowTranslation = true,
                Verses = new ObservableCollection<List<TokenDisplayViewModel>>(),
            });
            VersesDisplay.Add(new VersesDisplay
            {
                BorderColor = Brushes.Brown,
                CorpusId = Guid.NewGuid(),
                DisplayName = "Mock Corpus2",
                RowTitle = "Mock Corpus",
                ShowTranslation = true,
                Verses = new ObservableCollection<List<TokenDisplayViewModel>>(),
            });
        }


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
            var msg = _parallelMessages.Where(p =>
                p.TranslationSetId == message.TranslationSetId && p.ParallelCorpusId == message.ParallelCorpusId).ToList();
            if (msg.Count == 0)
            {
                _parallelMessages.Add(message);
            }

            // current project
            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    var VerseTokens = await BuildTokenDisplayViewModels(message);
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Found {VerseTokens.Count} TokensTextRow entities.",
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

        private async Task<List<TokenDisplayViewModel>> BuildTokenDisplayViewModels(ShowParallelTranslationWindowMessage message)
        {
            List<TokenDisplayViewModel> VerseTokens = new();
            var verseOut = new ObservableCollection<List<TokenDisplayViewModel>>();

            var row = await VerseTextRow(Convert.ToInt32(CurrentBcv.BBBCCCVVV), message);

            if (row is null)
            {

                OnUIThread(() =>
                {
                    UpdateParallelCorpusDisplay(message, verseOut, message.ParallelCorpusDisplayName + "    No verse data in this verse range", true);
                    NotifyOfPropertyChange(() => VersesDisplay);

                    ProgressBarVisibility = Visibility.Collapsed;
                });
            }
            else
            {
                NotesDictionary = await Note.GetAllDomainEntityIdNotes(Mediator);
                CurrentTranslationSet = await GetTranslationSet(message);
                CurrentTranslations = await CurrentTranslationSet.GetTranslations(row.SourceTokens.Select(t => t.TokenId));
                VerseTokens = GetTokenDisplayViewModels(row.SourceTokens);
                LabelSuggestions = await GetLabelSuggestions();
                verseOut.Add(VerseTokens);

                OnUIThread(() =>
                {
                    UpdateParallelCorpusDisplay(message, verseOut, message.ParallelCorpusDisplayName, true);
                    NotifyOfPropertyChange(() => VersesDisplay);

                    ProgressBarVisibility = Visibility.Collapsed;
                });
            }

            return VerseTokens;
        }

        public async Task<EngineParallelTextRow?> VerseTextRow(int BBBCCCVVV, ShowParallelTranslationWindowMessage message)
        {
            try
            {
                var corpusIds = await ParallelCorpus.GetAllParallelCorpusIds(Mediator);
                var guid = Guid.Parse(message.ParallelCorpusId);
                var corpus = await ParallelCorpus.Get(Mediator, corpusIds.First(p => p.Id == guid));
                var verse = corpus.GetByVerseRange(new VerseRef(BBBCCCVVV), (ushort)VerseOffsetRange, (ushort)VerseOffsetRange);

                // save out the corpus for future use
                // _parallelProjects



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

        public async Task<DAL.Alignment.Translation.TranslationSet?> GetTranslationSet(ShowParallelTranslationWindowMessage message)
        {
            DAL.Alignment.Translation.TranslationSet translationSet;
            try
            {
                if (message.TranslationSetId == "")
                {
                    var translationSetIds = await DAL.Alignment.Translation.TranslationSet.GetAllTranslationSetIds(Mediator);
                    translationSet = await DAL.Alignment.Translation.TranslationSet.Get(translationSetIds.First().translationSetId, Mediator);
                }
                else
                {
                    translationSet = await DAL.Alignment.Translation.TranslationSet.Get(new TranslationSetId(Guid.Parse(message.TranslationSetId)), Mediator);
                }

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
                    let translation = GetParallelTranslation(paddedToken.token)
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

                    if (message.ParatextProjectId == _projectManager?.ManuscriptHebrewGuid.ToString())
                    {
                        // our fake Manuscript corpus
                        var bookInfo = new BookInfo();
                        var books = bookInfo.GenerateScriptureBookList();

                        metadata = new ParatextProjectMetadata
                        {
                            Id = _projectManager.ManuscriptHebrewGuid.ToString(),
                            CorpusType = CorpusType.ManuscriptHebrew,
                            Name = "Macula Hebrew",
                            AvailableBooks = books,
                        };
                    }
                    else if (message.ParatextProjectId == _projectManager?.ManuscriptGreekGuid.ToString())
                    {
                        // our fake Manuscript corpus
                        var bookInfo = new BookInfo();
                        var books = bookInfo.GenerateScriptureBookList();

                        metadata = new ParatextProjectMetadata
                        {
                            Id = _projectManager.ManuscriptGreekGuid.ToString(),
                            CorpusType = CorpusType.ManuscriptGreek,
                            Name = "Macula Greek",
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

                    OnUIThread(() =>
                    {
                        UpdateVersesDisplay(message, new ObservableCollection<List<TokenDisplayViewModel>>(),
                            message.ProjectName + " - " + message.TokenizationType +
                            "    No verse data in this verse range", false);
                        ProgressBarVisibility = Visibility.Collapsed;
                    });
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

                    OnUIThread(() =>
                    {
                        UpdateVersesDisplay(message, new ObservableCollection<List<TokenDisplayViewModel>>(),
                            message.ProjectName + " - " + message.TokenizationType +
                            "    No verse data in this verse range", false);
                        ProgressBarVisibility = Visibility.Collapsed;
                    });

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

            for (int i = 0; i < _parallelMessages.Count; i++)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                await ShowNewParallelTranslation(_parallelMessages[i], _cancellationTokenSource.Token, _cancellationTokenSource.Token);
            }

            
        }

        private void UpdateVersesDisplay(ShowTokenizationWindowMessage message, ObservableCollection<List<TokenDisplayViewModel>> verses, string title, bool ShowTranslations)
        {
            var brush = GetCorpusBrushColor(message);

            var row = VersesDisplay.FirstOrDefault(v => v.CorpusId == message.CorpusId);
            if (row is null)
            {
                VersesDisplay.Add(new Models.VersesDisplay
                {
                    CorpusId = message.CorpusId,
                    BorderColor = brush,
                    ShowTranslation = ShowTranslations,
                    RowTitle = title,
                    Verses = verses,
                });
            }
            else
            {
                row.CorpusId = message.CorpusId;
                row.BorderColor = brush;
                row.ShowTranslation = ShowTranslations;
                row.RowTitle = title;
                row.Verses = verses;
            }

            NotifyOfPropertyChange(() => VersesDisplay);
        }

        private static Brush? GetCorpusBrushColor(ShowTokenizationWindowMessage message)
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
                case CorpusType.ManuscriptHebrew:
                    brush = Brushes.MediumOrchid;
                    break;
                case CorpusType.ManuscriptGreek:
                    brush = Brushes.MediumOrchid;
                    break;
                default:
                    brush = Brushes.Blue;
                    break;
            }

            return brush;
        }

        private void UpdateParallelCorpusDisplay(ShowParallelTranslationWindowMessage message, ObservableCollection<List<TokenDisplayViewModel>> verses, string title, bool ShowTranslations = true)
        {
            // same color as defined in SharedVisualTemplates.xaml
            Brush brush = Brushes.SaddleBrown;

            var row = VersesDisplay.FirstOrDefault(v => v.CorpusId == Guid.Parse(message.ParallelCorpusId));
            if (row is null)
            {
                VersesDisplay.Add(new Models.VersesDisplay
                {
                    CorpusId = Guid.Parse(message.ParallelCorpusId),
                    BorderColor = brush,
                    ShowTranslation = ShowTranslations,
                    RowTitle = title + $"    ({CurrentBcv.BookName} {CurrentBcv.BookNum}:{CurrentBcv.VerseNum})",
                    Verses = verses,
                });
            }
            else
            {
                row.CorpusId = Guid.Parse(message.ParallelCorpusId);
                row.BorderColor = brush;
                row.ShowTranslation = ShowTranslations;
                row.RowTitle = title + $"    ({CurrentBcv.BookName} {CurrentBcv.BookNum}:{CurrentBcv.VerseNum})";
                row.Verses = verses;
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

        private Translation GetParallelTranslation(ClearBible.Engine.Corpora.Token token)
        {
            var translation = CurrentTranslations.FirstOrDefault(t => t.SourceToken.TokenId.Id == token.TokenId.Id);
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

        public void MoveCorpusDown(object obj)
        {
            var row = obj as VersesDisplay;

            var index = VersesDisplay.Select((element, index) => new { element, index })
                .FirstOrDefault(x => x.element.Equals(row))?.index ?? -1;

            if (index == VersesDisplay.Count - 1)
            {
                return;
            }
            
            VersesDisplay.Move(index, index + 1);
        }
        public void MoveCorpusUp(object obj)
        {
            var row = obj as VersesDisplay;

            var index = VersesDisplay.Select((element, index) => new { element, index })
                .FirstOrDefault(x => x.element.Equals(row))?.index ?? -1;

            if (index < 1)
            {
                return;
            }

            VersesDisplay.Move(index, index - 1);
        }

        public void DeleteCorpusRow(object obj)
        {
            var row = obj as VersesDisplay;

            // remove from the display
            var index = VersesDisplay.Select((element, index) => new { element, index })
                .FirstOrDefault(x => x.element.Equals(row))?.index ?? -1;

            VersesDisplay.RemoveAt(index);


            // remove stored collection
            var tokenProject  = _tokenProjects.FirstOrDefault(x => x.CorpusId==row.CorpusId);
            if (tokenProject is not null)
            {
                _tokenProjects.Remove(tokenProject);
            }
            
            // remove stored message
            var tokenMessage = _projectMessages.FirstOrDefault(x => x.CorpusId == row.CorpusId);
            if (tokenMessage is not null)
            {
                _projectMessages.Remove(tokenMessage);
            }
            
        }


        public Task HandleAsync(BCVLoadedMessage message, CancellationToken cancellationToken)
        {
            this.BcvDictionary = _projectManager.CurrentParatextProject.BcvDictionary;

            return Task.CompletedTask;
        }


        #endregion // Methods

        #region Event Handlers

        public void TokenMouseEnter(object sender, TokenEventArgs e)
        {
            if (e.TokenDisplayViewModel.HasNote)
            {
                DisplayNote(e.TokenDisplayViewModel);
            }
        }

        public void TranslationClicked(object sender, TranslationEventArgs e)
        {
            DisplayTranslation(e);
        }

        public void NoteMouseEnter(object sender, NoteEventArgs e)
        {
            DisplayNote(e.TokenDisplayViewModel);
        }

        public void NoteCreate(object sender, NoteEventArgs e)
        {
            DisplayNote(e.TokenDisplayViewModel);
        }

        public void TranslationApplied(object sender, TranslationEventArgs e)
        {
            Task.Run(() => TranslationAppliedAsync(e).GetAwaiter()); 

        }

        public async Task TranslationAppliedAsync(TranslationEventArgs e)
        {
            try
            {
                ProgressBarVisibility = Visibility.Visible;
                await CurrentTranslationSet.PutTranslation(e.Translation, e.TranslationActionType);
                await VerseChangeRerender();
                HideTranslation();
            }
            finally
            {
                ProgressBarVisibility = Visibility.Collapsed;
            }

        }

        public void TranslationCancelled(object sender, RoutedEventArgs e)
        {
            HideTranslation();
        }

        public void NoteAdded(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteAddedAsync(e).GetAwaiter());
        }

        public async Task NoteAddedAsync(NoteEventArgs e)
        {
            HideNote();

            await e.Note.CreateOrUpdate(Mediator);
            await e.Note.AssociateDomainEntity(Mediator, e.EntityId);
            foreach (var label in e.Note.Labels)
            {
                if (label.LabelId == null)
                {
                    await label.CreateOrUpdate(Mediator);
                }
                await e.Note.AssociateLabel(Mediator, label);
            }

            
        }

        public async Task NoteUpdated(object sender, NoteEventArgs e)
        {
            HideNote();

            await e.Note.CreateOrUpdate(Mediator);
        }

        public async Task NoteDeleted(object sender, NoteEventArgs e)
        {
            HideNote();

            await e.Note.Delete(Mediator);
        }

        public async Task LabelSelected(object sender, LabelEventArgs e)
        {
            // If this is a new note, we'll handle the labels when the note is added.
            if (e.Note.NoteId != null)
            {
                await e.Note.AssociateLabel(Mediator, e.Label);
            }
        }

        public async Task LabelAdded(object sender, LabelEventArgs e)
        {
            // If this is a new note, we'll handle the labels when the note is added.
            if (e.Note.NoteId != null)
            {
                e.Label = await e.Note.CreateAssociateLabel(Mediator, e.Label.Text);
            }
        }

        public void CloseNotePaneRequested(object sender, NoteEventArgs args)
        {
            HideNote();
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
        private void HideNote()
        {
            NoteControlVisibility = Visibility.Collapsed;
            NotifyOfPropertyChange(nameof(NoteControlVisibility));
        }

        public Note CurrentNote { get; set; }
        public TokenDisplayViewModel CurrentTokenDisplayViewModel { get; set; }


        public IEnumerable<TranslationOption> TranslationOptions { get; set; }
        public TranslationOption CurrentTranslationOption { get; set; }
        private async void DisplayTranslation(TranslationEventArgs e)
        {
            TranslationControlVisibility = Visibility.Visible;

            CurrentTokenDisplayViewModel = e.TokenDisplayViewModel;
            TranslationOptions = await GetTranslationOptions(e.Translation);
            CurrentTranslationOption = TranslationOptions.FirstOrDefault(to => to.Word == e.Translation.TargetTranslationText);

            NotifyOfPropertyChange(nameof(TranslationControlVisibility));
            NotifyOfPropertyChange(nameof(CurrentTokenDisplayViewModel));
            NotifyOfPropertyChange(nameof(TranslationOptions));
            NotifyOfPropertyChange(nameof(CurrentTranslationOption));
        }

        private async Task<IEnumerable<TranslationOption>> GetTranslationOptions(Translation translation)
        {
            var translationModelEntry = await CurrentTranslationSet.GetTranslationModelEntryForToken(translation.SourceToken);
            var translationOptions = translationModelEntry.OrderByDescending(option => option.Value)
                .Select(option => new TranslationOption { Word = option.Key, Probability = option.Value })
                .Take(4)
                .ToList();
            return translationOptions;
        }

        private void HideTranslation()
        {
            TranslationControlVisibility = Visibility.Collapsed;
            NotifyOfPropertyChange(nameof(TranslationControlVisibility));
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
