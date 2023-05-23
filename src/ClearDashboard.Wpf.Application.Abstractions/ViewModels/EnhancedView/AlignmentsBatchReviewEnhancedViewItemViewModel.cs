
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class AlignmentsBatchReviewEnhancedViewItemViewModel : EnhancedViewItemViewModel
    {
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

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializeAsync(cancellationToken);
        }

        public override Task GetData(CancellationToken cancellationToken)
        {

           // var counts =AlignmentSet.Get(a)
            return base.GetData(cancellationToken);
        }
    }
}
