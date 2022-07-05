using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.CQRS.Features;

public abstract record ProjectRequestCommand<TData>(string ProjectName) : IRequest<RequestResult<TData>>;

public abstract class AlignmentDbContextCommandHandler<TRequest, TResponse, TData> : IRequestHandler<TRequest, TResponse>
    where TRequest : ProjectRequestCommand<TData>, IRequest<TResponse>
    where TResponse : RequestResult<TData>, new()
{
    protected ProjectNameDbContextFactory? ProjectNameDbContextFactory { get; init; }
    protected IProjectProvider? ProjectProvider { get; set; }
    protected ILogger Logger { get; init; }

    protected AlignmentContext AlignmentContext { get; set; } = null!;

    protected AlignmentDbContextCommandHandler(ProjectNameDbContextFactory? projectNameDbContextFactory, IProjectProvider? projectProvider, ILogger logger)
    {
        ProjectNameDbContextFactory = projectNameDbContextFactory ?? throw new ArgumentNullException(nameof(projectNameDbContextFactory));
        ProjectProvider = projectProvider ?? throw new ArgumentNullException(nameof(projectProvider));
        Logger = logger;
    }

    protected abstract Task<TResponse> SaveData(TRequest request, CancellationToken cancellationToken);

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try 
        {
            AlignmentContext = await ProjectNameDbContextFactory!.GetDatabaseContext(ProjectProvider.CurrentProject.ProjectName).ConfigureAwait(false);
            return await SaveData(request, cancellationToken);

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
}