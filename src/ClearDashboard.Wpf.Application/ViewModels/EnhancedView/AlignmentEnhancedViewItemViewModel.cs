using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using static ClearDashboard.DataAccessLayer.Threading.BackgroundTaskStatus;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Events;
using Alignment = ClearDashboard.DAL.Alignment.Translation.Alignment;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using Token = ClearBible.Engine.Corpora.Token;
using ClearDashboard.DataAccessLayer.Features.MarbleDataRequests;
using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.Converters;
using System.Collections;
using System.ComponentModel;
using System.Windows.Data;


// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming
namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{

    public class PagingCollectionView : ListCollectionView
    {
        private readonly IList _innerList;
        private readonly int _itemsPerPage;

        private int _currentPage = 1;

        public PagingCollectionView(IList innerList, int itemsPerPage)
            : base(innerList)
        {
            _innerList = innerList;
            _itemsPerPage = itemsPerPage;
        }

        public override int Count
        {
            get
            {
                if (_innerList.Count == 0) return 0;
                if (_currentPage < PageCount) // page 1..n-1
                {
                    return _itemsPerPage;
                }
                else // page n
                {
                    var itemsLeft = _innerList.Count % _itemsPerPage;
                    return 0 == itemsLeft ? _itemsPerPage : itemsLeft;
                }
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CurrentPage"));
            }
        }

        public int ItemsPerPage => _itemsPerPage;

        public int PageCount => (_innerList.Count + _itemsPerPage - 1) / _itemsPerPage;

        private int EndIndex
        {
            get
            {
                var end = _currentPage * _itemsPerPage - 1;
                return (end > _innerList.Count) ? _innerList.Count : end;
            }
        }

        private int StartIndex => (_currentPage - 1) * _itemsPerPage;

        public override object GetItemAt(int index)
        {
            var offset = index % (_itemsPerPage);
            return _innerList[StartIndex + offset];
        }

        public void MoveToNextPage()
        {
            if (_currentPage < this.PageCount)
            {
                CurrentPage += 1;
            }
            Refresh();
        }

        public void MoveToPreviousPage()
        {
            if (_currentPage > 1)
            {
                CurrentPage -= 1;
            }
            Refresh();
        }
    }

    public static class LinqExtensions
    {
        public static async Task<TResult[]> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return await Task.WhenAll(source.Select(selector));
        }
    }
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
                                                  EditMode editMode = EditMode.MainViewOnly)
            : base(projectManager, enhancedViewManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, windowManager, localizationService, editMode)
        {
            ShowEditButton = EditMode == EditMode.ManualToggle;
            EnableEditMode = false;
            EditModeButtonLabel = LocalizationService.Get("BulkAlignmentReview_BulkAlignmentReview");
            _sourceToTarget = true;

            AlignmentTypes = AlignmentTypes.Assigned_Invalid |
                             AlignmentTypes.Assigned_Unverified |
                             AlignmentTypes.Assigned_Verified |
                             AlignmentTypes.FromAlignmentModel_Unverified_Not_Otherwise_Included;

            // TODO:  this should be a user setting.
            _countsByTrainingText = true;
            RelevantBooks = new List<Book>();

            CreateAlignmentTypesMap();

        }



        private readonly DebounceDispatcher _debounceTimer = new();

        public bool HasPivotWords => PivotWords is { Count: > 0 };

        private AlignedWord? CurrentAlignedWord { get; set; }

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

        public PagingCollectionView PagedBulkAlignments
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

        private bool _sourceToTarget;

        public override async Task GetData(CancellationToken cancellationToken)
        {
            //await base.GetData(cancellationToken);
            if (EditMode == EditMode.EditorViewOnly)
            {
                await GetEditorData();
            }
            else
            {
                await base.GetData(cancellationToken);
            }
        }

        protected override async Task GetEditorData()
        {

            await GetPivotWords();

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
                _sourceToTarget = (item.Tag as string) == BulkAlignmentReviewTags.Source;
                _debounceTimer.DebounceAsync(1000, async () => await GetPivotWords());
            }
        }

        private bool _countsByTrainingText;
        public void OnCountByTextTypeChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem item)
            {
                _countsByTrainingText = (item.Tag as string) == BulkAlignmentReviewTags.CountsByTrainingText;
                _debounceTimer.DebounceAsync(1000, async () => await GetPivotWords());
            }
        }


        public void OnAlignedWordOptionsChanged(SelectionChangedEventArgs e)
        {
            _debounceTimer.DebounceAsync(1000, async () => await GetPivotWords());
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

                _ = await Task.Factory.StartNew(async () =>
                {
                    try
                    {

                        FetchingData = true;
                        ProgressBarVisibility = Visibility.Visible;
                        _verseContexts = await _alignmentSet.GetAlignmentVerseContexts(CurrentAlignedWord.Source,
                            CurrentAlignedWord.Target, _countsByTrainingText, CurrentBook.Number, AlignmentTypes);

                        //await Execute.OnUIThreadAsync(async () =>
                        //{

                        var verseRows = await _verseContexts.Where(vc => vc.alignment.AlignmentId.SourceTokenId.BookNumber == CurrentBook.Number).SelectAsync(async verseContext =>
                            new BulkAlignmentVerseRow
                            {

                                Alignment = verseContext.alignment,
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
                                    _alignmentSet.ParallelCorpusId.SourceTokenizedCorpusId.Detokenizer,
                                    _alignmentSet.ParallelCorpusId.SourceTokenizedCorpusId.CorpusId.IsRtl,
                                    _alignmentSet.ParallelCorpusId.TargetTokenizedCorpusId.Detokenizer,
                                    _alignmentSet.ParallelCorpusId.TargetTokenizedCorpusId.CorpusId.IsRtl
                                )
                            });

                        BulkAlignments = new List<BulkAlignmentVerseRow>(verseRows.OrderBy(vr => vr.SourceRef));

                        PagedBulkAlignments = new PagingCollectionView(BulkAlignments, 5);
                        //});

                    }
                    finally
                    {
                        FetchingData = false;
                        ProgressBarVisibility = Visibility.Hidden;
                    }
                });

            }
        }

        public void OnAlignmentApprovalChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem item)
            {
                var approvalType = (item.Tag as string);
                _debounceTimer.DebounceAsync(10, async () => await UpdateAlignmentStatuses(approvalType));

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

                    if (_alignmentSet == null && EnhancedViewItemMetadatum is AlignmentEnhancedViewItemMetadatum)
                    {
                        AlignmentSetId = Guid.Parse(((AlignmentEnhancedViewItemMetadatum)EnhancedViewItemMetadatum)
                            .AlignmentSetId!);
                        _alignmentSet = await AlignmentSet.Get(new AlignmentSetId(AlignmentSetId), Mediator!);
                    }

                    var alignmentsToUpdate = BulkAlignments.Where(ba => ba.IsSelected).Select(ba => ba.Alignment).ToList();
                    var alignmentsToSave = new List<Alignment>();
                    if (alignmentsToUpdate.Any())
                    {
                        var verification = DetermineVerification(approvalType);
                        alignmentsToSave.AddRange(alignmentsToUpdate.Select(alignment => new Alignment(alignment.AlignedTokenPair, verification)));
                        await _alignmentSet.PutAlignments(alignmentsToSave, CancellationToken.None);
                    }
                }
                finally
                {
                    FetchingData = false;
                    ProgressBarVisibility = Visibility.Hidden;
                }
            });
        }

        private string DetermineVerification(string? approvalType)
        {
            switch (approvalType)
            {
                case BulkAlignmentReviewTags.ApproveSelected:
                    return "Verified";
                case BulkAlignmentReviewTags.DisapproveSelected:
                    return "Invalid";
                case BulkAlignmentReviewTags.MarkSelectedAsNeedsReview:
                default:
                    return "Unverified";
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
                        AlignedWords = new BindableCollection<AlignedWord>(alignedWordDictionary.Select(kvp =>
                                new AlignedWord
                                {
                                    Source = _sourceToTarget ? pivotWord.Word : kvp.Key,
                                    Target = _sourceToTarget ? kvp.Key : pivotWord.Word,
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
            if (alignedWord is { Source: { }, Target: { } } && _alignmentSet != null)
            {
                CurrentAlignedWord = alignedWord;
                RelevantBooks = alignedWord.RelevantBooks;
                CurrentBook = RelevantBooks[0];
            }
        }

        private AlignmentSet? _alignmentSet;
        private async Task GetPivotWords()
        {
            _ = await Task.Factory.StartNew(async () =>
            {
                var taskName = "GetPivotWords";
                var cancellationToken = new CancellationTokenSource().Token;
                try
                {

                    //await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                    //    description: $"Fetching Pivot Words", cancellationToken: cancellationToken);


                    AlignedWords = null;
                    BulkAlignments = null;
                    AlignmentCounts = null;
                    FetchingData = true;
                    ProgressBarVisibility = Visibility.Visible;

                    if (_alignmentSet == null && EnhancedViewItemMetadatum is AlignmentEnhancedViewItemMetadatum)
                    {
                        AlignmentSetId = Guid.Parse(((AlignmentEnhancedViewItemMetadatum)EnhancedViewItemMetadatum)
                            .AlignmentSetId!);
                        _alignmentSet = await AlignmentSet.Get(new AlignmentSetId(AlignmentSetId), Mediator!);
                    }

                    if (_alignmentSet != null)
                    {
                        AlignmentCounts = await _alignmentSet.GetAlignmentCounts(_sourceToTarget, _countsByTrainingText, true,
                            AlignmentTypes);

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
                }
                finally
                {

                    FetchingData = false;
                    ProgressBarVisibility = Visibility.Hidden;
                }
            });
        }

        private Dictionary<AlignmentTypes, string> AlignmentTypesMap = new Dictionary<AlignmentTypes, string>();
        private List<Book> _relevantBooks;
        private Book _currentBook;
        private PagingCollectionView _pagedBulkAlignments;

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
                { AlignmentTypes.Assigned_Verified, LocalizationService.Get("BulkAlignmentReview_Approved") },
                { AlignmentTypes.Assigned_Invalid, LocalizationService.Get("BulkAlignmentReview_Disapproved") },
                { AlignmentTypes.Assigned_Unverified, LocalizationService.Get("BulkAlignmentReview_NeedsApproval") },
            };
        }
    }
}
