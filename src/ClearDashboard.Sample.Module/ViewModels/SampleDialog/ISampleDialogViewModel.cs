using ClearDashboard.DataAccessLayer.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Sample.Module.ViewModels.SampleDialog
{
    public interface ISampleDialogViewModel
    {
        Visibility StatusBarVisibility { get; set; }
        string? DialogTitle { get; set; }

        string? SampleId { get; set; }
        void Ok();
        void Cancel();
        Task<LongRunningTaskStatus> AddVersion();

        Task<LongRunningTaskStatus> AddRevision();
    }
}
