
namespace ClearDashboard.DAL.CQRS
{
    public class QueryResult<T> : Result<T>
    {
        public QueryResult(T? result, bool success = true, string message = "Success") : base(result, success, message)
        {
        }
    }
}
