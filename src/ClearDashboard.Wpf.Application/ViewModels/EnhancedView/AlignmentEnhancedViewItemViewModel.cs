using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Features.MarbleDataRequests;
using ClearDashboard.Wpf.Application.Converters;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.Threading;
using ClearDashboard.Wpf.Application.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.ViewModels;
using Alignment = ClearDashboard.DAL.Alignment.Translation.Alignment;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using Token = ClearBible.Engine.Corpora.Token;
using ClearBible.Engine.Exceptions;
using SIL.Scripture;
using SIL.Machine.Corpora;


// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming
namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class AlignmentEnhancedViewItemViewModel : VerseAwareEnhancedViewItemViewModel, IHandle<UiLanguageChangedMessage>
    {

       
        public AlignmentEnhancedViewItemViewModel(DashboardProjectManager? projectManager,
                                                  IEnhancedViewManager enhancedViewManager,
                                                  INavigationService? navigationService,
                                                  ILogger<VerseAwareEnhancedViewItemViewModel>? logger,
                                                  IEventAggregator? eventAggregator,
                                                  IMediator? mediator,
                                                  ILifetimeScope? lifetimeScope,
                                                  IWindowManager windowManager,
                                                  ILocalizationService localizationService,
                                                  NoteManager noteManager,
                                                  EditMode editMode = EditMode.MainViewOnly)
            : base(
                  projectManager, 
                  enhancedViewManager, 
                  navigationService, 
                  logger, 
                  eventAggregator, 
                  mediator, 
                  lifetimeScope, 
                  windowManager, 
                  localizationService, 
                  noteManager, 
                  editMode)
        {
            ShowEditButton = EditMode == EditMode.ManualToggle;
            EnableEditMode = false;
            EditModeButtonLabel = LocalizationService.Get("BulkAlignmentReview_BulkAlignmentReview");
            SourceToTarget = true;

            SelectedFontFamily = SourceFontFamily;
            SelectedRtl = IsRtl;

            AlignmentTypes = AlignmentTypes.Assigned_Invalid |
                             AlignmentTypes.Assigned_Unverified |
                             AlignmentTypes.Assigned_Verified |
                             AlignmentTypes.FromAlignmentModel_Unverified_Not_Otherwise_Included;

            // TODO:  this should be a user setting.
            CountsByTrainingText = true;
            AlignmentFetchOptions = AlignmentFetchOptions.ByBook;
            ShowBookSelector = true;
            RelevantBooks = new List<Book>();

            CreateAlignmentTypesMap();

        }


        protected override string CreateParallelCorpusItemTitle(ParallelCorpusEnhancedViewItemMetadatum metadatum,
            string localizationKey, int rowCount)
        {
            if (EditMode == EditMode.EditorViewOnly)
            {

                return $"{metadatum.ParallelCorpusDisplayName ?? string.Empty} {LocalizationService.Get("BulkAlignmentReview_BulkAlignmentEditor")}";
            }
            return base.CreateParallelCorpusItemTitle(metadatum, localizationKey, rowCount);
       
        }

        protected override string CreateNoVerseDataTitle(ParallelCorpusEnhancedViewItemMetadatum metadatum)
        {
            if (EditMode == EditMode.EditorViewOnly)
            {
                return $"{metadatum.ParallelCorpusDisplayName} {LocalizationService.Get("BulkAlignmentReview_BulkAlignmentEditor")}";
            }
            
            return base.CreateNoVerseDataTitle(metadatum);
        }

        public override void CreateTitle(TokenizedCorpusEnhancedViewItemMetadatum metadatum, IReadOnlyList<TokensTextRow>? tokensTextRowsRange,
            BookChapterVerseViewModel? currentBcv, bool versesInRange = true)
        {
            if (EditMode != EditMode.MainViewOnly)
            {
                Title = $"{CreateBaseTitle(metadatum)} Bulk Alignment Editor";
            }
            else
            {
                base.CreateTitle(metadatum, tokensTextRowsRange, currentBcv, versesInRange);
            }
            
        }

        public FontFamily? SelectedFontFamily
        {
            get => _selectedFontFamily;
            set => Set(ref _selectedFontFamily, value);
        }

        public bool SelectedRtl
        {
            get => _selectedRtl;
            set => Set(ref _selectedRtl, value);
        }

        public EnhancedViewModel EnhancedViewModel => (EnhancedViewModel)Parent;

        private readonly DebounceDispatcher _debounceTimer = new();

        public bool HasPivotWords => PivotWords is { Count: > 0 };

        private AlignedWord? CurrentAlignedWord { get; set; }

        private AlignmentFetchOptions _alignmentFetchOptions;
        public AlignmentFetchOptions AlignmentFetchOptions
        {
            get => _alignmentFetchOptions;
            set => Set(ref _alignmentFetchOptions, value);
        }

        public string PivotWordsCountMessage => $"{(PivotWords is { Count: > 0 } ? PivotWords.Count : 0)} records.";

        private BindableCollection<PivotWord>? _pivotWords;
        public BindableCollection<PivotWord>? PivotWords
        {
            get => _pivotWords;
            set
            {
                Set(ref _pivotWords, value);
                NotifyOfPropertyChange(nameof(HasPivotWords));
                NotifyOfPropertyChange(nameof(PivotWordsCountMessage));
            }
        }

        public bool HasAlignedWords => AlignedWords is { Count: > 0 };

        public bool ShowBookSelector
        {
            get => _showBookSelector;
            set => Set(ref _showBookSelector, value);
        }

        public string AlignedWordsCountMessage => $"{(AlignedWords is { Count: > 0 } ? AlignedWords.Count : 0)} records.";
        private BindableCollection<AlignedWord>? _alignedWords;

        public BindableCollection<AlignedWord>? AlignedWords
        {
            get => _alignedWords;
            set
            {
                Set(ref _alignedWords, value);
                NotifyOfPropertyChange(nameof(HasAlignedWords));
                NotifyOfPropertyChange(nameof(AlignedWordsCountMessage));
            }
        }

        public bool HasBulkAlignments => BulkAlignments is { Count: > 0 };

        public string BulkAlignmentsCountMessage => $"{(BulkAlignments is { Count: > 0 } ? BulkAlignments.Count : 0)} records.";

        public PagingCollectionView? PagedBulkAlignments
        {
            get => _pagedBulkAlignments;
            set => Set(ref _pagedBulkAlignments, value);
        }

        private List<BulkAlignmentVerseRow>? _bulkAlignments;
        public List<BulkAlignmentVerseRow>? BulkAlignments
        {
            get => _bulkAlignments;
            set
            {
                Set(ref _bulkAlignments, value);
                NotifyOfPropertyChange(nameof(HasBulkAlignments));
                NotifyOfPropertyChange(nameof(BulkAlignmentsCountMessage));
            }
        }

        public bool SourceToTarget
        {
            get => _sourceToTarget;
            private set => Set(ref _sourceToTarget, value);
        }

        public override async Task GetData(CancellationToken cancellationToken)
        {
            //await base.GetData(cancellationToken);
            if (EditMode == EditMode.EditorViewOnly)
            {

                await EnsureAlignmentSet();
                var getEditorDataTask = GetEditorData(cancellationToken);
                var setFontsAndRtlTask = SetFontsAndRtl();
                var getDataTask = base.GetData(cancellationToken);

                await Task.WhenAll(getDataTask, getEditorDataTask, setFontsAndRtlTask);

             
            }
            else
            {
                await base.GetData(cancellationToken);
            }
          
        }

        private async Task SetFontsAndRtl()
        {
            switch (EnhancedViewItemMetadatum)
            {
                case AlignmentEnhancedViewItemMetadatum alignmentEnhancedViewItemMetadatum:

                    await Execute.OnUIThreadAsync(async () =>
                    {
                        SourceFontFamily = await GetFontFamily(alignmentEnhancedViewItemMetadatum.SourceParatextId!);
                        TargetFontFamily = await GetFontFamily(alignmentEnhancedViewItemMetadatum.TargetParatextId!);
                        IsRtl = alignmentEnhancedViewItemMetadatum.IsRtl ?? false;
                        IsTargetRtl = alignmentEnhancedViewItemMetadatum.IsTargetRtl ?? false;
                        SelectedFontFamily = SourceFontFamily;
                        SelectedRtl = IsRtl;
                    });
                    break;
            }
        }

        protected override async Task GetEditorData(CancellationToken cancellationToken)
        {
         
            await GetPivotWords(cancellationToken);

        }

        public void OnToggleAlignmentsChecked(CheckBox? checkBox)
        {
            if (checkBox != null && BulkAlignments is { Count: > 0 })
            {
                foreach (var bulkAlignment in BulkAlignments)
                {
                    bulkAlignment.IsSelected = checkBox.IsChecked ?? false;
                }
            }
        }

        public async void OnSyncButtonClicked(BulkAlignmentVerseRow row)
        {
            await EventAggregator.PublishOnUIThreadAsync(new VerseChangedMessage(row.BBBCCCVVV, true));
        }

        public void OnPivotWordSourceChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem item)
            {
                SourceToTarget = (item.Tag as string) == BulkAlignmentReviewTags.Source;
                SelectedFontFamily = SourceToTarget ? SourceFontFamily : TargetFontFamily;
                SelectedRtl = SourceToTarget ? IsRtl : IsTargetRtl;
                _debounceTimer.DebounceAsync(1000, async () => await GetPivotWords(CancellationToken.None));
            }
        }

        public bool CountsByTrainingText
        {
            get => _countsByTrainingText;
            private set => Set(ref _countsByTrainingText, value);
        }

        public void OnCountByTextTypeChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem item)
            {
                CountsByTrainingText = (item.Tag as string) == BulkAlignmentReviewTags.CountsByTrainingText;
                _debounceTimer.DebounceAsync(1000, async () => await GetPivotWords(CancellationToken.None));
            }
        }


        public void OnAlignedWordOptionsChanged(SelectionChangedEventArgs e)
        {
            _debounceTimer.DebounceAsync(1000, async () => await GetPivotWords(CancellationToken.None));
        }

        public void OnNextPageClicked()
        {
            Execute.OnUIThread(() => PagedBulkAlignments.MoveToNextPage());
        }

        public void OnPreviousPageClicked()
        {
            Execute.OnUIThread(() => PagedBulkAlignments.MoveToPreviousPage());
           
        }

        public async void OnBookChanged(SelectionChangedEventArgs e)
        {
            if (CurrentAlignedWord != null && e.AddedItems.Count > 0 && e.AddedItems[0] is Book item)
            {
                await GetBulkAlignments();
            }
        }

        private async Task GetBulkAlignments()
        {
            _ = await Task.Factory.StartNew(async () =>
            {
                BulkAlignments = null;
              
                PagedBulkAlignments = new PagingCollectionView(new List<BulkAlignmentVerseRow>(), 0);
                try
                {
                    FetchingData = true;
                    ProgressBarVisibility = Visibility.Visible;

                    var result = AlignmentFetchOptions == AlignmentFetchOptions.ByBook
                        ? await AlignmentSet.GetAlignmentVerseContexts(CurrentAlignedWord.Source,
                            CurrentAlignedWord.Target, CountsByTrainingText, CurrentBook.Number, AlignmentTypes)
                        : await AlignmentSet.GetAlignmentVerseContexts(CurrentAlignedWord.Source,
                            CurrentAlignedWord.Target, CountsByTrainingText, null, AlignmentTypes);


                    _verseContexts = result.VerseContexts;

                    //await Execute.OnUIThreadAsync(async () =>
                    //{

                    var verseRows = AlignmentFetchOptions == AlignmentFetchOptions.ByBook
                        ? await _verseContexts
                            .Where(vc => vc.alignment.AlignmentId.SourceTokenId.BookNumber == CurrentBook.Number).SelectAsync(
                                async verseContext =>
                                    await CreateBulkAlignmentVerseRow(verseContext))
                        : await _verseContexts.SelectAsync(async verseContext =>
                            await CreateBulkAlignmentVerseRow(verseContext));

                    BulkAlignments = new List<BulkAlignmentVerseRow>(verseRows.OrderBy(vr => vr.SourceRef));

                    PagedBulkAlignments = new PagingCollectionView(BulkAlignments, BulkAlignments.Count);
                    //});
                }
                finally
                {
                    FetchingData = false;
                    ProgressBarVisibility = Visibility.Hidden;
                }
            });
        }

        private async Task<BulkAlignmentVerseRow> CreateBulkAlignmentVerseRow((Alignment alignment, IEnumerable<Token> sourceVerseTokens, uint sourceVerseTokensIndex, IEnumerable<Token> targetVerseTokens, uint targetVerseTokensIndex) verseContext)
        {
            return new BulkAlignmentVerseRow
            {

                Alignment = verseContext.alignment,
                //IsSourceRtl = verseContext.alignment.AlignmentId.SourceTokenId.
                IsSelected = false,
                Type = AlignmentTypesMap[verseContext.alignment.ToAlignmentType(AlignmentTypes)],
                BulkAlignmentDisplayViewModel = await BulkAlignmentDisplayViewModel.CreateAsync(
                    LifetimeScope,
                    new BulkAlignment
                    {
                        Alignment = verseContext.alignment,
                        SourceVerseTokens = verseContext.sourceVerseTokens,
                        SourceVerseTokensIndex = verseContext.sourceVerseTokensIndex,
                        TargetVerseTokens = verseContext.targetVerseTokens,
                        TargetVerseTokensIndex = verseContext.targetVerseTokensIndex
                    },
                    ParallelCorpus
                )
            };
        }

        public void OnAlignmentApprovalChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem item)
            {
                var approvalType = (item.Tag as string);
                _debounceTimer.DebounceAsync(10, async () => await UpdateAlignmentStatuses(approvalType));

            }
        }

        //OnAlignmentFetchOptionsChanged
        public void OnAlignmentFetchOptionsChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem item)
            {
                AlignmentFetchOptions = (item.Tag is AlignmentFetchOptions options ? options : AlignmentFetchOptions.ByBook);
                ShowBookSelector = AlignmentFetchOptions == AlignmentFetchOptions.ByBook;
                _debounceTimer.DebounceAsync(10, async () => await GetBulkAlignments());

            }
        }

        private async Task UpdateAlignmentStatuses(string? approvalType)
        {
            _ = await Task.Factory.StartNew(async () =>
            {
                var taskName = "UpdateAlignmentStatuses";
                var cancellationToken = new CancellationTokenSource().Token;
                try
                {

                    FetchingData = true;
                    ProgressBarVisibility = Visibility.Visible;

                    await EnsureAlignmentSet();

                    if (BulkAlignments != null)
                    {

                        var alignmentsToUpdate = BulkAlignments.Where(ba => ba.IsSelected).Select(ba => ba.Alignment)
                            .ToList();
                        var alignmentsToSave = new List<Alignment>();
                        if (alignmentsToUpdate.Any())
                        {
                            var verification = approvalType.DetermineAlignmentVerificationStatus();
                            alignmentsToSave.AddRange(alignmentsToUpdate.Select(alignment =>
                                new Alignment(alignment.AlignedTokenPair, verification)));
                            await AlignmentSet.PutAlignments(alignmentsToSave, CancellationToken.None);
                        }
                    }
                }
                finally
                {
                    FetchingData = false;
                    ProgressBarVisibility = Visibility.Hidden;
                }
            });
        }

        private string DetermineAlignmentVerificationStatus(string? approvalType)
        {
            switch (approvalType)
            {
                case BulkAlignmentReviewTags.MarkSelectedAsValid:
                    return AlignmentVerificationStatus.Verified;
                case BulkAlignmentReviewTags.MarkSelectedAsInvalid:
                    return AlignmentVerificationStatus.Invalid;
                case BulkAlignmentReviewTags.MarkSelectedAsNeedsReview:
                    return AlignmentVerificationStatus.Unverified;
                default:
                    return AlignmentVerificationStatus.Unverified;
            }
        }

        public void OnTokenClicked(TokenEventArgs e)
        {
            var source = e;
        }


        public void OnSourceTargetCountRowSelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is AlignedWord alignedWord)
            {
                GetAlignmentVerseContexts(alignedWord);
            }
        }

        public void OnPivotWordRowSelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is PivotWord pivotWord)
            {
                if (AlignmentCounts != null && pivotWord.Word != null)
                {
                    var success = AlignmentCounts.TryGetValue(pivotWord.Word, out IDictionary<string, (IDictionary<string, uint> StatusCounts, string BookNumbers)> alignedWordDictionary);

                    if (success && alignedWordDictionary != null)
                    {
                        PagedBulkAlignments = null;
                        AlignedWords = new BindableCollection<AlignedWord>(alignedWordDictionary.Select(kvp =>
                                new AlignedWord
                                {
                                    Source = SourceToTarget ? pivotWord.Word : kvp.Key,
                                    Target = SourceToTarget ? kvp.Key : pivotWord.Word,
                                    Count = kvp.Value.StatusCounts.Sum(k => k.Value),
                                    PivotWord = pivotWord,
                                    RelevantBooks = CreateRelevantBooks(kvp.Value.BookNumbers)
                                })
                            .OrderByDescending(kvp => kvp.Count));
                    }
                }
            }
        }

        private List<Book> CreateRelevantBooks(string bookNumbers)
        {
            var numbers = bookNumbers.Split(',');
            return numbers.Select(number => Convert.ToInt32(number)).Select(index => new Book { Number = index, Code = VerseHelper.BookNames[index].code }).ToList();
        }

        private IDictionary<string, IDictionary<string, (IDictionary<string, uint> StatusCounts, string BookNumbers)>>? AlignmentCounts { get; set; }

        private AlignmentTypes _alignmentTypes;
        public AlignmentTypes AlignmentTypes
        {
            get => _alignmentTypes;
            set => Set(ref _alignmentTypes, value);
        }

        public List<Book> RelevantBooks
        {
            get => _relevantBooks;
            set => Set(ref _relevantBooks, value);
        }

        public Book CurrentBook
        {
            get => _currentBook;
            set => Set(ref _currentBook, value);
        }

        private IEnumerable<(Alignment alignment, IEnumerable<Token> sourceVerseTokens, uint sourceVerseTokensIndex, IEnumerable<Token> targetVerseTokens, uint targetVerseTokensIndex)>? _verseContexts;
        private void GetAlignmentVerseContexts(AlignedWord alignedWord)
        {
            if (alignedWord is { Source: { }, Target: { } } && AlignmentSet != null)
            {
                CurrentAlignedWord = alignedWord;
                RelevantBooks = alignedWord.RelevantBooks;
                CurrentBook = RelevantBooks[0];
            }
        }

        private AlignmentSet? AlignmentSet { get; set; }

        private async Task GetPivotWords(CancellationToken cancellationToken)
        {
            PivotWords = null;
            AlignedWords = null;
            BulkAlignments = null;
            AlignmentCounts = null;
            PagedBulkAlignments = null;

            _ = await Task.Factory.StartNew(async () =>
            {
                var taskName = "GetPivotWords";
                //var cancellationToken = new CancellationTokenSource().Token;
                try
                {

                    //await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                    //    description: $"Fetching Pivot Words", cancellationToken: cancellationToken);


                    FetchingData = true;
                    ProgressBarVisibility = Visibility.Visible;

                    await EnsureAlignmentSet();

                    if (AlignmentSet != null)
                    {
                        AlignmentCounts = await AlignmentSet.GetAlignmentCounts(SourceToTarget, CountsByTrainingText, true,
                            AlignmentTypes, cancellationToken: cancellationToken);

                        await Execute.OnUIThreadAsync(async () =>
                        {
                            PivotWords = new BindableCollection<PivotWord>(AlignmentCounts.Select(kvp =>
                                    new PivotWord { Word = kvp.Key, Count = kvp.Value.Sum(k => k.Value.StatusCounts.Sum(k2 => (int)k2.Value)) })
                                .OrderByDescending(kvp => kvp.Count));
                            await Task.CompletedTask;
                        });

                    }
                }
                catch (Exception ex)
                {
                    //await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                    //    exception: ex, cancellationToken: cancellationToken,
                    //    backgroundTaskMode: BackgroundTaskMode.PerformanceMode);
                    Logger.LogError(ex, "An unexpected error occurred while fetch PivotWords.");
                }
                finally
                {

                    FetchingData = false;
                    ProgressBarVisibility = Visibility.Hidden;
                }
            });
        }

        private ParallelCorpus ParallelCorpus { get; set; }
        private async Task EnsureAlignmentSet()
        {
            if (AlignmentSet == null && EnhancedViewItemMetadatum is AlignmentEnhancedViewItemMetadatum)
            {
                AlignmentSetId = Guid.Parse(((AlignmentEnhancedViewItemMetadatum)EnhancedViewItemMetadatum)
                    .AlignmentSetId!);
                AlignmentSet = await AlignmentSet.Get(new AlignmentSetId(AlignmentSetId), Mediator!);
                ParallelCorpus = await ParallelCorpus.GetAsync(LifetimeScope!, AlignmentSet.ParallelCorpusId);
            }
        }

        private Dictionary<AlignmentTypes, string> AlignmentTypesMap = new Dictionary<AlignmentTypes, string>();
        private List<Book> _relevantBooks;
        private Book _currentBook;
        private PagingCollectionView? _pagedBulkAlignments;
        private bool _showBookSelector;
        private FontFamily _selectedFontFamily;
        private bool _selectedRtl;
        private bool _countsByTrainingText;
        private bool _sourceToTarget;


        public async Task HandleAsync(UiLanguageChangedMessage message, CancellationToken cancellationToken)
        {
            CreateAlignmentTypesMap();
            await Task.CompletedTask;
        }

        private void CreateAlignmentTypesMap()
        {
            AlignmentTypesMap = new Dictionary<AlignmentTypes, string>
            {
                { AlignmentTypes.FromAlignmentModel_Unverified_Not_Otherwise_Included, LocalizationService.Get("BulkAlignmentReview_Machine") },
                { AlignmentTypes.Assigned_Verified, LocalizationService.Get("BulkAlignmentReview_Valid") },
                { AlignmentTypes.Assigned_Invalid, LocalizationService.Get("BulkAlignmentReview_Invalid") },
                { AlignmentTypes.Assigned_Unverified, LocalizationService.Get("BulkAlignmentReview_NeedsReview") },
            };
        }

        protected override IEnumerable<IRow> Rows
        {
            get
            {
                var verses = (EnhancedViewItemMetadatum as AlignmentEnhancedViewItemMetadatum)
                    ?.ParallelCorpus
                    ?.GetByVerseRange(
                        new VerseRef(ParentViewModel.CurrentBcv.GetBBBCCCVVV()),
                        (ushort)ParentViewModel.VerseOffsetRange,
                        (ushort)ParentViewModel.VerseOffsetRange)
                    ?? throw new InvalidDataEngineException(name: "metadata or parallelcorpus", value: "null");
                return verses.parallelTextRows;
            }
        }
        protected override List<TokenizedTextCorpusId> TokenizedTextCorpusIds
        {
            get
            {
                var metadatum = EnhancedViewItemMetadatum as AlignmentEnhancedViewItemMetadatum;
                return new List<TokenizedTextCorpusId>()
                {
                    metadatum?.ParallelCorpus?.ParallelCorpusId?.SourceTokenizedCorpusId
                        ?? throw new InvalidStateEngineException(name: "metadatum, metadatum.ParallelCorpus, or ParallelCorpusId", value: "null"),
                    metadatum.ParallelCorpus.ParallelCorpusId.TargetTokenizedCorpusId
                        ?? throw new InvalidStateEngineException(name: "metadatum, metadatum.ParallelCorpus, or ParallelCorpusId", value: "null")
                };
            }
        }
    }
}
