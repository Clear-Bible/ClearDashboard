using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Enums;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.Project
{
    public  interface IProjectDesignSurfaceViewModel
    {

        Task ExecuteCorpusNodeMenuCommand(CorpusNodeMenuItemViewModel corpusNodeMenuItem);
        Task ExecuteConnectionMenuCommand(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem);
        Task AddParatextCorpus(string selectedParatextProjectId, ParallelProjectType parallelProjectType);
        Task AddParallelCorpus(ParallelCorpusConnectionViewModel newParallelCorpusConnection, ParallelProjectType parallelProjectType);
        Task SaveDesignSurfaceData();

        bool IsBusy { get; }
       
    }
}
