using System;
using System.Threading;

namespace ClearDashboard.DataAccessLayer.Threading;

public class LongRunningTask : IDisposable, IEquatable<LongRunningTask>
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
        Dispose();
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

    public bool Equals(LongRunningTask? other)
    {
        return Equals((object?)other);
    }

    public override bool Equals(object? obj)
    {
        return obj?.GetType() == typeof(LongRunningTask) && 
           obj is LongRunningTask longRunningTask &&
           Name == longRunningTask.Name;

    }
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}