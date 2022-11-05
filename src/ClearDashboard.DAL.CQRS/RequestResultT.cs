namespace ClearDashboard.DAL.CQRS
{
    public class RequestResult<T> : Result<T>
    {
        public RequestResult()
        {
        }

        #nullable enable
        public RequestResult(T? result = default, bool success = true, string message = "Success", bool canceled = false) : base(result, success, message, canceled)
        {
        }
    }
}
