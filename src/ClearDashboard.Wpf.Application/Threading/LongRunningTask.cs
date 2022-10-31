using System;
using System.Threading;

namespace ClearDashboard.Wpf.Application.Threading;

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

    public void Dispose()
    {
        CancellationTokenSource?.Dispose();
    }
}