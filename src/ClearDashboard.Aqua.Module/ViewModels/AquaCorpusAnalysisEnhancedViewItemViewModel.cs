using Autofac;
using Caliburn.Micro;
using ClearDashboard.Aqua.Module.Models;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;


// ReSharper disable InconsistentNaming

namespace ClearDashboard.Aqua.Module.ViewModels
{
    public class AquaCorpusAnalysisEnhancedViewItemViewModel : VerseAwareEnhancedViewItemViewModel
    {
        public AquaCorpusAnalysisEnhancedViewItemViewModel(
            DashboardProjectManager? projectManager, 
            INavigationService? navigationService, 
            ILogger<VerseAwareEnhancedViewItemViewModel>? logger, 
            IEventAggregator? eventAggregator, 
            IMediator? mediator, 
            ILifetimeScope? lifetimeScope,
            IWindowManager windowManager, 
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, windowManager, localizationService)
        {
        }

        public override Task GetData(EnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken)
        {
            UrlString = ((AquaCorpusAnalysisEnhancedViewItemMetadatum)metadatum).UrlString;
            return base.GetData(metadatum, cancellationToken);
        }

        private string? urlString_;
        public string? UrlString
        {
            get => urlString_;
            set
            {
                urlString_ = value;
                NotifyOfPropertyChange(() => UrlString);
            }
        }
    }
}
