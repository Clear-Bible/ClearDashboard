using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Collaboration.Features
{

    /// <summary>
    /// A base class used to query data from Sqlite databases - typically found in the "Resources" folder found in the directory of the executable.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public abstract class
        CollabRequestHandler<TRequest, TResponse, TData> : ResourceRequestHandler<TRequest, TResponse, TData>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger _logger;
        protected CollabRequestHandler(ILogger logger) : base(logger)
        {
            _logger = logger;
            //no-op
        }


    }
}
