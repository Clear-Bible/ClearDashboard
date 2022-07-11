namespace ClearDashboard.DAL.CQRS;

public class CommandResult<T> : Result<T>
{
    #nullable enable
    public CommandResult(T? result, bool success = true, string message = "Success") : base(result, success, message)
    {
    }
}