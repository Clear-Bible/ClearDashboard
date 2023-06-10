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
using System;
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
        }

        private BindableCollection<PivotWord> _pivotWords;
        public BindableCollection<PivotWord> PivotWords
        {
            get => _pivotWords;
            set => Set(ref _pivotWords, value);
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
            _ = await Task.Factory.StartNew(async () =>
            {
                try
                {
                    FetchingData = true;
                    ProgressBarVisibility = Visibility.Visible;

                    AlignmentSetId =
                        Guid.Parse((EnhancedViewItemMetadatum as AlignmentEnhancedViewItemMetadatum).AlignmentSetId);

                    await GetPivotWords();

                }
                finally
                {
                    FetchingData = false;
                    ProgressBarVisibility = Visibility.Hidden;
                }
            });
        }


        public async void OnPivotWordSourceChanged(SelectionChangedEventArgs e)
        {

            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListBoxItem item)
            {
                _sourceToTarget = (item.Tag as string) == BulkAlignmentReviewTags.Source;

                await GetPivotWords();
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


        private async Task GetPivotWords()
            //private async Task<ICollectionView> GetPivotWords()
        {
            
                var alignmentSet =
                    await AlignmentSet.Get(new AlignmentSetId(AlignmentSetId), Mediator!);

                if (alignmentSet != null)
                {
                    var alignmentCounts = await alignmentSet.GetAlignmentCounts(_sourceToTarget);
                    PivotWords = new BindableCollection<PivotWord>(alignmentCounts.Select(kvp =>
                        new PivotWord { Word = kvp.Key, Count = kvp.Value.Count }).OrderByDescending(kvp=>kvp.Count));
                }
           
        }

    }
}
