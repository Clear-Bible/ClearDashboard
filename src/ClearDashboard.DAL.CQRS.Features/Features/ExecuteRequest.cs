using MediatR;

namespace ClearDashboard.DAL.CQRS.Features.Features
{
    public abstract class Requests
    {
        private bool _isBusy;
        
        public virtual Task<TResponse> ExecuteRequest<TResponse>(
            IRequest<TResponse> request,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            _isBusy = true;
            try
            {
                return mediator?.Send<TResponse>(request, cancellationToken);
            }
            finally
            {
                _isBusy = false;
            }
        }
    }
}
