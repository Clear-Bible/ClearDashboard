using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.DataAccessLayer.Wpf;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Interfaces
{
    interface IWorkspace
    {
        ILogger Logger { get; }
        INavigationService NavigationService { get;}
        ProjectManager ProjectManager { get;  }

        void HandleEventAsync(object sender, PipeEventArgs args);
        //void OnViewAttached(object view, object context);
        //void Dispose(bool disposing);
    }
}
