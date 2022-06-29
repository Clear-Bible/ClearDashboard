using System;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features;

public abstract record ProjectRequestCommand<TData>(string ProjectName) : IRequest<RequestResult<TData>>;

public abstract class AlignmentDbContextCommandHandler<TRequest, TResponse, TData> : IRequestHandler<TRequest, TResponse>
    where TRequest : ProjectRequestCommand<TData>, IRequest<TResponse>
    where TResponse : RequestResult<TData>, new()
{
    protected ProjectNameDbContextFactory? ProjectNameDbContextFactory { get; init; }
    protected ILogger Logger { get; init; }

    protected AlignmentContext AlignmentContext { get; set; } = null!;

    protected AlignmentDbContextCommandHandler(ProjectNameDbContextFactory? projectNameDbContextFactory, ILogger logger)
    {
        ProjectNameDbContextFactory = projectNameDbContextFactory ?? throw new ArgumentNullException(nameof(projectNameDbContextFactory));
        Logger = logger;
    }

    protected abstract Task<TResponse> SaveData(TRequest request, CancellationToken cancellationToken);

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try 
        {
            AlignmentContext = await ProjectNameDbContextFactory!.GetDatabaseContext(request.ProjectName).ConfigureAwait(false);
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