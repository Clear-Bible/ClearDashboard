using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using EngineToken = ClearBible.Engine.Corpora.Token;
using Label = ClearDashboard.DAL.Alignment.Notes.Label;
using Note = ClearDashboard.DAL.Alignment.Notes.Note;
using ParallelCorpus = ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;

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
        private readonly DashboardProjectManager? _projectManager;
        private readonly IServiceProvider _serviceProvider;

        private CancellationTokenSource? _cancellationTokenSource;
        private bool? _handleAsyncRunning;
        private string? _tokenizationType;
        private TokenizedTextCorpus? _currentTokenizedTextCorpus;
        private Visibility? _progressBarVisibility = Visibility.Visible;
        private string? _message;
        private BookInfo? _currentBook;

        private bool InComingChangesStarted { get; set; }

        private string CurrentBookDisplay => string.IsNullOrEmpty(CurrentBook?.Code) ? string.Empty : $"<{CurrentBook.Code}>";

        // used for storing the displayed corpus order
        public List<DisplayOrder> DisplayOrder = new();

        private List<TokenProject> _tokenProjects = new();
        private List<ShowTokenizationWindowMessage> _projectMessages = new();
        
        private List<ParallelProject> _parallelProjects = new();
        private List<ShowParallelTranslationWindowMessage> _parallelMessages = new();



        #endregion //Member Variables

        #region Public Properties

        public bool IsRtl { get; set; }

        #region BCV
        private bool _paratextSync = true;
        public bool ParatextSync
        {
            get => _paratextSync;
            set
            {
                if (value)
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

        private int _verseOffsetRange;
        public int VerseOffsetRange
        {
            get => _verseOffsetRange;
            set
            {
                if (value != _verseOffsetRange)
                {
                    _verseOffsetRange = value;
#pragma warning disable CS4014
                    VerseChangeRerender();
#pragma warning restore CS4014
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
                NotifyOfPropertyChange(() => CurrentBookDisplay);
            }
        }

        private string _verseChange = string.Empty;
        public string VerseChange
        {
            get => _verseChange;
            set
            {
                if (_verseChange == "000000000")
                {
                    return;
                }
                
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

#pragma warning disable CS4014
                    VerseChangeRerender();
#pragma warning restore CS4014
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
            set => Set(ref _progressBarVisibility, value);
                //Set(ref _progressBarVisibility, value);
        }

        private ObservableCollection<VersesDisplay> _versesDisplay = new();
        public ObservableCollection<VersesDisplay> VersesDisplay
        {
            get => _versesDisplay;
            set => Set(ref _versesDisplay, value);
        }

        public string? Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        private ObservableCollection<TokensTextRow>? _verses;
        public ObservableCollection<TokensTextRow>? Verses
        {
            get => _verses;
            set => Set(ref _verses, value);
        }

        public Dictionary<IId, IEnumerable<Note>>? NotesDictionary { get; set; }
        public DAL.Alignment.Translation.TranslationSet CurrentTranslationSet { get; set; }
        public EngineStringDetokenizer Detokenizer { get; set; } = new EngineStringDetokenizer(new LatinWordDetokenizer());
        public IEnumerable<Translation> CurrentTranslations { get; set; }
        public IEnumerable<Label> LabelSuggestions { get; set; }

        #endregion //Observable Properties


        #region Constructor

        // ReSharper disable once UnusedMember.Global
#pragma warning disable CS8618
        public EnhancedViewModel()
#pragma warning restore CS8618
        {
            // required by design-time binding
            MockData();
        }

        // ReSharper disable once UnusedMember.Global
#pragma warning disable CS8618
        public EnhancedViewModel(INavigationService navigationService, ILogger<EnhancedViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator, IMediator mediator,
            ILifetimeScope? lifetimeScope, IServiceProvider serviceProvider) :
            base(navigationService: navigationService, logger: logger, projectManager: projectManager,
                eventAggregator: eventAggregator, mediator: mediator, lifetimeScope: lifetimeScope)
#pragma warning restore CS8618
        {

            _logger = logger;
            _projectManager = projectManager;
            _serviceProvider = serviceProvider;

            Title = "⳼ " + LocalizationStrings.Get("Windows_EnhancedView", Logger);
            ContentId = "ENHANCEDVIEW";

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
            if (_projectManager.CurrentParatextProject.BcvDictionary != null)
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


        //public void LaunchMirrorView(double actualWidth, double actualHeight)
        //{
        //    LaunchMirrorView<TextCollectionsView>.Show(this, actualWidth, actualHeight);
        //}


        #region Corpus

        public async Task ShowCorpusTokens(ShowTokenizationWindowMessage message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received TokenizedTextCorpusMessage.");
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

        public async Task<ParallelCorpus> GetParallelCorpus(ParallelCorpusId? corpusId = null)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                if (corpusId == null)
                {
                    var corpusIds = await ParallelCorpus.GetAllParallelCorpusIds(Mediator!);
                    corpusId = corpusIds.First();
                }
                var corpus = await ParallelCorpus.Get(Mediator!, corpusId);

                Detokenizer = corpus.Detokenizer;
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved parallel corpus {corpus.ParallelCorpusId.Id} in {stopwatch.ElapsedMilliseconds} ms");
                return corpus;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }


        private async Task<List<TokenDisplayViewModel>> BuildTokenDisplayViewModels(ShowParallelTranslationWindowMessage message)
        {
            var VerseDisplayViewModel = _serviceProvider!.GetService<VerseDisplayViewModel>();

            List<TokenDisplayViewModel> verseTokens = new();
            var versesOut = new ObservableCollection<VerseDisplayViewModel>();

            List<string> verseRange = GetValidVerseRange(CurrentBcv.BBBCCCVVV, VerseOffsetRange);

            var rows = await VerseTextRow(Convert.ToInt32(CurrentBcv.BBBCCCVVV), message);

            if (rows is null)
            {

                OnUIThread(() =>
                {
                    UpdateParallelCorpusDisplay(message, versesOut, message.ParallelCorpusDisplayName + "    No verse data in this verse range", true);
                    NotifyOfPropertyChange(() => VersesDisplay);

                    ProgressBarVisibility = Visibility.Collapsed;
                });
            }
            else
            {
                NotesDictionary = await Note.GetAllDomainEntityIdNotes(Mediator);
                CurrentTranslationSet = await GetTranslationSet(message);
                foreach (var row in rows)
                {
                    await VerseDisplayViewModel!.BindAsync(row, CurrentTranslationSet, Detokenizer);
                    versesOut.Add(VerseDisplayViewModel);
                }

                BookChapterVerseViewModel bcv = new BookChapterVerseViewModel();
                string title = message.ParallelCorpusDisplayName ?? string.Empty;
                if (rows.Count <= 1)
                {
                    // only one verse
                    bcv.SetVerseFromId(verseRange[0]);
                    title += $"  ({bcv.BookName} {bcv.ChapterNum}:{bcv.VerseNum})";
                }
                else
                {
                    // multiple verses
                    bcv.SetVerseFromId(verseRange[0]);
                    title += $"  ({bcv.BookName} {bcv.ChapterNum}:{bcv.VerseNum}-";
                    bcv.SetVerseFromId(verseRange[^1]);
                    title += $"{bcv.VerseNum})";
                }

                OnUIThread(() =>
                {
                    UpdateParallelCorpusDisplay(message, versesOut, title);
                    NotifyOfPropertyChange(() => VersesDisplay);

                    ProgressBarVisibility = Visibility.Collapsed;
                });
            }

            return verseTokens;
        }



       
        private List<string> GetValidVerseRange(string bbbcccvvv, int offset)
        {
            List<string> verseRange = new();
            verseRange.Add(bbbcccvvv);

            int currentVerse = Convert.ToInt32(bbbcccvvv.Substring(6));
            
            // get lower range first
            int j = 1;
            while (j <= offset)
            {
                // check verse
                if (BcvDictionary.ContainsKey(bbbcccvvv.Substring(0, 6) + (currentVerse - j).ToString("000")))
                {
                    verseRange.Add(bbbcccvvv.Substring(0, 6) + (currentVerse - j).ToString("000"));
                }
                
                j++;
            }


            // get upper range
            j = 1;
            while (j <= offset)
            {
                // check verse
                if (BcvDictionary.ContainsKey(bbbcccvvv.Substring(0, 6) + (currentVerse + j).ToString("000")))
                {
                    verseRange.Add(bbbcccvvv.Substring(0, 6) + (currentVerse + j).ToString("000"));
                }

                j++;
            }

            // sort list
            verseRange.Sort();

            return verseRange;
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

        public async Task ShowNewCorpusTokens(ShowTokenizationWindowMessage message, CancellationToken cancellationToken,
            CancellationToken localCancellationToken)
        {
            // current project
            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    ParatextProjectMetadata metadata;

                    if (message.ParatextProjectId == ManuscriptIds.HebrewManuscriptId)
                    {
                        // our fake Manuscript corpus
                        var bookInfo = new BookInfo();
                        var books = bookInfo.GenerateScriptureBookList();

                        metadata = new ParatextProjectMetadata
                        {
                            Id = ManuscriptIds.HebrewManuscriptId,
                            CorpusType = CorpusType.ManuscriptHebrew,
                            Name = "Macula Hebrew",
                            AvailableBooks = books,
                        };
                    }
                    else if (message.ParatextProjectId == ManuscriptIds.GreekManuscriptId)
                    {
                        // our fake Manuscript corpus
                        var bookInfo = new BookInfo();
                        var books = bookInfo.GenerateScriptureBookList();

                        metadata = new ParatextProjectMetadata
                        {
                            Id = ManuscriptIds.GreekManuscriptId,
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

                            //var VerseDisplayViewModel = _serviceProvider.GetService<VerseDisplayViewModel>();

                            //await VerseDisplayViewModel!.BindAsync(verseRangeRow, null, new EngineStringDetokenizer(Detokenizer), message.IsRTL);

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
                    Logger?.LogError(ex, "An unexpected error occurred while displaying corpus tokens.");
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
                    _cancellationTokenSource?.Dispose();
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
                        List<TokensTextRow> corpus = new() { verseRangeRow };

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

        #endregion

        #region Parallel

        public async Task ShowParallelTranslationTokens(ShowParallelTranslationWindowMessage message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received TokenizedTextCorpusMessage.");
            _handleAsyncRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var localCancellationToken = _cancellationTokenSource.Token;

            ProgressBarVisibility = Visibility.Visible;

            TokenProject? project = null;

            await ShowParallelTranslation(message, cancellationToken, localCancellationToken);
        }

        public async Task ShowParallelTranslation(ShowParallelTranslationWindowMessage message,
            CancellationToken cancellationToken, CancellationToken localCancellationToken)
        {
            var msg = _parallelMessages.Where(p =>
                p.TranslationSetId == message.TranslationSetId && p.ParallelCorpusId == message.ParallelCorpusId).ToList();
            if (msg.Count == 0)
            {
                _parallelMessages.Add(message);
            }

            // current project
            _ = await Task.Factory.StartNew(async () =>
            {
                try
                {
                    var verseTokens = await BuildTokenDisplayViewModels(message);
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Found {verseTokens.Count} TokensTextRow entities.",
                            StartTime = DateTime.Now,
                            TaskLongRunningProcessStatus = LongRunningProcessStatus.Completed
                        }), cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "The attempt to ShowNewParallelTranslation failed.");
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
                    if (_cancellationTokenSource != null) 
                        _cancellationTokenSource.Dispose();
                }
            }, cancellationToken);
        }

        #endregion

        private async Task VerseChangeRerender()
        {
            for (var i = 0; i < _tokenProjects.Count; i++)
            {
                ProgressBarVisibility = Visibility.Visible;
                _cancellationTokenSource = new CancellationTokenSource();

                await ShowExistingCorpusTokens(_projectMessages[i], _cancellationTokenSource.Token, _tokenProjects[i],
                    _cancellationTokenSource.Token).ConfigureAwait(false);
            }

            for (var i = 0; i < _parallelMessages.Count; i++)
            {
                ProgressBarVisibility = Visibility.Visible;
                _cancellationTokenSource = new CancellationTokenSource();
                
                await ShowParallelTranslation(_parallelMessages[i], _cancellationTokenSource.Token,
                    _cancellationTokenSource.Token);
            }

            
        }



        private async Task<List<EngineParallelTextRow?>> VerseTextRow(int bbbcccvvv, ShowParallelTranslationWindowMessage message)
        {
            try
            {
                var corpusIds = await ParallelCorpus.GetAllParallelCorpusIds(Mediator);
                var guid = Guid.Parse(message.ParallelCorpusId);


                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                // check if this project exist or not
                var project = _parallelProjects.FirstOrDefault(x => x.ParallelCorpusId == guid);
                ParallelCorpus corpus;
                if (project is null)
                {
                    // save this to the list
                    corpus = await ParallelCorpus.Get(Mediator, corpusIds.First(p => p.Id == guid));
                    _parallelProjects.Add(new ParallelProject
                    {
                        ParallelCorpusId = guid,
                        parallelTextRows= corpus,
                    });
                    stopwatch.Stop();
                    Logger?.LogInformation($"Retrieved parallel corpus first time {corpus.ParallelCorpusId.Id} in {stopwatch.ElapsedMilliseconds} ms");

                }
                else
                {
                    corpus = project.parallelTextRows;
                    stopwatch.Stop();
                    Logger?.LogInformation($"Retrieved parallel corpus subsequent times {corpus.ParallelCorpusId.Id} in {stopwatch.ElapsedMilliseconds} ms");
                }

                var verses = corpus.GetByVerseRange(new VerseRef(bbbcccvvv), (ushort)VerseOffsetRange, (ushort)VerseOffsetRange);
                
                List<EngineParallelTextRow?> rows = new();
                foreach (var verse in verses.parallelTextRows)
                {
                    rows.Add(verse as EngineParallelTextRow);
                }

                return rows;  // return verses.parallelTextRows.FirstOrDefault() as EngineParallelTextRow;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        private void UpdateVersesDisplay(ShowTokenizationWindowMessage message, ObservableCollection<List<TokenDisplayViewModel>> verses, string title, bool showTranslations)
        {
            var brush = GetCorpusBrushColor(message);

            var row = VersesDisplay.FirstOrDefault(v => v.CorpusId == message.CorpusId);
            if (row is null)
            {
                VersesDisplay.Add(new VersesDisplay
                {
                    CorpusId = message.CorpusId,
                    BorderColor = brush,
                    ShowTranslation = showTranslations,
                    RowTitle = title,
                    Verses = verses,
                    IsRtl = message.IsRTL,
                });

                // add to the grouping for saving
                DisplayOrder.Add(new Models.DisplayOrder
                {
                    MsgType = Models.DisplayOrder.MessageType.ShowTokenizationWindowMessage,
                    Data = message
                });
            }
            else
            {
                row.CorpusId = message.CorpusId;
                row.BorderColor = brush;
                row.ShowTranslation = showTranslations;
                row.RowTitle = title;
                row.Verses = verses;
                row.IsRtl = message.IsRTL;
            }

            NotifyOfPropertyChange(() => VersesDisplay);
        }

        private void UpdateParallelCorpusDisplay(ShowParallelTranslationWindowMessage message,
            ObservableCollection<VerseDisplayViewModel> verses, string title, bool showTranslations = true)
        {
            // same color as defined in SharedVisualTemplates.xaml
            Brush brush = Brushes.SaddleBrown;

            var row = VersesDisplay.FirstOrDefault(v => v.CorpusId == Guid.Parse(message.ParallelCorpusId));
            if (row is null)
            {
                VersesDisplay.Add(new VersesDisplay
                {
                    CorpusId = Guid.Parse(message.ParallelCorpusId),
                    BorderColor = brush,
                    ShowTranslation = showTranslations,
                    RowTitle = title,
                    ParallelVerses = verses,
                });

                // add to the grouping for saving
                DisplayOrder.Add(new Models.DisplayOrder
                {
                    MsgType = Models.DisplayOrder.MessageType.ShowParallelTranslationWindowMessage,
                    Data = message
                });
            }
            else
            {
                row.CorpusId = Guid.Parse(message.ParallelCorpusId);
                row.BorderColor = brush;
                row.ShowTranslation = showTranslations;
                row.RowTitle = title;
                row.ParallelVerses = verses;
            }

            NotifyOfPropertyChange(() => VersesDisplay);
        }

        private static Brush? GetCorpusBrushColor(ShowTokenizationWindowMessage message)
        {
            // same color as defined in SharedVisualTemplates.xaml
            Brush brush;
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

        private IEnumerable<(EngineToken token, string paddingBefore, string paddingAfter)>? GetTokens(List<TokensTextRow> corpus, int bbbcccvvv)
        {
            var textRow = corpus.FirstOrDefault(row => ((VerseRef)row.Ref).BBBCCCVVV == bbbcccvvv);
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

        public async Task AddNoteToDatabase(Note note, IId entityId)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await note.CreateOrUpdate(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Added note {note.NoteId.Id} in {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Restart();
#endif
                await note.AssociateDomainEntity(Mediator, entityId);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Associated note {note.NoteId.Id} with entity {entityId.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                if (note.Labels.Any())
                {
#if DEBUG
                    stopwatch.Restart();
#endif
                    foreach (var label in note.Labels)
                    {
                        if (label.LabelId == null)
                        {
                            await label.CreateOrUpdate(Mediator);
                        }
                        await note.AssociateLabel(Mediator, label);
                    }
#if DEBUG
                    stopwatch.Stop();
                    Logger?.LogInformation($"Associated labels with note {note.NoteId.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                }
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
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

        private void MoveCorpusDown(object obj)
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

        private void MoveCorpusUp(object obj)
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

        private void DeleteCorpusRow(object obj)
        {
            var row = obj as VersesDisplay;

            // remove from the display
            var index = VersesDisplay.Select((element, index) => new { element, index })
                .FirstOrDefault(x => x.element.Equals(row))?.index ?? -1;

            VersesDisplay.RemoveAt(index);
            // remove from the grouping for saving
            DisplayOrder.RemoveAt(index);

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

        #region IHandle

        public async Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            if (CurrentBcv.BibleBookList.Count == 0)
            {
                return;
            }

            if (message.Verse != "" && CurrentBcv.BBBCCCVVV != message.Verse.PadLeft(9, '0'))
            {
                // send to log
                await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{DisplayName}: Project Change"), cancellationToken);

                InComingChangesStarted = true;
                CurrentBcv.SetVerseFromId(message.Verse);
                InComingChangesStarted = false;
            }

        }

        public async Task HandleAsync(ProjectChangedMessage message, CancellationToken cancellationToken)
        {
            if (ProjectManager?.CurrentParatextProject is not null)
            {
                // send to log
                await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{DisplayName}: Project Change"), cancellationToken);

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
            _logger.LogInformation("Received TokenizedTextCorpusMessage.");
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

        public Task HandleAsync(BCVLoadedMessage message, CancellationToken cancellationToken)
        {
            BcvDictionary = _projectManager.CurrentParatextProject.BcvDictionary;

            return Task.CompletedTask;
        }

        #endregion

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
                TranslationControlVisibility = Visibility.Collapsed;
                //HideTranslation();
            }
            finally
            {
                ProgressBarVisibility = Visibility.Collapsed;
            }
        }

        public void TranslationCancelled(object sender, RoutedEventArgs e)
        {
            TranslationControlVisibility = Visibility.Collapsed;
            //HideTranslation();
        }

        public void NoteAdded(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteAddedAsync(e).GetAwaiter());
        }

        public async Task NoteAddedAsync(NoteEventArgs e)
        {
            //HideNote();

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

            await AddNoteToDatabase(e.Note, e.EntityId);

            // TODO: notify the token that a note was added
            //var token = VerseTokens.FirstOrDefault(vt => vt.Token.TokenId == e.EntityId);
            //if (token != null)
            //{
            //    token.NoteAdded(e.Note);
            //}

            //Message = $"Note '{e.Note.Text}' added to token ({e.EntityId})";
            //NotifyOfPropertyChange(nameof(Message));

            //e.TokenDisplayViewModel.Notes.Add(e.Note);
        }

        public async Task NoteUpdated(object sender, NoteEventArgs e)
        {
            //HideNote();

            await e.Note.CreateOrUpdate(Mediator);
        }

        public async Task NoteDeleted(object sender, NoteEventArgs e)
        {
            //HideNote();

            await e.Note.Delete(Mediator);
        }
        

        public void LabelSelected(object sender, LabelEventArgs e)
        {
            Task.Run(() => LabelSelectedAsync(e).GetAwaiter());
        }

        public async Task LabelSelectedAsync(LabelEventArgs e)
        {
            // If this is a new note, we'll handle the labels when the note is added.
            if (e.Note.NoteId != null)
            {
                await e.Note.AssociateLabel(Mediator, e.Label);
            }
        }
        
        public void LabelAdded(object sender, LabelEventArgs e)
        {
            Task.Run(() => LabelAddedAsync(e).GetAwaiter());
        }

        public async Task LabelAddedAsync(LabelEventArgs e)
        {
            // If this is a new note, we'll handle the labels when the note is added.
            if (e.Note.NoteId != null)
            {
                e.Label = await e.Note.CreateAssociateLabel(Mediator, e.Label.Text);
            }
        }

        public void CloseNotePaneRequested(object sender, RoutedEventArgs args)
        {
            NoteControlVisibility = Visibility.Collapsed;
        }

        // ReSharper restore UnusedMember.Global

        #endregion

        #region VerseControlMethods

        private Visibility _noteControlVisibility = Visibility.Collapsed;
        public Visibility NoteControlVisibility
        {
            get => _noteControlVisibility;
            set => Set(ref _noteControlVisibility, value);
        }

        private Visibility _translationControlVisibility = Visibility.Collapsed;
        public Visibility TranslationControlVisibility
        {
            get => _translationControlVisibility;
            set => Set(ref _translationControlVisibility, value);
        }


        // public Visibility NoteControlVisibility { get; set; } = Visibility.Collapsed;
        private void DisplayNote(TokenDisplayViewModel tokenDisplayViewModel)
        {
            CurrentTokenDisplayViewModel = tokenDisplayViewModel;
            NoteControlVisibility = Visibility.Visible;
        }

        //private void HideNote()
        //{
        //    NoteControlVisibility = Visibility.Collapsed;
        //    NotifyOfPropertyChange(nameof(NoteControlVisibility));
        //}

        //public Note CurrentNote { get; set; }

        private TokenDisplayViewModel _currentTokenDisplayViewModel;
        private IEnumerable<TranslationOption> _translationOptions;
        private TranslationOption? _currentTranslationOption;

        public TokenDisplayViewModel CurrentTokenDisplayViewModel
        {
            get => _currentTokenDisplayViewModel;
            set => Set(ref _currentTokenDisplayViewModel, value);
        }

        public IEnumerable<TranslationOption> TranslationOptions
        {
            get => _translationOptions;
            set => Set(ref _translationOptions, value);
        }

        public TranslationOption? CurrentTranslationOption
        {
            get => _currentTranslationOption;
            set => Set(ref _currentTranslationOption, value);
        }

        //public IEnumerable<TranslationOption> TranslationOptions { get; set; }
        //public TranslationOption CurrentTranslationOption { get; set; }
        private async void DisplayTranslation(TranslationEventArgs e)
        {
            await Task.Factory.StartNew(async () =>
            {
                OnUIThread(() => ProgressBarVisibility = Visibility.Visible);

                CurrentTokenDisplayViewModel = e.TokenDisplayViewModel;
                TranslationOptions = await GetTranslationOptions(e.Translation);
                CurrentTranslationOption =
                    TranslationOptions.FirstOrDefault(to => to.Word == e.Translation.TargetTranslationText);
                
                OnUIThread(() => TranslationControlVisibility = Visibility.Visible);
                OnUIThread(() => ProgressBarVisibility = Visibility.Collapsed);
            });
        }

        private async Task<IEnumerable<TranslationOption>> GetTranslationOptions(Translation translation)
        {
            var translationModelEntry = await CurrentTranslationSet.GetTranslationModelEntryForToken(translation.SourceToken);
            var translationOptions = translationModelEntry.OrderByDescending(option => option.Value)
                .Select(option => new TranslationOption { Word = option.Key, Count = option.Value })
                .Take(4)
                .ToList();
            return translationOptions;
        }

        #endregion

        //private void HideTranslation()
        //{
        //    TranslationControlVisibility = Visibility.Collapsed;
        //    NotifyOfPropertyChange(nameof(TranslationControlVisibility));
        //}

        //private IEnumerable<TranslationOption> GetMockTranslationOptions(string sourceTranslation)
        //{
        //    var result = new List<TranslationOption>();

        //    var random = new Random();
        //    var optionCount = random.Next(4) + 2;     // 2-5 options
        //    var remainingPercentage = 100d;

        //    var basePercentage = random.NextDouble() * remainingPercentage;
        //    result.Add(new TranslationOption { Word = sourceTranslation, Count = basePercentage });
        //    remainingPercentage -= basePercentage;

        //    for (var i = 1; i < optionCount - 1; i++)
        //    {
        //        var percentage = random.NextDouble() * remainingPercentage;
        //        result.Add(new TranslationOption { Word = GetMockOogaWord(), Count = percentage });
        //        remainingPercentage -= percentage;
        //    }

        //    result.Add(new TranslationOption { Word = GetMockOogaWord(), Count = remainingPercentage });

        //    return result.OrderByDescending(to => to.Count);
        //}

        //private readonly List<string> MockOogaWords = new() { "Ooga", "booga", "bong", "biddle", "foo", "boi", "foodie", "fingle", "boing", "la" };


        //private static int mockOogaWordsIndexer_;

        //private string GetMockOogaWord()
        //{
        //    var result = MockOogaWords[mockOogaWordsIndexer_++];
        //    if (mockOogaWordsIndexer_ == MockOogaWords.Count) mockOogaWordsIndexer_ = 0;
        //    return result;
        //}
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
