using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Aqua
{
    public interface IAquaDialogViewModel : IParatextCorpusDialogViewModel
    {
        Visibility StatusBarVisibility { get; set; }
        string? DialogTitle { get; set; }

        // already in IParatextCorpusDialogViewModel
        //void Ok();
        //void Cancel();

        Task<LongRunningTaskStatus> AddVersion();

        Task<LongRunningTaskStatus> AddRevision();
    }
}
