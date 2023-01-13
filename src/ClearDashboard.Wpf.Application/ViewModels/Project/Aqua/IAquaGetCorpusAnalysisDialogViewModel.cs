using ClearDashboard.DataAccessLayer.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Aqua
{
    public interface IAquaGetCorpusAnalysisDialogViewModel : IAquaDialogViewModel
    {
        string? RequestId { get; set; }
        Task<LongRunningTaskStatus> GetAnalysis();
        string? Analysis { get; set; }

    }
}
