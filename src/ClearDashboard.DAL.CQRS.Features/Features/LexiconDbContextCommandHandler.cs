using ClearDashboard.DAL.CQRS.Features.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace ClearDashboard.DAL.CQRS.Features;

public abstract record LexiconRequestCommand<TData> : IRequest<RequestResult<TData>>;

public abstract class LexiconDbContextCommandHandler<TRequest, TResponse, TData> : IRequestHandler<TRequest, TResponse>
    where TRequest : LexiconRequestCommand<TData>,IRequest<TResponse>
    where TResponse : RequestResult<TData>, new()
{
    protected LexiconDbContextFactory? LexiconDbContextFactory { get; init; }
    protected ILogger Logger { get; init; }

    protected LexiconDbContext LexiconDbContext { get; set; } = null!;

    protected LexiconDbContextCommandHandler(LexiconDbContextFactory? lexiconDbContextFactory, ILogger logger)
    {
        LexiconDbContextFactory = lexiconDbContextFactory ?? throw new ArgumentNullException(nameof(lexiconDbContextFactory));
        Logger = logger;
    }

    protected abstract Task<TResponse> SaveDataAsync(TRequest request, CancellationToken cancellationToken);

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Try creating a child scope - essentially 'per handler request' - so that
            // when GetDatabaseContext is called, a new ProjectDbContext is created and then 
            // disposed after SaveDataAsync is called and the handlerScope is disposed:
            if (LexiconDbContextFactory!.ServiceScope.Tag == Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag)
            {
                LexiconDbContext = await LexiconDbContextFactory!.GetDatabaseContext(false).ConfigureAwait(false);
                return await SaveDataAsync(request, cancellationToken);
            }
            else
            {
                using var requestScope = LexiconDbContextFactory!.ServiceScope
                    .BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

                LexiconDbContext = await LexiconDbContextFactory!.GetDatabaseContext(false, requestScope).ConfigureAwait(false);
                return await SaveDataAsync(request, cancellationToken);
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

    public static async Task<LexiconDbContext> GetRequestScopeLexiconDbContext(LexiconDbContextFactory lexiconDbContextFactory)
    {
        // Try creating a child scope - essentially 'per handler request' - so that
        // when GetDatabaseContext is called, a new ProjectDbContext is created and then 
        // disposed after SaveDataAsync is called and the handlerScope is disposed:
        if (lexiconDbContextFactory!.ServiceScope.Tag == Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag)
        {
            return await lexiconDbContextFactory!.GetDatabaseContext(
                false).ConfigureAwait(false);
        }
        else
        {
            using var requestScope = lexiconDbContextFactory!.ServiceScope
                .BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            return await lexiconDbContextFactory!.GetDatabaseContext(
                false,
                requestScope).ConfigureAwait(false);
        }
    }
}