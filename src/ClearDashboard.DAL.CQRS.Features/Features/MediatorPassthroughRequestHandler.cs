using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.CQRS.Features;

public abstract class MediatorPassthroughRequestHandler<TRequest, TResponse, TData> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse: RequestResult<TData>
{

    protected ILogger Logger { get; }
    protected IMediator Mediator { get; }

    protected MediatorPassthroughRequestHandler(IMediator mediator, ILogger logger)
    {
        Mediator = mediator;
        Logger = logger;
      
    }

    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

}