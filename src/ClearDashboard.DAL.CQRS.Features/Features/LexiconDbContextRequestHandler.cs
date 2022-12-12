using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.CQRS.Features
{
    public abstract record LexiconRequestQuery<TData> : IRequest<RequestResult<TData>>;
    
    public abstract class LexiconDbContextQueryHandler<TRequest, TResponse, TData> : IRequestHandler<TRequest, TResponse>
        where TRequest : LexiconRequestQuery<TData>, IRequest<TResponse>
        where TResponse : RequestResult<TData>, new()
    {
        protected LexiconDbContextFactory? LexiconDbContextFactory { get; init; }
        protected ILogger Logger { get; init; }

        protected LexiconDbContext LexiconDbContext { get; set; }
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected LexiconDbContextQueryHandler(LexiconDbContextFactory? lexiconDbContextFactory, ILogger logger)
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            LexiconDbContextFactory = lexiconDbContextFactory ?? throw new ArgumentNullException(nameof(lexiconDbContextFactory));
            Logger = logger;
        }

        protected abstract Task<TResponse> GetDataAsync(TRequest request, CancellationToken cancellationToken);
       
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (LexiconDbContextFactory!.ServiceScope.Tag == Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag)
                {
                    LexiconDbContext = await LexiconDbContextFactory!.GetDatabaseContext(false).ConfigureAwait(false);
                    return await GetDataAsync(request, cancellationToken);
                }
                else
                {
                    using var requestScope = LexiconDbContextFactory!.ServiceScope
                        .BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

                    LexiconDbContext = await LexiconDbContextFactory!.GetDatabaseContext(false, requestScope).ConfigureAwait(false);
                    return await GetDataAsync(request, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                return new TResponse
                {
                    Message = "Operation Canceled",
                    Success = false,
                    Canceled = true
                };
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = (ex.InnerException is not null) ?
                    $" (inner exception message: {ex.InnerException.Message})" :
                    "";
                return new TResponse
                {
                    Message = $"Exception type: {ex.GetType().Name}, having message: {ex.Message}{innerExceptionMessage}",
                    Success = false
                };
            }
           
        }
    }


}
