using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Project
{
    public class AlignmentViewModel : PaneViewModel
    {

        public AlignmentViewModel()
        {

        }

        public AlignmentViewModel(INavigationService navigationService, ILogger<AlignmentViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            Title = "⳼ ALIGNMENT TOOL";
            ContentId = "ALIGNMENTTOOL";
            DisplayName = "⳼ ALIGNMENT TOOL";
        }
    }
}
