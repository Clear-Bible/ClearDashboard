
using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class AlignmentsBatchReviewEnhancedViewItemViewModel : EnhancedViewItemViewModel
    {
        private ICollectionView _pivotWords;

        public AlignmentsBatchReviewEnhancedViewItemViewModel(DashboardProjectManager? projectManager,
            IEnhancedViewManager enhancedViewManager,
            INavigationService? navigationService,
            ILogger<AlignmentsBatchReviewEnhancedViewItemViewModel>? logger,
            IEventAggregator? eventAggregator,
            IMediator? mediator,
            ILifetimeScope? lifetimeScope,
            ILocalizationService localizationService) :
            base(projectManager, enhancedViewManager, navigationService, logger, eventAggregator, mediator,
                lifetimeScope, localizationService)
        {
        }


        public ICollectionView PivotWords
        {
            get => _pivotWords;
            set => Set(ref _pivotWords, value);
        }

        public AlignmentsBatchReviewEnhancedViewItemMetadatum? AlignmentsBatchReviewEnhancedViewItemMetadatum
        {
            get
            {
               
                if (EnhancedViewItemMetadatum is AlignmentsBatchReviewEnhancedViewItemMetadatum metadatum)
                {
                    return metadatum;
                }
                throw new NullReferenceException($"EnhancedViewItemMetadatum must be of type '{nameof(Models.EnhancedView.AlignmentsBatchReviewEnhancedViewItemMetadatum)}' and must not be null.");
            }
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializeAsync(cancellationToken);
        }

        public override event EventHandler<DeactivationEventArgs> AttemptingDeactivation
        {
            add => base.AttemptingDeactivation += value;
            remove => base.AttemptingDeactivation -= value;
        }

        public override async Task GetData(CancellationToken cancellationToken)
        {
            //PivotWords = await GetPivotWords();

            await GetPivotWords();
           // return base.GetData(cancellationToken);
        }

        private async Task GetPivotWords()
        //private async Task<ICollectionView> GetPivotWords()
        {
            if (AlignmentsBatchReviewEnhancedViewItemMetadatum != null)
            {
                var alignmentSet =
                    await AlignmentSet.Get(
                        new AlignmentSetId(Guid.Parse(AlignmentsBatchReviewEnhancedViewItemMetadatum.AlignmentSetId ??
                                                      throw new NullReferenceException("'AlignmentSetId' must not be null."))), Mediator!);

                if (alignmentSet != null)
                {
                    var alignmentCounts = alignmentSet.GetAlignmentCounts(true, new CancellationToken());
                }
            }

            //var bindableCollection = new BindableCollection<>()
            //return CollectionViewSource.GetDefaultView(collectionName);

            //return null;
        }
    }
}
