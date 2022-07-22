using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Project
{
    public class CorpusViewModel : PaneViewModel
    {

        public CorpusViewModel()
        {

        }

        public CorpusViewModel(INavigationService navigationService, ILogger<CorpusViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            Title = "Corpus";
            ContentId = "CORPUSTOOL";
        }
    }
}
