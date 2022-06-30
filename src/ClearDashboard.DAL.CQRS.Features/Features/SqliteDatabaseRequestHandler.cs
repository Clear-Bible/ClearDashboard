using System.Data.SQLite;
using ClearDashboard.DataAccessLayer.Data.Sqlite;
using MediatR;
using Microsoft.Extensions.Logging;

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
    protected SQLiteDataReader? DataReader { get; private set; }

    protected SqliteDatabaseRequestHandler(ILogger logger) : base(logger)
    {
        //no-op
    }

    protected TData ExecuteSqliteCommandAndProcessData(string commandText)
    {
        var connectionManager = new SqliteConnectionManager(ResourcePath, Logger);
        try
        {
            var sqliteCmd = connectionManager.CreateCommand(commandText);
            DataReader = sqliteCmd.ExecuteReader();
            return ProcessData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"An unexpected error occurred while executing the following command: '{commandText}'");
            throw;
        }
        finally
        {
            connectionManager.CloseConnection();
        }
    }
}