
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DAL.NamedPipes;
using ClearDashboard.DataAccessLayer;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Interfaces
{
    interface IWorkspace
    {
        ILogger Logger { get; set; }
        INavigationService NavigationService { get; set; }
        ProjectManager _DAL { get; set; }

        void HandleEventAsync(object sender, NamedPipesClient.PipeEventArgs args);
        //void OnViewAttached(object view, object context);
        //void Dispose(bool disposing);
    }
}
