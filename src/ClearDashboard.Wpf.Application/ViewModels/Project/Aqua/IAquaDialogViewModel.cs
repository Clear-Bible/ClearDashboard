using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Aqua
{
    public interface IAquaDialogViewModel
    {
        Visibility StatusBarVisibility { get; set; }
        string? DialogTitle { get; set; }

        void Ok();
        void Cancel();
    }
}
