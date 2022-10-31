using System.Threading;

namespace ClearDashboard.Wpf.Application.Threading;

public class LongRunningTask
{
    public LongRunningTask(string name, CancellationTokenSource cancellationTokenSource, LongRunningTaskStatus status = LongRunningTaskStatus.NotStarted)
    {
        Name = name;
        Status = status;
        CancellationTokenSource = cancellationTokenSource;
    }

    //public LongRunningTask()
    //{
    //    CancellationTokenSource = new CancellationTokenSource();
    //    Status = LongRunningTaskStatus.NotStarted;
    //}

    public CancellationTokenSource? CancellationTokenSource { get; private set; }
    public string Name { get; set; }
    public LongRunningTaskStatus Status { get; set; }
}