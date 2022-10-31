namespace ClearDashboard.DataAccessLayer.Threading;

public enum LongRunningTaskStatus
{
    NotStarted,
    Running,
    Completed,
    Failed,
    CancellationRequested,
    Cancelled
}

//public enum LongRunningProcessStatus
//{
//    NotStarted,
//    Running,
//    Completed,
//    Failed,
//    CancellationRequested,
//    Cancelled
//}