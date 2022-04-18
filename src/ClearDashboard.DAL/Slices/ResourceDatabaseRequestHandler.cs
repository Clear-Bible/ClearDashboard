using System;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Sqlite;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Slices;

/// <summary>
/// A base class used to query data from Sqlite databases - typically found in the "Resources" folder found in the directory of the executable.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <typeparam name="TData"></typeparam>
public abstract class ResourceDatabaseRequestHandler<TRequest, TResponse, TData> : IRequestHandler<TRequest, TResponse> 
    where TRequest : IRequest<TResponse> 
    where TData : new()
{
    protected ILogger Logger { get; private set; }
    protected string ResourceDirectory => Path.Combine(Environment.CurrentDirectory, "Resources");
    protected abstract string DatabaseName { get; }
    protected string DatabasePath => Path.Combine(ResourceDirectory, DatabaseName);
    protected SQLiteDataReader? DataReader { get; private set; }

    protected ResourceDatabaseRequestHandler(ILogger logger)
    {
        Logger = logger;
    }

    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    protected abstract TData? ProcessData();

    protected TData? ExecuteCommand(string commandText) 
    {
        var connectionManager = new SqliteConnectionManager(DatabasePath, Logger);
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

    protected QueryResult<TResultData> ValidateDatabasePath<TResultData>(TResultData data)
    {
        var queryResult = new QueryResult<TResultData>(data);

        if (string.IsNullOrEmpty(DatabaseName))
        {
            LogAndSetUnsuccessfulResult(ref queryResult, "Please set 'DatabaseName'.");
            return queryResult;
        }
     
        if (!File.Exists(DatabasePath))
        {
           LogAndSetUnsuccessfulResult(ref queryResult, $"{DatabaseName} does not exist in the directory {ResourceDirectory}");
        }
        return queryResult;
    }

    protected void LogAndSetUnsuccessfulResult<TResultData>(ref QueryResult<TResultData> queryResult, string message, Exception? ex = null)
    {
        if (ex != null)
        {
            Logger.LogError(ex, message);
            queryResult.Message = ex.Message;
        }
        else
        {
            Logger.LogError(message);
            queryResult.Message = message;
        }
     
        queryResult.Success = false;
    }
   
}