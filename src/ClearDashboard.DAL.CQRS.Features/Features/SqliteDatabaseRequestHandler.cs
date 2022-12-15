using MediatR;
using Microsoft.Extensions.Logging;
using SqliteConnection = Microsoft.Data.Sqlite.SqliteConnection;

namespace ClearDashboard.DAL.CQRS.Features;

/// <summary>
/// A base class used to query data from Sqlite databases - typically found in the "Resources" folder found in the directory of the executable.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <typeparam name="TData"></typeparam>
public abstract class SqliteDatabaseRequestHandler<TRequest, TResponse, TData> : ResourceRequestHandler<TRequest, TResponse, TData>
    where TRequest : IRequest<TResponse>
{
    protected Microsoft.Data.Sqlite.SqliteDataReader? DataReader { get; private set; }

    protected SqliteDatabaseRequestHandler(ILogger logger) : base(logger)
    {
        //no-op
    }

    protected TData ExecuteSqliteCommandAndProcessData(string commandText)
    {
        var connection = new SqliteConnection($"Data Source={ResourcePath};Cache=Shared");
        connection.CreateCollation("NOCASE", (s1, s2) => string.Compare(s1, s2, StringComparison.InvariantCultureIgnoreCase));
        try
        {
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = commandText;
            DataReader = cmd.ExecuteReader();

            return ProcessData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"An unexpected error occurred while executing the following command: '{commandText}'");
            throw;
        }
        finally
        {
            connection.Close();
        }
    }
}