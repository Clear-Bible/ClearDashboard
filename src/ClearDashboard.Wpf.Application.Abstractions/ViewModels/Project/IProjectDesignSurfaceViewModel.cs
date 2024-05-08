using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.Project
{
    public  interface IProjectDesignSurfaceViewModel
    {

        Task ExecuteCorpusNodeMenuCommand(CorpusNodeMenuItemViewModel corpusNodeMenuItem, CancellationToken cancellationToken);
        Task ExecuteConnectionMenuCommand(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem, CancellationToken cancellationToken);
        Task AddParatextCorpus(string selectedParatextProjectId, ParallelProjectType parallelProjectType);
        Task AddParallelCorpus(ParallelCorpusConnectionViewModel newParallelCorpusConnection, ParallelProjectType parallelProjectType, CancellationToken cancellationToken);
        Task SaveDesignSurfaceData();

        bool IsBusy { get; }
       
    }
}
