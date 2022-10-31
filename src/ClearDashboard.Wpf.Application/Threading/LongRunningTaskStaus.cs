namespace ClearDashboard.Wpf.Application.Threading;

public enum LongRunningTaskStatus
{
    NotStarted,
    Running,
    Failed,
    Completed,
    Canceled
}