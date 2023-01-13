using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Aqua
{
    public interface IAquaRequestCorpusAnalysisDialogViewModel : IParatextCorpusDialogViewModel, IAquaDialogViewModel
    {
        Task<LongRunningTaskStatus> RequestAnalysis();
    }
}
