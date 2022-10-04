using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.CQRS.Features;

public abstract record ProjectRequestCommand<TData> : IRequest<RequestResult<TData>>;

public abstract class ProjectDbContextCommandHandler<TRequest, TResponse, TData> : IRequestHandler<TRequest, TResponse> /*, IDisposable */
    where TRequest : ProjectRequestCommand<TData>,IRequest<TResponse>
    where TResponse : RequestResult<TData>, new()
{
    protected ProjectDbContextFactory? ProjectNameDbContextFactory { get; init; }
    protected IProjectProvider? ProjectProvider { get; set; }
    protected ILogger Logger { get; init; }

    protected ProjectDbContext ProjectDbContext { get; set; } = null!;

    protected ProjectDbContextCommandHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider? projectProvider, ILogger logger)
    {
        ProjectNameDbContextFactory = projectNameDbContextFactory ?? throw new ArgumentNullException(nameof(projectNameDbContextFactory));
        ProjectProvider = projectProvider ?? throw new ArgumentNullException(nameof(projectProvider));
        Logger = logger;
    }

    protected abstract Task<TResponse> SaveDataAsync(TRequest request, CancellationToken cancellationToken);

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ProjectProvider!.HasCurrentProject)
            {
                throw new InvalidOperationException("The ProjectProvider does not have a current project.");
            }

            // Try creating a child scope - essentially 'per handler request' - so that
            // when GetDatabaseContext is called, a new ProjectDbContext is created and then 
            // disposed after SaveDataAsync is called and the handlerScope is disposed:
            if (ProjectNameDbContextFactory!.ServiceScope.Tag == Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag)
            {
                ProjectDbContext = await ProjectNameDbContextFactory!.GetDatabaseContext(
                    ProjectProvider?.CurrentProject!.ProjectName!,
                    false).ConfigureAwait(false);
                return await SaveDataAsync(request, cancellationToken);
            }
            else
            {
                using var requestScope = ProjectNameDbContextFactory!.ServiceScope
                .BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

                ProjectDbContext = await ProjectNameDbContextFactory!.GetDatabaseContext(
                    ProjectProvider?.CurrentProject!.ProjectName!,
                    false,
                    requestScope).ConfigureAwait(false);
                return await SaveDataAsync(request, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            return new TResponse
            {
                Message = ex.Message,
                Success = false
            };
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            ProjectNameDbContextFactory?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}