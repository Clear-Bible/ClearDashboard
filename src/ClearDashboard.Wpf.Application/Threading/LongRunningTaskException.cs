using System;

namespace ClearDashboard.Wpf.Application.Threading;

public class LongRunningTaskException : Exception
{
    public LongRunningTaskException(string? message) : base(message)
    {

    }

    public LongRunningTaskException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}