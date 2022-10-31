using System;
using System.Threading;

namespace ClearDashboard.DataAccessLayer.Threading;

public class LongRunningTask : IDisposable
{
    public LongRunningTask(string name, CancellationTokenSource cancellationTokenSource, LongRunningTaskStatus status = LongRunningTaskStatus.NotStarted)
    {
        Name = name;
        Status = status;
        CancellationTokenSource = cancellationTokenSource;
    }

    public CancellationTokenSource? CancellationTokenSource { get; private set; }
    public string Name { get; set; }
    public LongRunningTaskStatus Status { get; set; }

    public void Cancel()
    {
        Status = LongRunningTaskStatus.Cancelled;
        CancellationTokenSource?.Cancel();
    }

    public void Complete()
    {
        Status = LongRunningTaskStatus.Completed;
        Dispose();
    }

    public void Dispose()
    {
        CancellationTokenSource?.Dispose();
        CancellationTokenSource = null;
    }
}