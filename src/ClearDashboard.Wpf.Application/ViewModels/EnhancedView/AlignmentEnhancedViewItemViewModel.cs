using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
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
using ClearDashboard.DataAccessLayer.Threading;
using static ClearDashboard.DataAccessLayer.Threading.BackgroundTaskStatus;

// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming
namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class AlignmentEnhancedViewItemViewModel : VerseAwareEnhancedViewItemViewModel
    {
        public AlignmentEnhancedViewItemViewModel(DashboardProjectManager? projectManager, IEnhancedViewManager enhancedViewManager, INavigationService? navigationService, ILogger<VerseAwareEnhancedViewItemViewModel>? logger, IEventAggregator? eventAggregator, IMediator? mediator, ILifetimeScope? lifetimeScope, IWindowManager windowManager, ILocalizationService localizationService)
            : base(projectManager, enhancedViewManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, windowManager, localizationService)
        {
            ShowEditButton = true;
            EnableEditMode = false;
            EditModeButtonLabel = LocalizationService.Get("BulkAlignmentReview_BulkAlignmentReview");
            _sourceToTarget = true;

            AlignmentTypes = AlignmentTypes.Assigned_Invalid | 
                             AlignmentTypes.Assigned_Unverified |
                             AlignmentTypes.Assigned_Verified |
                             AlignmentTypes.FromAlignmentModel_Unverified_Not_Otherwise_Included;

            // TODO:  this should be a user setting.
            _countsByTrainingText = true;
        }



        private readonly DebounceDispatcher _debounceTimer = new ();

        public bool HasPivotWords => PivotWords is { Count: > 0 };

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
        private BindableCollection<BulkAlignment>? _bulkAlignments;
        public BindableCollection<BulkAlignment>? BulkAlignments
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
                    bulkAlignment.IsSelected = checkBox.IsChecked ?? false ;
                }
            }
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


        public async void OnSourceTargetCountRowSelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is AlignedWord alignedWord)
            {
                await GetAlignmentVerseContexts(alignedWord);
            }
        }
        
        public void OnPivotWordRowSelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is PivotWord pivotWord)
            {
                if (AlignmentCounts != null && pivotWord.Word != null)
                {
                    var success = AlignmentCounts.TryGetValue(pivotWord.Word, out IDictionary<string, IDictionary<string, uint>>? alignedWordDictionary);

                    if (success && alignedWordDictionary != null)
                    {
                        AlignedWords = new BindableCollection<AlignedWord>(alignedWordDictionary.Select(kvp =>
                                new AlignedWord
                                {
                                    Source = _sourceToTarget ? pivotWord.Word : kvp.Key, 
                                    Target = _sourceToTarget ? kvp.Key : pivotWord.Word, 
                                    Count = kvp.Value.Sum(k=>k.Value),
                                    PivotWord = pivotWord
                                })
                            .OrderByDescending(kvp => kvp.Count));
                    }
                }
            }
        }

        private IDictionary<string, IDictionary<string, IDictionary<string, uint>>>? AlignmentCounts { get; set; }

        private AlignmentTypes _alignmentTypes;
        public AlignmentTypes AlignmentTypes
        {
            get => _alignmentTypes;
            set => Set(ref _alignmentTypes, value);
        }

        private IEnumerable<(Alignment alignment, IEnumerable<Token> sourceVerseTokens, uint sourceVerseTokensIndex, IEnumerable<Token> targetVerseTokens, uint targetVerseTokensIndex)>? _verseContexts;
        private async Task GetAlignmentVerseContexts(AlignedWord alignedWord)
        {
            _ = await Task.Factory.StartNew(async () =>
            {
                try
                {
                    if (alignedWord is { Source: { }, Target: { } } && _alignmentSet != null)
                    {
                        FetchingData = true;
                        ProgressBarVisibility = Visibility.Visible;
                        _verseContexts = await _alignmentSet.GetAlignmentVerseContexts(alignedWord.Source,
                            alignedWord.Target, _countsByTrainingText, AlignmentTypes);
                        BulkAlignments = new BindableCollection<BulkAlignment>(_verseContexts.Select(verseContext =>
                            new BulkAlignment
                            {
                                Alignment = verseContext.alignment, IsSelected = false,
                                Type = verseContext.alignment.Verification,
                                SourceVerseTokens = verseContext.sourceVerseTokens,
                                SourceVerseTokensIndex = verseContext.sourceVerseTokensIndex,
                                TargetVerseTokens = verseContext.targetVerseTokens,
                                TargetVerseTokensIndex = verseContext.targetVerseTokensIndex
                                
                            }));
                    }
                }
                finally
                {
                    FetchingData = false;
                    ProgressBarVisibility = Visibility.Hidden;
                }
            });
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
                        AlignmentCounts = await _alignmentSet.GetAlignmentCounts(_sourceToTarget, _countsByTrainingText,
                            AlignmentTypes);
                        PivotWords = new BindableCollection<PivotWord>(AlignmentCounts.Select(kvp =>
                                new PivotWord { Word = kvp.Key, Count = kvp.Value.Count })
                            .OrderByDescending(kvp => kvp.Count));
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
    }
}
