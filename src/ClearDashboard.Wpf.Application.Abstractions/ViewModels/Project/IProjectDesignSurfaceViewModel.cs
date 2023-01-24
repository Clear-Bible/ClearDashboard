using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;

namespace ClearDashboard.Wpf.Application.ViewModels.Project
{
    public  interface IProjectDesignSurfaceViewModel
    {

        Task ExecuteCorpusNodeMenuCommand(CorpusNodeMenuItemViewModel corpusNodeMenuItem);
        Task ExecuteConnectionMenuCommand(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem);
        Task AddParatextCorpus(string selectedParatextProjectId);
        Task AddParallelCorpus(ParallelCorpusConnectionViewModel newParallelCorpusConnection);
        Task SaveDesignSurfaceData();

        bool IsBusy { get; }
    }
}
