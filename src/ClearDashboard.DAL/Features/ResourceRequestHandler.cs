using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features;

public abstract class ResourceRequestHandler<TRequest, TResponse, TData> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TData : new()
{
    protected ILogger Logger { get; }
    protected string ResourceDirectory { get; set; }  = Path.Combine(Environment.CurrentDirectory, "Resources");
    protected abstract string ResourceName { get; set; }
    protected string ResourcePath => Path.Combine(ResourceDirectory, ResourceName);
    
    protected ResourceRequestHandler(ILogger logger)
    {
        Logger = logger;
    }

    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    protected abstract TData ProcessData();

    protected QueryResult<TResultData> ValidateResourcePath<TResultData>(TResultData data)
    {
        var queryResult = new QueryResult<TResultData>(data);

        if (string.IsNullOrEmpty(ResourceName))
        {
            LogAndSetUnsuccessfulResult(ref queryResult, "Please set 'ResourceName'.");
            return queryResult;
        }
       
        if (!File.Exists(ResourcePath) )
        {

            LogAndSetUnsuccessfulResult(ref queryResult, $"{ResourceName} does not exist in the directory {ResourceDirectory}");
        }
        return queryResult;
    }

    protected void LogAndSetUnsuccessfulResult<TResultData>(ref QueryResult<TResultData> queryResult, string message, Exception? ex = null)
    {
        if (ex != null)
        {
            Logger.LogError(ex, message);
            queryResult.Message = string.IsNullOrEmpty(message)? ex.Message: $"{message}. {ex.Message}";
        }
        else
        {
            Logger.LogError(message);
            queryResult.Message = message;
        }
        queryResult.Success = false;
    }

}