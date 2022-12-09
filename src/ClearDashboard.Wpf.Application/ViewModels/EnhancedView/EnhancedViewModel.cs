using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Tokenization;
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
using FontFamily = System.Windows.Media.FontFamily;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;
using Uri = System.Uri;

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
       
        private CancellationTokenSource? _cancellationTokenSource;
        private bool? _handleAsyncRunning;
        private string? _tokenizationType;
        private string? _message;
        private BookInfo? _currentBook;

        private string CurrentBookDisplay => string.IsNullOrEmpty(CurrentBook?.Code) ? string.Empty : $"<{CurrentBook.Code}>";
        private IEnumerable<VerseAwareEnhancedViewItemViewModel> VerseAwareEnhancedViewItemViewModels => Items.Where(item => item.GetType() == typeof(VerseAwareEnhancedViewItemViewModel)).Cast<VerseAwareEnhancedViewItemViewModel>();



        #endregion //Member Variables

        #region Public Properties

        public EnhancedViewLayout? EnhancedViewLayout
        {
            get => _enhancedViewLayout;
            set => Set(ref _enhancedViewLayout, value);
        }

        public bool IsRtl { get; set; }

        public List<string> WorkingJobs { get; set; } = new();

        public NoteManager NoteManager { get; set; }

        private MainViewModel MainViewModel => (MainViewModel)Parent;

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

                var set = Set(ref _paratextSync, value);
                if (set)
                {
                    EnhancedViewLayout!.ParatextSync = _paratextSync;
                }
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
            set
            {
                var set = Set(ref _currentBcv, value);
                if (set)
                {
                    EnhancedViewLayout!.BBBCCCVVV = _currentBcv.BBBCCCVVV;
                }
            }
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
                    EnhancedViewLayout!.VerseOffset = value;
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

        public new string? Title
        {
            get => base.Title;
            set  
            {
                base.Title = value;
                if (EnhancedViewLayout != null)
                {
                    EnhancedViewLayout.Title = value;
                }
               
                NotifyOfPropertyChange(Title);

            }
        }

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
            ILifetimeScope? lifetimeScope) :
            base(navigationService: navigationService, logger: logger, projectManager: projectManager,
                eventAggregator: eventAggregator, mediator: mediator, lifetimeScope: lifetimeScope)
#pragma warning restore CS8618
        {

            _logger = logger;
            _projectManager = projectManager;
            NoteManager = noteManager;
          

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

        public async Task Initialize(EnhancedViewLayout enhancedViewLayout)
        {
            EnhancedViewLayout = enhancedViewLayout;

            Title = enhancedViewLayout.Title;
            VerseOffsetRange = enhancedViewLayout.VerseOffset;
            BcvDictionary = ProjectManager!.CurrentParatextProject.BcvDictionary;
            ParatextSync = enhancedViewLayout.ParatextSync;
            CurrentBcv.SetVerseFromId(enhancedViewLayout.BBBCCCVVV);

            await Task.CompletedTask;
        }

        public async Task AddItem(EnhancedViewItemMetadatum item, CancellationToken cancellationToken)
        {
            EnhancedViewLayout!.EnhancedViewItems.Add(item);
            await ActivateNewVerseAwareViewItem(item, cancellationToken);
        }

        public async Task LoadData(CancellationToken token)
        {
            await Parallel.ForEachAsync(EnhancedViewLayout!.EnhancedViewItems, new ParallelOptions(), async (enhancedViewItemMetadatum, cancellationToken) =>
            {
                //_ = await Task.Factory.StartNew(async () =>
                //{
                    await ActivateNewVerseAwareViewItem(enhancedViewItemMetadatum, cancellationToken);
                //}, cancellationToken);
            });
            
        }

        private async Task ActivateNewVerseAwareViewItem(EnhancedViewItemMetadatum enhancedViewItemMetadatum, CancellationToken cancellationToken)
        {
            await Execute.OnUIThreadAsync(async () =>
            {
                var verseAwareEnhancedViewItemViewModel =
                    await ActivateItemAsync<VerseAwareEnhancedViewItemViewModel>(cancellationToken);
                await verseAwareEnhancedViewItemViewModel!.GetData(enhancedViewItemMetadatum, cancellationToken);
            });
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

            await ReloadData();

            sw.Stop();
            _logger.LogInformation("VerseChangeRerender took {0} ms", sw.ElapsedMilliseconds);
        }

        private async Task ReloadData()
        {
            await Parallel.ForEachAsync(VerseAwareEnhancedViewItemViewModels, new ParallelOptions(), async (viewModel, cancellationToken) =>
            {
                await Execute.OnUIThreadAsync(async () =>
                {
                    await viewModel.RefreshData(cancellationToken);
                });

            });
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
            EnhancedViewLayout!.EnhancedViewItems.Move(index, index - 1);
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
            MoveableItems.Move(index, index + 1);
            EnhancedViewLayout!.EnhancedViewItems.Move(index, index + 1);

        }

        private void DeleteCorpusRow(object obj)
        {
            var item = (EnhancedViewItemViewModel)obj;
            
            var index = Items.Select((element, index) => new { element, index })
                .FirstOrDefault(x => x.element.Equals(item))?.index ?? -1;

            Items.RemoveAt(index);
            EnhancedViewLayout!.EnhancedViewItems.RemoveAt(index);

        }

        public void InnerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is VerseDisplayViewModel verseDisplayViewModel)
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

        //public async Task ShowCorpusTokens(AddTokenizedCorpusToEnhancedViewMessage message, CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("Received TokenizedTextCorpusMessage.");
        //    _handleAsyncRunning = true;
        //    _cancellationTokenSource = new CancellationTokenSource();
        //    var localCancellationToken = _cancellationTokenSource.Token;

        //    EnhancedViewLayout!.EnhancedViewItems.Add(message.Metadatum);
            
        //    ProgressBarVisibility = Visibility.Visible;

        //    await ShowCorpusText(message, cancellationToken, localCancellationToken);
        //}

        public static ParatextProjectMetadata HebrewManuscriptMetadata => new ParatextProjectMetadata
        {
            Id = ManuscriptIds.HebrewManuscriptId,
            CorpusType = CorpusType.ManuscriptHebrew,
            Name = "Macula Hebrew",
            AvailableBooks = BookInfo.GenerateScriptureBookList(),
        };

        public static ParatextProjectMetadata GreekManuscriptMetadata => new ParatextProjectMetadata
        {
            Id = ManuscriptIds.GreekManuscriptId,
            CorpusType = CorpusType.ManuscriptGreek,
            Name = "Macula Greek",
            AvailableBooks = BookInfo.GenerateScriptureBookList(),
        };


        //public async Task<DAL.Alignment.Translation.TranslationSet> GetTranslationSet(string translationSetId)
        //{
        //    return await DAL.Alignment.Translation.TranslationSet.Get(new TranslationSetId(Guid.Parse(translationSetId)), Mediator!);
        //}
        //public static async Task<DAL.Alignment.Translation.AlignmentSet> GetAlignmentSet(string alignmentSetId, IMediator mediator)
        //{
        //    return await DAL.Alignment.Translation.AlignmentSet.Get(new AlignmentSetId(Guid.Parse(alignmentSetId)), mediator);
        //}
        //private IEnumerable<(EngineToken token, string paddingBefore, string paddingAfter)>? GetTokens(List<TokensTextRow> textRows, int bbbcccvvv)
        //{
        //    var textRow = textRows.FirstOrDefault(row => ((VerseRef)row.Ref).BBBCCCVVV == bbbcccvvv);
        //    if (textRow != null)
        //    {
        //        var detokenizer = new EngineStringDetokenizer(new LatinWordDetokenizer());
        //        return detokenizer.Detokenize(textRow.Tokens);
        //    }

        //    return null;
        //}

        private async Task UpdateVersesDisplay(AddTokenizedCorpusToEnhancedViewMessage message, BindableCollection<VerseDisplayViewModel> verses, string title, bool showTranslations)
        {
            var fontFamily = MainViewModel.GetFontFamilyFromParatextProjectId(message.Metadatum.ParatextProjectId);

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


            var row = VerseAwareEnhancedViewItemViewModels.FirstOrDefault(v => v.CorpusId == message.Metadatum.CorpusId);
            if (row is null)
            {
                var viewModel = await ActivateItemAsync<VerseAwareEnhancedViewItemViewModel>();
                viewModel.CorpusId = message.Metadatum.CorpusId.Value;
                viewModel.BorderColor = brush;
                viewModel.ShowTranslation = showTranslations;
                viewModel.Title = title;
                viewModel.Verses = verses;
                viewModel.IsRtl = message.Metadatum.IsRtl.Value;
                viewModel.SourceFontFamily = family;


                // add to the grouping for saving
                EnhancedViewLayout!.EnhancedViewItems.Add(message.Metadatum);
            }
            else
            {
                row.CorpusId = message.Metadatum.CorpusId.Value;
                row.BorderColor = brush;
                row.ShowTranslation = showTranslations;
                row.Title = title;
                row.Verses = verses;
                row.IsRtl = message.Metadatum.IsRtl.Value;
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
            switch (message.Metadatum.CorpusType)
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

        //public async Task ShowParallelTranslationTokens(AddAlignmentToEnhancedViewMessage message, CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("Received TokenizedTextCorpusMessage.");
        //    _handleAsyncRunning = true;
        //    _cancellationTokenSource = new CancellationTokenSource();
        //    var localCancellationToken = _cancellationTokenSource.Token;
        //    ProgressBarVisibility = Visibility.Visible;

        //    await ShowParallelTranslation(message, cancellationToken, localCancellationToken);
        //}

        //public async Task ShowParallelTranslation(AddAlignmentToEnhancedViewMessage message,
        //    CancellationToken cancellationToken, CancellationToken localCancellationToken)
        //{

        //    WorkingJobs.Add(message.TranslationSetId);

        //    var msg = _parallelMessages.Where(p =>
        //        p.TranslationSetId == message.TranslationSetId && p.AlignmentSetId == message.AlignmentSetId && p.ParallelCorpusId == message.ParallelCorpusId).ToList();
        //    if (msg.Count == 0)
        //    {
        //        _parallelMessages.Add(message);
        //    }

        //    // current project
        //    _ = await Task.Factory.StartNew(async () =>
        //    {
        //        try
        //        {
        //            ProgressBarVisibility = Visibility.Visible;

        //            var verseTokens = await BuildTokenDisplayViewModels(message);
        //            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
        //                new BackgroundTaskStatus
        //                {
        //                    Name = "Fetch Book",
        //                    Description = $"Found {verseTokens.Count} TokensTextRow entities.",
        //                    StartTime = DateTime.Now,
        //                    TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed
        //                }), cancellationToken);
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.LogError(ex, "The attempt to ShowNewParallelTranslation failed.");
        //            if (!localCancellationToken.IsCancellationRequested)
        //            {
        //                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
        //                    new BackgroundTaskStatus
        //                    {
        //                        Name = "Fetch Book",
        //                        EndTime = DateTime.Now,
        //                        ErrorMessage = $"{ex}",
        //                        TaskLongRunningProcessStatus = LongRunningTaskStatus.Failed
        //                    }), cancellationToken);
        //            }
        //        }
        //        finally
        //        {
        //            _handleAsyncRunning = false;
        //            if (_cancellationTokenSource != null)
        //                _cancellationTokenSource.Dispose();

        //            // remove from the job stack
        //            for (int i = WorkingJobs.Count - 1; i >= 0; i--)
        //            {
        //                if (WorkingJobs[i] == message.TranslationSetId)
        //                {
        //                    WorkingJobs.RemoveAt(i);
        //                }
        //            }

        //            if (WorkingJobs.Count == 0)
        //            {
        //                ProgressBarVisibility = Visibility.Collapsed;
        //            }
        //        }
        //    }, cancellationToken);
        //}

      

      

      

    

        //private async Task UpdateParallelCorpusDisplay(AddAlignmentToEnhancedViewMessage message,
        //    BindableCollection<VerseDisplayViewModel> verses, string title, bool showTranslations = true)
        //{
        //    // same color as defined in SharedVisualTemplates.xaml
        //    Brush brush = Brushes.SaddleBrown;

        //    // get the font family for this project
        //    var sourceFontFamily = MainViewModel.GetFontFamilyFromParatextProjectId(message.SourceParatextId);
        //    var targetFontFamily = MainViewModel.GetFontFamilyFromParatextProjectId(message.TargetParatextId);

        //    FontFamily fontFamilySource;
        //    try
        //    {
        //        fontFamilySource = new(sourceFontFamily);
        //    }
        //    catch (Exception e)
        //    {
        //        fontFamilySource = new("Segoe UI");
        //    }

        //    FontFamily fontFamilyTarget;
        //    try
        //    {
        //        fontFamilyTarget = new(targetFontFamily);
        //    }
        //    catch (Exception e)
        //    {
        //        fontFamilyTarget = new("Segoe UI");
        //    }



        //    VerseAwareEnhancedViewItemViewModel? enhancedViewItemViewModel;
        //    if (message.AlignmentSetId is null)
        //    {
        //        // interlinear
        //        enhancedViewItemViewModel = VerseAwareEnhancedViewItemViewModels.FirstOrDefault(v =>
        //             v.CorpusId == Guid.Parse(message.ParallelCorpusId) &&
        //             v.TranslationSetId == Guid.Parse(message.TranslationSetId));

        //    }
        //    else
        //    {
        //        // alignment
        //        enhancedViewItemViewModel = VerseAwareEnhancedViewItemViewModels.FirstOrDefault(v =>
        //             v.CorpusId == Guid.Parse(message.ParallelCorpusId) &&
        //             v.AlignmentSetId == Guid.Parse(message.AlignmentSetId));

        //    }


        //    if (enhancedViewItemViewModel is null)
        //    {
        //        var alignmentSetId = Guid.Empty;
        //        if (message.AlignmentSetId is not null)
        //        {
        //            alignmentSetId = Guid.Parse(message.AlignmentSetId);
        //            brush = Brushes.DarkGreen;
        //        }

        //        var translationSetId = Guid.Empty;
        //        if (message.TranslationSetId is not null)
        //        {
        //            translationSetId = Guid.Parse(message.TranslationSetId);
        //        }

        //        var viewModel = await ActivateItemAsync<VerseAwareEnhancedViewItemViewModel>();

        //        viewModel.AlignmentSetId = alignmentSetId;
        //        viewModel.ParallelCorpusId = Guid.Parse(message.ParallelCorpusId);
        //        viewModel.TranslationSetId = translationSetId;
        //        viewModel.CorpusId = Guid.Parse(message.ParallelCorpusId);
        //        viewModel.BorderColor = brush;
        //        viewModel.ShowTranslation = showTranslations;
        //        viewModel.Title = title;
        //        viewModel.Verses = verses;
        //        viewModel.IsRtl = message.IsRTL.Value;
        //        viewModel.IsTargetRtl = message.IsTargetRTL ?? false;
        //        viewModel.SourceFontFamily = fontFamilySource;
        //        viewModel.TargetFontFamily = fontFamilyTarget;
        //        viewModel.TranslationFontFamily = fontFamilyTarget;


        //        if (message.AlignmentSetId is not null)
        //        {
        //            EnhancedViewLayout.EnhancedViewItems.Add(new AlignmentEnhancedViewItemMetadatum
        //            {
        //                AlignmentSetId = message.AlignmentSetId,
        //                DisplayName = message.DisplayName,
        //                IsNewWindow = message.IsNewWindow,
        //                IsRtl = message.IsRTL,
        //                IsTargetRtl = message.IsTargetRTL,
        //                ParallelCorpusDisplayName = message.ParallelCorpusDisplayName,
        //                ParallelCorpusId = message.ParallelCorpusId,
        //                SourceParatextId = message.SourceParatextId,
        //                TargetParatextId = message.TargetParatextId

        //            });
        //        }

        //        if (message.TranslationSetId is not null)
        //        {
        //            EnhancedViewLayout.EnhancedViewItems.Add(new InterlinearEnhancedViewItemMetadatum
        //            {
        //                TranslationSetId = message.TranslationSetId,
        //                DisplayName = message.DisplayName,
        //                IsNewWindow = message.IsNewWindow,
        //                IsRtl = message.IsRTL,
        //                IsTargetRtl = message.IsTargetRTL,
        //                ParallelCorpusDisplayName = message.ParallelCorpusDisplayName,
        //                ParallelCorpusId = message.ParallelCorpusId,
        //                SourceParatextId = message.SourceParatextId,
        //                TargetParatextId = message.TargetParatextId

        //            });
        //        }
              
        //    }
        //    else
        //    {
        //        enhancedViewItemViewModel.Title = title;
        //        enhancedViewItemViewModel.Verses = verses;
        //        enhancedViewItemViewModel.BorderColor = brush;
        //    }

        //    NotifyOfPropertyChange(() => Items);
        //}

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
        private EnhancedViewLayout? _enhancedViewLayout;

        public Visibility NoteControlVisibility
        {
            get => _noteControlVisibility;
            set => Set(ref _noteControlVisibility, value);
        }

        #endregion

      
    }
}
