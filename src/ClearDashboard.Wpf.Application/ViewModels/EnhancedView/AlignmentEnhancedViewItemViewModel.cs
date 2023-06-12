using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Controls;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.Views.EnhancedView;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.EventsAndDelegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


// ReSharper disable InconsistentNaming

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{



    //public static class PivotWordSources
    //{
    //    public const string Source = "Source";
    //    public const string Target = "Target";
    //}

    public class AlignmentEnhancedViewItemViewModel : VerseAwareEnhancedViewItemViewModel
    {
        public AlignmentEnhancedViewItemViewModel(DashboardProjectManager? projectManager, IEnhancedViewManager enhancedViewManager, INavigationService? navigationService, ILogger<VerseAwareEnhancedViewItemViewModel>? logger, IEventAggregator? eventAggregator, IMediator? mediator, ILifetimeScope? lifetimeScope, IWindowManager windowManager, ILocalizationService localizationService)
            : base(projectManager, enhancedViewManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, windowManager, localizationService)
        {
            ShowEditButton = true;
            EnableEditMode = false;
            EditModeButtonLabel = LocalizationService.Get("BulkAlignmentReview_BulkAlignmentReview");
            _sourceToTarget = true;



            AlignedWordsOptions = new BindableCollection<string>
            {
                LocalizationService.Get("BulkAlignmentReview_Machine"),
                LocalizationService.Get("BulkAlignmentReview_NeedsReview"),
                LocalizationService.Get("BulkAlignmentReview_Disapprove"),
                LocalizationService.Get("BulkAlignmentReview_Approve")
            };

            SelectedAlignedWordsOptions = new BindableCollection<string>()
            {
                LocalizationService.Get("BulkAlignmentReview_Machine"),
                LocalizationService.Get("BulkAlignmentReview_NeedsReview"),
                LocalizationService.Get("BulkAlignmentReview_Disapprove"),
                LocalizationService.Get("BulkAlignmentReview_Approve")
            };

            // TODO:  this should be a user setting.
            _countsByTrainingText = true;
        }

        private BindableCollection<PivotWord>? _pivotWords;
        public BindableCollection<PivotWord>? PivotWords
        {
            get => _pivotWords;
            set => Set(ref _pivotWords, value);
        }

        private BindableCollection<AlignedWord>? _alignedWords;
        public BindableCollection<AlignedWord>? AlignedWords
        {
            get => _alignedWords;
            set => Set(ref _alignedWords, value);
        }

        protected override void OnViewAttached(object view, object context)
        {
            var v = (AlignmentEnhancedViewItemView)view;
            var abr = (AlignmentBulkReview)v.FindName("AlignmentBulkReview");
            var lb = (MultipleSelectionListBox)abr.FindName("AlignedWordsOptionListBox");

            lb.SetSelected(SelectedAlignedWordsOptions);

            base.OnViewAttached(view, context);
        }

        private bool _sourceToTarget;

        protected override async Task GetEditorData()
        {

            await GetPivotWords();

        }


        public async void OnPivotWordSourceChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem item)
            {
                _sourceToTarget = (item.Tag as string) == BulkAlignmentReviewTags.Source;

                AlignedWords = null;
                await GetPivotWords();
            }
        }

        private bool _countsByTrainingText;
        public async void OnByTextTypeChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem item)
            {
                _countsByTrainingText = (item.Tag as string) == BulkAlignmentReviewTags.CountsByTrainingText;

                AlignedWords = null;
                await GetPivotWords();
            }
            await Task.CompletedTask;
        }

        public async void OnPivotWordRowSelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is PivotWord item)
            {
                var success = AlignmentCounts.TryGetValue(item.Word, out IDictionary<string, IDictionary<string, uint>> alignedWordDictionary);

                if (success)
                {
                    AlignedWords = new BindableCollection<AlignedWord>(alignedWordDictionary.Select(kvp =>
                        new AlignedWord { Source = item.Word, Target = kvp.Key, Count = kvp.Value.Count }).OrderByDescending(kvp => kvp.Count));
                }
                await Task.CompletedTask;
            }
        }

        private BindableCollection<string> _selectedAlignedWordsOptions;

        public BindableCollection<string> SelectedAlignedWordsOptions
        {
            get => _selectedAlignedWordsOptions;
            set => Set(ref _selectedAlignedWordsOptions, value);
        }

        private BindableCollection<string> _alignedWordsOptions;

        public BindableCollection<string> AlignedWordsOptions
        {
            get => _alignedWordsOptions;
            set => Set(ref _alignedWordsOptions, value);
        }

        private IDictionary<string, IDictionary<string, IDictionary<string, uint>>> AlignmentCounts { get; set; }

        private async Task GetPivotWords()
        {
            _ = await Task.Factory.StartNew(async () =>
            {
                try
                {
                    FetchingData = true;
                    ProgressBarVisibility = Visibility.Visible;

                    AlignmentSetId =
                        Guid.Parse((EnhancedViewItemMetadatum as AlignmentEnhancedViewItemMetadatum).AlignmentSetId);
                    var alignmentSet =
                            await AlignmentSet.Get(new AlignmentSetId(AlignmentSetId), Mediator!);

                    if (alignmentSet != null)
                    {
                        AlignmentCounts = await alignmentSet.GetAlignmentCounts(_sourceToTarget);
                        PivotWords = new BindableCollection<PivotWord>(AlignmentCounts.Select(kvp =>
                            new PivotWord { Word = kvp.Key, Count = kvp.Value.Count }).OrderByDescending(kvp => kvp.Count));
                    }



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
