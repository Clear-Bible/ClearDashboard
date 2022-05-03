using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.WebApiParatextPlugin.Features.Project;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.WebApiParatextPlugin.Features
{
    public abstract class FeatureSliceController : ApiController
    {
        protected readonly IMediator Mediator;
        protected readonly ILogger Logger;

        protected FeatureSliceController(IMediator mediator, ILogger logger)
        {
            Mediator = mediator;
            Logger = logger;
        }

        protected async Task<QueryResult<TData>> ExecuteCommandAsync<TResponse, TData>(IRequest<TResponse> request, CancellationToken cancellationToken) where TResponse: QueryResult<TData>
        {
            try
            {
                return await Mediator.Send(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An unexpected error occurred while executing a command.");
                return new QueryResult<TData>(default(TData), false, ex.Message);
            }
        }

    }
}
