using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Translation;
using MahApps.Metro.Controls;
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
        }

        private BindableCollection<PivotWord> _pivotWords;
        public BindableCollection<PivotWord> PivotWords
        {
            get => _pivotWords;
            set => Set(ref _pivotWords, value);
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

        private async Task GetPivotWords()
            //private async Task<ICollectionView> GetPivotWords()
        {
            
                var alignmentSet =
                    await AlignmentSet.Get(new AlignmentSetId(AlignmentSetId), Mediator!);

                if (alignmentSet != null)
                {
                    var alignmentCounts = await alignmentSet.GetAlignmentCounts(_sourceToTarget, new CancellationToken());
                    PivotWords = new BindableCollection<PivotWord>(alignmentCounts.Select(kvp =>
                        new PivotWord { Word = kvp.Key, Count = kvp.Value.Count }).OrderByDescending(kvp=>kvp.Count));
                }
           
        }

    }
}
