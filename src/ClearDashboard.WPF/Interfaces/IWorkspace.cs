using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.NamedPipes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Interfaces
{
    interface IWorkspace
    {
        ILogger Logger { get; set; }
        INavigationService NavigationService { get; set; }
        ProjectManager ProjectManager { get; set; }

        void HandleEventAsync(object sender, PipeEventArgs args);
        //void OnViewAttached(object view, object context);
        //void Dispose(bool disposing);
    }
}
