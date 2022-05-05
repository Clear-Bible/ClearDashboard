
namespace ClearDashboard.DAL.CQRS
{
    public class RequestResult<T> : Result<T>
    {
        public RequestResult()
        {

        }

        public RequestResult(T? result = default(T), bool success = true, string message = "Success") : base(result, success, message)
        {
        }
    }
}
