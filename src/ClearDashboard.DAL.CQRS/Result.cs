namespace ClearDashboard.DAL.CQRS;

public abstract class Result<T>
{
    protected Result() {}
    #nullable enable
    protected Result(T? result = default(T), bool success = true, string message = "Success")
    {
        Success = success;
        Message = message;
        Data = result;
    }

    public T? Data { get; set; }   
    public bool Success { get; set; }
    public string Message { get; set; }

    public bool HasData => Data != null;
}