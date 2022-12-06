using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using EngineToken = ClearBible.Engine.Corpora.Token;
using FontFamily = System.Windows.Media.FontFamily;
using ParallelCorpus = ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class EnhancedViewModel : DashboardConductorAllActive<object>, IPaneViewModel, 
        IHandle<TokenizedTextCorpusLoadedMessage>,
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
        private string? _message;
        private BookInfo? _currentBook;


        private string CurrentBookDisplay => string.IsNullOrEmpty(CurrentBook?.Code) ? string.Empty : $"<{CurrentBook.Code}>";

        // used for storing the displayed corpus order
        public List<EnhancedViewItemMetadatum> EnhancedViewItemMetadata = new();

        private readonly List<TokenProject> _tokenProjects = new();
        private readonly List<AddTokenizedCorpusToEnhancedViewMessage> _projectMessages = new();

        private readonly List<ParallelProject> _parallelProjects = new();
        private readonly List<AddAlignmentToEnhancedViewMessage> _parallelMessages = new();



        #endregion //Member Variables

        #region Public Properties

        public bool IsRtl { get; set; }

        public List<string> WorkingJobs { get; set; } = new();

        public NoteManager NoteManager { get; set; }

        private VerseDisplayViewModel _selectedVerseDisplayViewModel;
        public VerseDisplayViewModel SelectedVerseDisplayViewModel
        {
            get => _selectedVerseDisplayViewModel;
            set => Set(ref _selectedVerseDisplayViewModel, value);
        }


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
                    CurrentBcv.SetVerseFromId(_projectManager!.CurrentVerse);
                }

                Set(ref _paratextSync, value);
            }
        }

        private Dictionary<string, string> _bcvDictionary;
        public Dictionary<string, string> BcvDictionary
        {
            get => _bcvDictionary;
            set => Set(ref _bcvDictionary, value);
        }

        private BookChapterVerseViewModel _currentBcv = new();
        public BookChapterVerseViewModel CurrentBcv
        {
            get => _currentBcv;
            set => Set(ref _currentBcv, value);
        }

        private int _verseOffsetRange;
        public int VerseOffsetRange
        {
            get => _verseOffsetRange;
            set
            {
                var set = Set(ref _verseOffsetRange, value);
                if (set)
                {
                    #pragma warning disable CS4014
                    VerseChangeRerender();
                    #pragma warning restore CS4014
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
                    ProjectManager!.CurrentVerse = value;
                    // push to Paratext
                    if (ParatextSync && !DashboardProjectManager.IncomingChangesStarted)
                    {
                        Task.Run(() =>
                            ExecuteRequest(new SetCurrentVerseCommand(value), CancellationToken.None)
                        );
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
      

        private string _currentCorpusName = string.Empty;
        public string CurrentCorpusName
        {
            get => _currentCorpusName;
            set => Set(ref _currentCorpusName, value);
        }

        public string? TokenizationType
        {
            get => _tokenizationType;
            set => Set(ref _tokenizationType, value);
        }

        //public TokenizedTextCorpus? CurrentTokenizedTextCorpus
        //{
        //    get => _currentTokenizedTextCorpus;
        //    set => Set(ref _currentTokenizedTextCorpus, value);
        //}

        private Visibility? _progressBarVisibility = Visibility.Visible;
        public Visibility? ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => Set(ref _progressBarVisibility, value);
        }

        public string? Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        private BindableCollection<TokensTextRow>? _verses;
        public BindableCollection<TokensTextRow>? Verses
        {
            get => _verses;
            set => Set(ref _verses, value);
        }

        public EngineStringDetokenizer Detokenizer { get; set; } = new EngineStringDetokenizer(new LatinWordDetokenizer());
        public EngineStringDetokenizer TargetDetokenizer { get; set; } = new EngineStringDetokenizer(new LatinWordDetokenizer());

        public IEnumerable<Translation> CurrentTranslations { get; set; }

        //private IEnumerable<Label> _labelSuggestions;
        //public IEnumerable<Label> LabelSuggestions
        //{
        //    get => _labelSuggestions;
        //    set => Set(ref _labelSuggestions, value);
        //}

        private TokenDisplayViewModel _currentToken;
        public TokenDisplayViewModel TokenForTranslation
        {
            get => _currentToken;
            set => Set(ref _currentToken, value);
        }

        private TokenDisplayViewModelCollection _selectedTokens = new();
        public TokenDisplayViewModelCollection SelectedTokens
        {
            get => _selectedTokens;
            set => Set(ref _selectedTokens, value);
        }


        private IEnumerable<TranslationOption> _translationOptions;
        public IEnumerable<TranslationOption> TranslationOptions
        {
            get => _translationOptions;
            set => Set(ref _translationOptions, value);
        }

        private TranslationOption? _currentTranslationOption;
        public TranslationOption? CurrentTranslationOption
        {
            get => _currentTranslationOption;
            set => Set(ref _currentTranslationOption, value);
        }

        #endregion //Observable Properties

        #region IPaneViewModel


        public ICommand RequestCloseCommand { get; set; }

        //private string _title = null;
        private string? _contentId;
        private bool _isSelected;
        private bool _isActive;
        #endregion //Member Variables

        #region Public Properties

        public Guid PaneId { get; set; }
        public DockSide DockSide { get; set; }

        public ImageSource? IconSource { get; protected set; }

        public string? ContentId
        {
            get => _contentId;
            set => Set(ref _contentId, value);
        }

        //public DockSide DockSide { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        public new bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    Set(ref _isActive, value);

                    if (this.ContentId == "ENHANCEDVIEW" && value)
                    {
                        // send out a notice that the active document has changed
                        EventAggregator.PublishOnUIThreadAsync(new ActiveDocumentMessage(PaneId));
                    }
                }
            }
        }
        public async Task RequestClose(object obj)
        {
            await EventAggregator.PublishOnUIThreadAsync(new CloseDockingPane(this.PaneId));
        }
        #endregion

        #region Constructor

        // ReSharper disable once UnusedMember.Global
#pragma warning disable CS8618
        public EnhancedViewModel(INavigationService navigationService, ILogger<EnhancedViewModel> logger,
            DashboardProjectManager? projectManager, NoteManager noteManager, IEventAggregator? eventAggregator, IMediator mediator,
            ILifetimeScope? lifetimeScope, IServiceProvider serviceProvider) :
            base(navigationService: navigationService, logger: logger, projectManager: projectManager,
                eventAggregator: eventAggregator, mediator: mediator, lifetimeScope: lifetimeScope)
#pragma warning restore CS8618
        {

            _logger = logger;
            _projectManager = projectManager;
            NoteManager = noteManager;
            _serviceProvider = serviceProvider;

            Title = "⳼ " + LocalizationStrings.Get("Windows_EnhancedView", Logger!);
            ContentId = "ENHANCEDVIEW";

            ProgressBarVisibility = Visibility.Collapsed;

            MoveCorpusDownRowCommand = new RelayCommand(MoveCorpusDown);
            MoveCorpusUpRowCommand = new RelayCommand(MoveCorpusUp);
            DeleteCorpusRowCommand = new RelayCommand(DeleteCorpusRow);

            RequestCloseCommand = new RelayCommandAsync(RequestClose);

            VerseDisplay.EventAggregator = eventAggregator;
            PaneId = Guid.NewGuid();
        }


        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            DisplayName = "Enhanced View";
          
           // await ActivateItemAsync<TestEnhancedViewItemViewModel>(cancellationToken);
            await base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            // grab the dictionary of all the verse lookups
            if (ProjectManager?.CurrentParatextProject is not null)
            {
                BcvDictionary = ProjectManager.CurrentParatextProject.BcvDictionary;

                var books = BcvDictionary.Values.GroupBy(b => b.Substring(0, 3))
                    .Select(g => g.First())
                    .ToList();

                foreach (var book in books)
                {
                    var bookId = book.Substring(0, 3);

                    var bookName = BookChapterVerseViewModel.GetShortBookNameFromBookNum(bookId);

                    CurrentBcv.BibleBookList?.Add(bookName);
                }
            }
            else
            {
                BcvDictionary = new Dictionary<string, string>();
            }

            CurrentBcv.SetVerseFromId(_projectManager!.CurrentVerse);
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
                    TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed
                }), cancellationToken);
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }


        #endregion //Constructor

        #region Methods


        private async Task VerseChangeRerender()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < _tokenProjects.Count; i++)
            {
                ProgressBarVisibility = Visibility.Visible;
                _cancellationTokenSource = new CancellationTokenSource();

                await ShowCorpusText(_projectMessages[i], _cancellationTokenSource.Token, _cancellationTokenSource.Token);
            }

            foreach (var t in _parallelMessages)
            {
                ProgressBarVisibility = Visibility.Visible;
                _cancellationTokenSource = new CancellationTokenSource();

                await ShowParallelTranslation(t, _cancellationTokenSource.Token,
                    _cancellationTokenSource.Token);
            }

            sw.Stop();
            _logger.LogInformation("VerseChangeRerender took {0} ms", sw.ElapsedMilliseconds);
        }

        private void MoveCorpusUp(object obj)
        {
            var row = obj as EnhancedViewItemViewModel;
            var index = MoveableItems.Select((element, index) => new { element, index })
                .FirstOrDefault(x => x.element.Equals(row))?.index ?? -1;

            if (index < 1)
            {
                return;
            }

            MoveableItems.Move(index, index - 1);
        }

        private void MoveCorpusDown(object obj)
        {
            var row = obj as EnhancedViewItemViewModel;
            var index = MoveableItems.Select((element, index) => new { element, index })
                .FirstOrDefault(x => x.element.Equals(row))?.index ?? -1;

            if (index == Items.Count - 1)
            {
                return;
            }
            MoveableItems.Move(index, index +1);

        }

        private void DeleteCorpusRow(object obj)
        {
            var row = (EnhancedViewItemViewModel)obj;


            var index = Items.Select((element, index) => new { element, index })
                .FirstOrDefault(x => x.element.Equals(row))?.index ?? -1;
            Items.RemoveAt(index);
            // remove from the grouping for saving
            EnhancedViewItemMetadata.RemoveAt(index);

            if (row is VerseAwareEnhancedViewItemViewModel)
            {
                var item = (VerseAwareEnhancedViewItemViewModel)row;
                // remove from stored collection
                var project = _parallelProjects.FirstOrDefault(x => x.ParallelCorpusId == item.ParallelCorpusId);
                if (project != null)
                {
                    _parallelProjects.Remove(project);
                }

                var parallelMessages =
                    _parallelMessages.FirstOrDefault(x => Guid.Parse(x.ParallelCorpusId!) == item.ParallelCorpusId);
                if (parallelMessages is not null)
                {
                    _parallelMessages.Remove(parallelMessages);
                }

                // remove stored collection
                var tokenProject = _tokenProjects.FirstOrDefault(x => x.CorpusId == item.CorpusId);
                if (tokenProject is not null)
                {
                    _tokenProjects.Remove(tokenProject);
                }

                // remove stored message
                var tokenMessage = _projectMessages.FirstOrDefault(x => x.CorpusId == item.CorpusId);
                if (tokenMessage is not null)
                {
                    _projectMessages.Remove(tokenMessage);
                }
            }
        }

        public void InnerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var verseDisplayViewModel = e.AddedItems[0] as VerseDisplayViewModel;
                if (verseDisplayViewModel is not null)
                {
                    SelectedVerseDisplayViewModel = verseDisplayViewModel;
                }
            }
        }


        //public void LaunchMirrorView(double actualWidth, double actualHeight)
        //{
        //    LaunchMirrorView<TextCollectionsView>.Show(this, actualWidth, actualHeight);
        //}


        #region Corpus

        public async Task ShowCorpusTokens(AddTokenizedCorpusToEnhancedViewMessage message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received TokenizedTextCorpusMessage.");
            _handleAsyncRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var localCancellationToken = _cancellationTokenSource.Token;


            ProgressBarVisibility = Visibility.Visible;


            //CODE REVIEW:  what is the purpose of the following code between *********

            // ***********************************************************************************************************
            //TokenProject? project = null;
            //// check if we have this already
            //try
            //{
            //    if (_tokenProjects.Count > 0)
            //    {
            //        project = _tokenProjects.First(p => p.ParatextProjectId == message.ParatextProjectId
            //                                            && p.TokenizationType == message.TokenizationType) ?? null;
            //    }
            //}
            //catch (Exception e)
            //{
            //    _logger.LogError(e.Message);
            //}
            // ***********************************************************************************************************

            ProgressBarVisibility = Visibility.Visible;

            await ShowCorpusText(message, cancellationToken, localCancellationToken);
        }

        public async Task ShowCorpusText(AddTokenizedCorpusToEnhancedViewMessage message, CancellationToken cancellationToken,
            CancellationToken localCancellationToken)
        {
            // add this to the job stack
            WorkingJobs.Add(message.TokenizedTextCorpusId.ToString());

            _ = await Task.Factory.StartNew(async () =>
            {
                try
                {
                    ParatextProjectMetadata metadata;

                    if (message.ParatextProjectId == ManuscriptIds.HebrewManuscriptId)
                    {
                        // our fake Manuscript corpus
                        var books = BookInfo.GenerateScriptureBookList();

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
                        // our fake Greek Manuscript corpus
                        var books = BookInfo.GenerateScriptureBookList();

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
                        var result = await _projectManager?.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken)!;
                        if (result.Success && result.HasData)
                        {
                            metadata = result.Data!.FirstOrDefault(b =>
                                           b.Id == message.ParatextProjectId!.Replace("-", "")) ??
                                       throw new InvalidOperationException();
                        }
                        else
                        {
                            throw new InvalidOperationException(result.Message);
                        }
                    }


                    TokenizationType = message.TokenizationType;

                    var bookFound = metadata.AvailableBooks.Any(b => b.Code == CurrentBcv.BookName);
                    CurrentBook = metadata.AvailableBooks.FirstOrDefault(b => b.Code == CurrentBcv.BookName);
                    //foreach (var book in metadata.AvailableBooks.Where(book => book.Code == CurrentBcv.BookName))
                    //{
                    //    CurrentBook = metadata?.AvailableBooks.First(b => b.Code == CurrentBcv.BookName);
                    //    bookFound = true;
                    //}

                    var project = _tokenProjects.FirstOrDefault(p => p.CorpusId == message.CorpusId);

                    TokenizedTextCorpus currentTokenizedTextCorpus;
                    if (project is null)
                    {
                        // get the entirety of text for this corpus
                        
                        currentTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator!, new TokenizedTextCorpusId(message.TokenizedTextCorpusId.Value));

                        // add this corpus to our master list
                        _tokenProjects.Add(new TokenProject
                        {
                            ParatextProjectId = message.ParatextProjectId,
                            ProjectName = message.ProjectName,
                            TokenizationType = message.TokenizationType,
                            CorpusId = message.CorpusId.Value,
                            TokenizedTextCorpusId = message.TokenizedTextCorpusId.Value,
                            Metadata = metadata,
                            TokenizedTextCorpus = currentTokenizedTextCorpus,
                        });

                        // add in the message so we can get it later
                        _projectMessages.Add(message);
                    }
                    else
                    {
                        currentTokenizedTextCorpus = project.TokenizedTextCorpus;
                    }

                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Getting book '{CurrentBook?.Code}'...",
                            StartTime = DateTime.Now,
                            TaskLongRunningProcessStatus = LongRunningTaskStatus.Running
                        }), cancellationToken);


                    List<TokensTextRow> tokensTextRows = new();
                    try
                    {
                        // get the rows for the current book and chapter
                        tokensTextRows = currentTokenizedTextCorpus[CurrentBook?.Code]
                            .GetRows()
                            .WithCancellation(localCancellationToken)
                            .Cast<TokensTextRow>()
                            .Where(ttr => ttr
                                .Tokens
                                .Count(t => t
                                    .TokenId
                                    .ChapterNumber == CurrentBcv.ChapterNum) > 0)
                            .ToList();
                    }
                    catch (Exception )
                    {
                        bookFound = false;
                    }


                    // get the row for the current verse
                    var index = 0;
                    for (var i = 0; i < tokensTextRows.Count; i++)
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
                    var tokensTextRowsRange = tokensTextRows.Skip(lowEnd).Take(offset).ToList();

                    // set the title to include the verse range
                    var title = message.ProjectName + " - " + message.TokenizationType;
                    if (tokensTextRowsRange.Count == 1)
                    {
                        title += $" ({CurrentBcv.BookName} {CurrentBcv.ChapterNum}:{CurrentBcv.VerseNum})";
                    }
                    else
                    {
                        // check to see if we actually have a verse
                        if (tokensTextRowsRange.Count > 0)
                        {
                            var startNum = (VerseRef)tokensTextRowsRange[0].Ref;
                            var endNum = (VerseRef)tokensTextRowsRange[^1].Ref;
                            title += $" ({CurrentBcv.BookName} {CurrentBcv.ChapterNum}:{startNum.VerseNum} - {endNum.VerseNum})";

                        }
                        else
                        {
                            title += $" ({CurrentBcv.BookName} {CurrentBcv.ChapterNum}:{CurrentBcv.VerseNum})";
                        }
                    }

                    // combine verse list into one VerseDisplayViewModel
                    BindableCollection<VerseDisplayViewModel> verses = new();

                    foreach (var textRow in tokensTextRowsRange)
                    {
                        var verseDisplayViewModel = _serviceProvider.GetService<VerseDisplayViewModel>();
                        //FIXME: detokenizer should come from message.Detokenizer.
                        await verseDisplayViewModel!.ShowCorpusAsync(
                            textRow,
                            //FIXME:surface serialization message.detokenizer,
                            new EngineStringDetokenizer(new LatinWordDetokenizer()),
                            message.IsRTL.Value);

                        verses.Add(verseDisplayViewModel);
                    }

                    //if (verses.Any())
                    //{
                    //    // Label suggestions are the same for each VerseDisplayViewModel
                    //    LabelSuggestions = verses.First().LabelSuggestions;
                    //}
                    if (bookFound)
                    {
                        OnUIThread(async () =>
                        {
                            await UpdateVersesDisplay(message, verses, title, false);
                            NotifyOfPropertyChange(() => Items);
                        });
                    }
                    else
                    {
                        OnUIThread(async () => { await UpdateVerseDisplayWhenBookOutOfRange(message); });
                    }

                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Found {tokensTextRows.Count} TokensTextRow entities.",
                            StartTime = DateTime.Now,
                            TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed
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
                                TaskLongRunningProcessStatus = LongRunningTaskStatus.Failed
                            }), cancellationToken);
                    }

                    OnUIThread(async () => { await UpdateVerseDisplayWhenBookOutOfRange(message); });
                }
                finally
                {
                    _handleAsyncRunning = false;
                    _cancellationTokenSource?.Dispose();

                    // remove from the job stack
                    for (int i = WorkingJobs.Count - 1; i >= 0; i--)
                    {
                        if (WorkingJobs[i] == message.TokenizedTextCorpusId.ToString())
                        {
                            WorkingJobs.RemoveAt(i);
                        }
                    }

                    if (WorkingJobs.Count == 0)
                    {
                        ProgressBarVisibility = Visibility.Collapsed;
                    }
                }
            }, cancellationToken);
        }

        private async Task UpdateVerseDisplayWhenBookOutOfRange(AddTokenizedCorpusToEnhancedViewMessage message)
        {
            await UpdateVersesDisplay(message, new BindableCollection<VerseDisplayViewModel>(),
                message.ProjectName + " - " + message.TokenizationType +
                "    No verse data in this verse range", false);
            ProgressBarVisibility = Visibility.Collapsed;
        }

        public async Task<DAL.Alignment.Translation.TranslationSet> GetTranslationSet(string translationSetId)
        {
            return await DAL.Alignment.Translation.TranslationSet.Get(new TranslationSetId(Guid.Parse(translationSetId)), Mediator);
        }
        public static async Task<DAL.Alignment.Translation.AlignmentSet> GetAlignmentSet(string alignmentSetId, IMediator mediator)
        {
            return await DAL.Alignment.Translation.AlignmentSet.Get(new AlignmentSetId(Guid.Parse(alignmentSetId)), mediator);
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

        private async Task UpdateVersesDisplay(AddTokenizedCorpusToEnhancedViewMessage message, BindableCollection<VerseDisplayViewModel> verses, string title, bool showTranslations)
        {
            // get the font family for this project
            var mainViewModel = IoC.Get<MainViewModel>();
            var fontFamily = mainViewModel.GetFontFamilyFromParatextProjectId(message.ParatextProjectId);

            FontFamily family;
            try
            {
                family = new(fontFamily);
            }
            catch (Exception e)
            {
                family = new("Segoe UI");
            }


            var brush = GetCorpusBrushColor(message);

           
            var row = VerseAwareEnhancedViewItemViewModels.FirstOrDefault(v => v.CorpusId == message.CorpusId);
            if (row is null)
            {
                var viewModel = await ActivateItemAsync<VerseAwareEnhancedViewItemViewModel>();
                viewModel.CorpusId = message.CorpusId.Value;
                viewModel.BorderColor = brush;
                viewModel.ShowTranslation = showTranslations;
                viewModel.Title = title;
                viewModel.Verses = verses;
                viewModel.IsRtl = message.IsRTL.Value;
                viewModel.SourceFontFamily = family;

                // add to the grouping for saving
                EnhancedViewItemMetadata.Add(new EnhancedViewItemMetadatum
                {
                    MessageType = MessageType.ShowTokenizationWindowMessage,
                    Data = message
                });
            }
            else
            {
                row.CorpusId = message.CorpusId.Value;
                row.BorderColor = brush;
                row.ShowTranslation = showTranslations;
                row.Title = title;
                row.Verses = verses;
                row.IsRtl = message.IsRTL.Value;
            }

            //do a dump of VerseDisplayViewModel Ids
            foreach (var verseDisplayViewModel in verses)
            {
                Debug.WriteLine($"INCOMMING ID: {verseDisplayViewModel.Id}");
            }
            NotifyOfPropertyChange(() => Items);
        }

        private static Brush? GetCorpusBrushColor(AddTokenizedCorpusToEnhancedViewMessage message)
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

        #endregion

        #region Parallel

        public async Task ShowParallelTranslationTokens(AddAlignmentToEnhancedViewMessage message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received TokenizedTextCorpusMessage.");
            _handleAsyncRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var localCancellationToken = _cancellationTokenSource.Token;
            ProgressBarVisibility = Visibility.Visible;

            await ShowParallelTranslation(message, cancellationToken, localCancellationToken);
        }

        public async Task ShowParallelTranslation(AddAlignmentToEnhancedViewMessage message,
            CancellationToken cancellationToken, CancellationToken localCancellationToken)
        {

            WorkingJobs.Add(message.TranslationSetId);

            var msg = _parallelMessages.Where(p =>
                p.TranslationSetId == message.TranslationSetId && p.AlignmentSetId == message.AlignmentSetId && p.ParallelCorpusId == message.ParallelCorpusId).ToList();
            if (msg.Count == 0)
            {
                _parallelMessages.Add(message);
            }

            // current project
            _ = await Task.Factory.StartNew(async () =>
            {
                try
                {
                    ProgressBarVisibility = Visibility.Visible;

                    var verseTokens = await BuildTokenDisplayViewModels(message);
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Found {verseTokens.Count} TokensTextRow entities.",
                            StartTime = DateTime.Now,
                            TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed
                        }), cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "The attempt to ShowNewParallelTranslation failed.");
                    if (!localCancellationToken.IsCancellationRequested)
                    {
                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                            new BackgroundTaskStatus
                            {
                                Name = "Fetch Book",
                                EndTime = DateTime.Now,
                                ErrorMessage = $"{ex}",
                                TaskLongRunningProcessStatus = LongRunningTaskStatus.Failed
                            }), cancellationToken);
                    }
                }
                finally
                {
                    _handleAsyncRunning = false;
                    if (_cancellationTokenSource != null)
                        _cancellationTokenSource.Dispose();

                    // remove from the job stack
                    for (int i = WorkingJobs.Count - 1; i >= 0; i--)
                    {
                        if (WorkingJobs[i] == message.TranslationSetId)
                        {
                            WorkingJobs.RemoveAt(i);
                        }
                    }

                    if (WorkingJobs.Count == 0)
                    {
                        ProgressBarVisibility = Visibility.Collapsed;
                    }
                }
            }, cancellationToken);
        }

        private async Task<List<TokenDisplayViewModel>> BuildTokenDisplayViewModels(AddAlignmentToEnhancedViewMessage message)
        {
            List<TokenDisplayViewModel> verseTokens = new();
            var versesOut = new BindableCollection<VerseDisplayViewModel>();

            List<string> verseRange = GetValidVerseRange(CurrentBcv.BBBCCCVVV, VerseOffsetRange);


            var rows = await VerseTextRow(Convert.ToInt32(CurrentBcv.BBBCCCVVV), message);

            if (rows is null)
            {

                OnUIThread(async () =>
                {
                    await UpdateParallelCorpusDisplay(message, versesOut, message.ParallelCorpusDisplayName + "    No verse data in this verse range", true);
                    NotifyOfPropertyChange(() => Items);
                });
            }
            else
            {
                //NotesDictionary = await Note.GetAllDomainEntityIdNotes(Mediator ?? throw new InvalidDataEngineException(name: "Mediator", value: "null"));
                foreach (var row in rows)
                {
                    var verseDisplayViewModel = _serviceProvider!.GetService<VerseDisplayViewModel>();
                    if (message.AlignmentSetId != null)

                        // ALIGNMENTS
                        await verseDisplayViewModel!.ShowAlignmentsAsync(
                            row ?? throw new InvalidDataEngineException(name: "row", value: "null"),
                            await GetAlignmentSet(message.AlignmentSetId!, Mediator!),
                            //FIXME:surface serialization message.SourceDetokenizer, 
                            new EngineStringDetokenizer(new LatinWordDetokenizer()),
                            message.IsRTL,
                            //FIXME:surface serialization message.TargetDetokenizer ?? throw new InvalidParameterEngineException(name: "message.TargetDetokenizer", value: "null", message: "message.TargetDetokenizer must not be null when message.AlignmentSetId is not null."),
                            new EngineStringDetokenizer(new LatinWordDetokenizer()),
                            message.IsTargetRTL ?? throw new InvalidDataEngineException(name: "IsTargetRTL", value: "null"));
                    else

                        // INTERLINEARS
                        await verseDisplayViewModel!.ShowTranslationAsync(
                            row ?? throw new InvalidDataEngineException(name: "row", value: "null"),
                            await GetTranslationSet(message.TranslationSetId ?? throw new InvalidDataEngineException(name: "message.TranslationSetId", value: "null")),
                            //FIXME:surface serialization message.SourceDetokenizer, 
                            new EngineStringDetokenizer(new LatinWordDetokenizer()),
                            message.IsRTL);
                    versesOut.Add(verseDisplayViewModel);
                }

                var title = message.ParallelCorpusDisplayName ?? string.Empty;
                if (message.AlignmentSetId != null)
                {
                    // ALIGNMENTS
                    title += " " + LocalizationStrings.Get("EnhancedView_Alignment", _logger);
                }
                else
                {
                    // INTERLINEARS
                    title += " " + LocalizationStrings.Get("EnhancedView_Interlinear", _logger);
                }

                var bcv = new BookChapterVerseViewModel();
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

                OnUIThread(async () =>
                {
                    await UpdateParallelCorpusDisplay(message, versesOut, title);
                    NotifyOfPropertyChange(() => Items);
                });
            }

            return verseTokens;
        }

        private List<string> GetValidVerseRange(string bbbcccvvv, int offset)
        {
            List<string> verseRange = new() { bbbcccvvv };

            var currentVerse = Convert.ToInt32(bbbcccvvv.Substring(6));

            // get lower range first
            var j = 1;
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

        private async Task<List<EngineParallelTextRow?>> VerseTextRow(int bbbcccvvv, AddAlignmentToEnhancedViewMessage message)
        {
            try
            {
                var corpusIds = await ParallelCorpus.GetAllParallelCorpusIds(Mediator);
                var guid = Guid.Parse(message.ParallelCorpusId);


                var stopwatch = Stopwatch.StartNew();
  
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
                        parallelTextRows = corpus,
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

        private IEnumerable<VerseAwareEnhancedViewItemViewModel> VerseAwareEnhancedViewItemViewModels  => Items.Where(item => item.GetType() == typeof(VerseAwareEnhancedViewItemViewModel)).Cast<VerseAwareEnhancedViewItemViewModel>();

        private async Task UpdateParallelCorpusDisplay(AddAlignmentToEnhancedViewMessage message,
            BindableCollection<VerseDisplayViewModel> verses, string title, bool showTranslations = true)
        {
            // same color as defined in SharedVisualTemplates.xaml
            Brush brush = Brushes.SaddleBrown;

            // get the fontfamily for this project
            var mainViewModel = IoC.Get<MainViewModel>();
            var sourceFontFamily = mainViewModel.GetFontFamilyFromParatextProjectId(message.SourceParatextId);
            var targetFontFamily = mainViewModel.GetFontFamilyFromParatextProjectId(message.TargetParatextId);

            FontFamily familySource;
            try
            {
                familySource = new(sourceFontFamily);
            }
            catch (Exception e)
            {
                familySource = new("Segoe UI");
            }

            FontFamily familyTarget;
            try
            {
                familyTarget = new(targetFontFamily);
            }
            catch (Exception e)
            {
                familyTarget = new("Segoe UI");
            }



            VerseAwareEnhancedViewItemViewModel? row;
            if (message.AlignmentSetId is null)
            {
                // interlinear
               row = VerseAwareEnhancedViewItemViewModels.FirstOrDefault(v =>
                    v.CorpusId == Guid.Parse(message.ParallelCorpusId) &&
                    v.TranslationSetId == Guid.Parse(message.TranslationSetId));

            }
            else
            {
                // alignment
               row = VerseAwareEnhancedViewItemViewModels.FirstOrDefault(v =>
                    v.CorpusId == Guid.Parse(message.ParallelCorpusId) &&
                    v.AlignmentSetId == Guid.Parse(message.AlignmentSetId));

            }


            if (row is null)
            {
                Guid alignmentSetId = Guid.Empty;
                if (message.AlignmentSetId is not null)
                {
                    alignmentSetId = Guid.Parse(message.AlignmentSetId);
                    brush = Brushes.DarkGreen;
                }

                Guid translationSetId = Guid.Empty;
                if (message.TranslationSetId is not null)
                {
                    translationSetId = Guid.Parse(message.TranslationSetId);
                }

                var viewModel = await ActivateItemAsync<VerseAwareEnhancedViewItemViewModel>();

                viewModel.AlignmentSetId = alignmentSetId;
                viewModel.ParallelCorpusId = Guid.Parse(message.ParallelCorpusId);
                viewModel.TranslationSetId = translationSetId;
                viewModel.CorpusId = Guid.Parse(message.ParallelCorpusId);
                viewModel.BorderColor = brush;
                viewModel.ShowTranslation = showTranslations;
                viewModel.Title = title;
                viewModel.Verses = verses;
                viewModel.IsRtl = message.IsRTL;
                viewModel.IsTargetRtl = message.IsTargetRTL ?? false;
                viewModel.SourceFontFamily = familySource;
                viewModel.TargetFontFamily = familyTarget;
                viewModel.TranslationFontFamily = familyTarget;

            
                // add to the grouping for saving
                EnhancedViewItemMetadata.Add(new EnhancedViewItemMetadatum
                {
                    MessageType = MessageType.ShowParallelTranslationWindowMessage,
                    Data = message
                });
            }
            else
            {
                row.Title = title;
                row.Verses = verses;
                row.BorderColor = brush;
            }

            NotifyOfPropertyChange(() => Items);
        }

        #endregion


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
                CurrentBcv.SetVerseFromId(message.Verse);
            }
        }

        public async Task HandleAsync(ProjectChangedMessage message, CancellationToken cancellationToken)
        {
            if (ProjectManager?.CurrentParatextProject is not null)
            {
                // send to log
                await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{DisplayName}: Project Change"), cancellationToken);

                DashboardProjectManager.IncomingChangesStarted = true;

                // set the CurrentBcv prior to listening to the event
                CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);

                NotifyOfPropertyChange(() => CurrentBcv);
                DashboardProjectManager.IncomingChangesStarted = false;
            }
            else
            {
                BcvDictionary = new Dictionary<string, string>();
            }
        }

        public async Task HandleAsync(TokenizedTextCorpusLoadedMessage message, CancellationToken cancellationToken)
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
                    var currentTokenizedTextCorpus = message.TokenizedTextCorpus;
                    TokenizationType = message.TokenizationName;
                    CurrentBook = message.ProjectMetadata.AvailableBooks.First();
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = "Fetch Book",
                            Description = $"Getting book '{CurrentBook?.Code}'...",
                            StartTime = DateTime.Now,
                            TaskLongRunningProcessStatus = LongRunningTaskStatus.Running
                        }), cancellationToken);

                    var tokensTextRows =
                        currentTokenizedTextCorpus[CurrentBook?.Code]
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
                            TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed
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
                                TaskLongRunningProcessStatus = LongRunningTaskStatus.Failed
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

            if (incomingMessage.Name == "Fetch Book" && incomingMessage.TaskLongRunningProcessStatus == LongRunningTaskStatus.CancellationRequested)
            {
                _cancellationTokenSource?.Cancel();

                // return that your task was cancelled
                incomingMessage.EndTime = DateTime.Now;
                incomingMessage.TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed;
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

        #region VerseDisplayControl

        private void UpdateSelection(TokenDisplayViewModel token, TokenDisplayViewModelCollection selectedTokens, bool addToSelection)
        {
            if (addToSelection)
            {
                foreach (var selectedToken in selectedTokens)
                {
                    if (!SelectedTokens.Contains(selectedToken))
                    {
                        SelectedTokens.Add(selectedToken);
                    }
                }

                if (!token.IsSelected)
                {
                    SelectedTokens.Remove(token);
                }
            }
            else
            {
                SelectedTokens = selectedTokens;
            }
            EventAggregator.PublishOnUIThreadAsync(new SelectionUpdatedMessage(SelectedTokens));
        }

        public void TokenClicked(object sender, TokenEventArgs e)
        {
            Task.Run(() => TokenClickedAsync(e).GetAwaiter());
        }

        public async Task TokenClickedAsync(TokenEventArgs e)
        {
            UpdateSelection(e.TokenDisplay, e.SelectedTokens, (e.ModifierKeys & ModifierKeys.Control) > 0);
            await NoteManager.SetCurrentNoteIds(SelectedTokens.NoteIds);
            NoteControlVisibility = SelectedTokens.Any(t => t.HasNote) ? Visibility.Visible : Visibility.Collapsed;
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId})";
        }

        public void TokenRightButtonDown(object sender, TokenEventArgs e)
        {
            Task.Run(() => TokenRightButtonDownAsync(e).GetAwaiter());
        }        
        
        public async Task TokenRightButtonDownAsync(TokenEventArgs e)
        {
            UpdateSelection(e.TokenDisplay, e.SelectedTokens, false);
            await NoteManager.SetCurrentNoteIds(SelectedTokens.NoteIds);
            NoteControlVisibility = SelectedTokens.Any(t => t.HasNote) ? Visibility.Visible : Visibility.Collapsed;
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) right-clicked";
        }

        public void TokenMouseEnter(object sender, TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) hovered";
        }

        public void TokenMouseLeave(object sender, TokenEventArgs e)
        {
            Message = string.Empty;
        }

        public void TranslationMouseEnter(object sender, TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token {e.Translation.SourceToken.TokenId} hovered";
        }

        public void TranslationMouseLeave(object sender, TranslationEventArgs e)
        {
            Message = string.Empty;
        }

        public void NoteCreate(object sender, NoteEventArgs e)
        {
            NoteControlVisibility = Visibility.Visible;
        }

        //        public void ParatextSend(object sender, ParatextEventArgs e)
        //        {
        //            NoteManager.ParatextSend(e);
        //;        }

        public void FilterPins(object sender, NoteEventArgs e)
        {
            //WORKS
            EventAggregator.PublishOnUIThreadAsync(new FilterPinsMessage(e.TokenDisplayViewModel.SurfaceText));
        }

        public void TranslateQuick(object sender, NoteEventArgs e)
        {
            try
            {
                var surfaceText = e.SelectedTokens.CombinedSurfaceText.Replace(',', ' ');
                var result = TranslateText(surfaceText);
                Message = $"Quick Translation: '{result}'";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Quick translation failed.");
            }
        }

        public string TranslateText(string input)
        {
            string url = string.Format
            ("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}", "auto", "en", Uri.EscapeUriString(input));
            HttpClient httpClient = new HttpClient();
            string result = httpClient.GetStringAsync(url).Result;

            var items = result.Split("\"");
            var translation = items[1];
            return translation;
        }

        #endregion

        #region NoteControl

        public void NoteAdded(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteAddedAsync(e).GetAwaiter());
        }

        public async Task NoteAddedAsync(NoteEventArgs e)
        {
            OnUIThread(async () =>
            {
                await NoteManager.AddNoteAsync(e.Note, e.EntityIds);
                NotifyOfPropertyChange(() => Items);
            });

            Message = $"Note '{e.Note.Text}' added to tokens {string.Join(", ", e.EntityIds.Select(id => id.ToString()))}";
        }

        public void NoteUpdated(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteUpdatedAsync(e).GetAwaiter());
        }

        public async Task NoteUpdatedAsync(NoteEventArgs e)
        {
            await NoteManager.UpdateNoteAsync(e.Note);
            Message = $"Note '{e.Note.Text}' updated on tokens {string.Join(", ", e.EntityIds.Select(id => id.ToString()))}";
        }

        public void NoteSendToParatext(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteSendToParatextAsync(e).GetAwaiter());
        }

        public async Task NoteSendToParatextAsync(NoteEventArgs e)
        {
            try
            {
                await NoteManager.SendToParatextAsync(e.Note);
                Message = $"Note '{e.Note.Text}' sent to Paratext.";
            }
            catch (Exception ex)
            {
                Message = $"Could not send note to Paratext: {ex.Message}";
            }
        }

        public void NoteDeleted(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteDeletedAsync(e).GetAwaiter());
        }

        public async Task NoteDeletedAsync(NoteEventArgs e)
        {
            if (e.Note.NoteId != null)
            {
                OnUIThread(async () =>
                {
                    await NoteManager.DeleteNoteAsync(e.Note, e.EntityIds);
                    NotifyOfPropertyChange(() => Items);
                });
            }
            Message = $"Note '{e.Note.Text}' deleted from tokens ({string.Join(", ", e.EntityIds.Select(id => id.ToString()))})";
        }

        public void NoteEditorMouseEnter(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteEditorMouseEnterAsync(e).GetAwaiter());
        }

        public async Task NoteEditorMouseEnterAsync(NoteEventArgs e)
        {
            await NoteManager.NoteMouseEnterAsync(e.Note, e.EntityIds);
        }

        public void NoteEditorMouseLeave(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteEditorMouseLeaveAsync(e).GetAwaiter());
        }

        public async Task NoteEditorMouseLeaveAsync(NoteEventArgs e)
        {
            await NoteManager.NoteMouseLeaveAsync(e.Note, e.EntityIds);
        }

        public void LabelAdded(object sender, LabelEventArgs e)
        {
            //WORKS
            Task.Run(() => LabelAddedAsync(e).GetAwaiter());
        }

        public async Task LabelAddedAsync(LabelEventArgs e)
        {
            //WORKS
            if (SelectedVerseDisplayViewModel is null)
            {
                return;
            }

            // If this is a new note, we'll handle the labels when the note is added.
            if (e.Note.NoteId != null)
            {
                await NoteManager.CreateAssociateNoteLabelAsync(e.Note, e.Label.Text);
            }
            Message = $"Label '{e.Label.Text}' added for note";
        }


        public void LabelSelected(object sender, LabelEventArgs e)
        {
            Task.Run(() => LabelSelectedAsync(e).GetAwaiter());
        }

        public async Task LabelSelectedAsync(LabelEventArgs e)
        {
            if (e.Note.NoteId != null)
            {
                await NoteManager.AssociateNoteLabelAsync(e.Note, e.Label);
            }
            Message = $"Label '{e.Label.Text}' selected for note";
        }

        public void LabelRemoved(object sender, LabelEventArgs e)
        {
            //WORKS
            Task.Run(() => LabelRemovedAsync(e).GetAwaiter());
        }

        public async Task LabelRemovedAsync(LabelEventArgs e)
        {
            //WORKS
            if (e.Note.NoteId != null)
            {
                await NoteManager.DetachNoteLabel(e.Note, e.Label);
            }
            Message = $"Label '{e.Label.Text}' removed for note";
        }

        public void CloseNotePaneRequested(object sender, RoutedEventArgs args)
        {
            //WORKS
            NoteControlVisibility = Visibility.Collapsed;
        }

        #endregion

        // ReSharper restore UnusedMember.Global

        #endregion

        #region VerseControlMethods

        private Visibility _noteControlVisibility = Visibility.Collapsed;
        public Visibility NoteControlVisibility
        {
            get => _noteControlVisibility;
            set => Set(ref _noteControlVisibility, value);
        }

        #endregion

    }
}
