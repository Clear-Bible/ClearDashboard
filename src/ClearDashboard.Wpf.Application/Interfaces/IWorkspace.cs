using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Interfaces
{
    public interface IWorkspace
    {
        ILogger Logger { get; }
        INavigationService NavigationService { get; }
        DashboardProjectManager ProjectManager { get; }
    }
}
