using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Project
{
    public class ProjectDesignSurfaceViewModel : ToolViewModel
    {

        public ProjectDesignSurfaceViewModel()
        {

        }

        public ProjectDesignSurfaceViewModel(INavigationService navigationService, ILogger<ProjectDesignSurfaceViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            Title = "PROJECT DESIGN SURFACE";
            ContentId = "PROJECTDESIGNSURFACETOOL";
        }
    }
}
