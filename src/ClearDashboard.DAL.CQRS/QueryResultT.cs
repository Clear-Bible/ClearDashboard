
namespace ClearDashboard.DAL.CQRS
{
    public class QueryResult<T> : Result<T>
    {
        public QueryResult()
        {

        }

        public QueryResult(T? result = default(T), bool success = true, string message = "Success") : base(result, success, message)
        {
        }
    }
}
