using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DataAccessLayer.Threading;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog
{
    public interface IAquaDialogViewModel
    {
        Visibility StatusBarVisibility { get; set; }
        string? DialogTitle { get; set; }

        // already in IParatextCorpusDialogViewModel
        void Ok();
        void Cancel();

        Task<LongRunningTaskStatus> AddVersion();

        Task<LongRunningTaskStatus> AddRevision();
    }
}
