using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.DataAccessLayer.Wpf;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Interfaces
{
    interface IWorkspace
    {
        ILogger _logger { get; set; }
        INavigationService _navigationService { get; set; }
        ProjectManager _projectManager { get; set; }

        void HandleEventAsync(object sender, PipeEventArgs args);
        //void OnViewAttached(object view, object context);
        //void Dispose(bool disposing);
    }
}
