using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.CQRS.Features
{
    public abstract record ProjectRequestQuery<TData> : IRequest<RequestResult<TData>>;
    
    public abstract class ProjectDbContextQueryHandler<TRequest, TResponse, TData> : IRequestHandler<TRequest, TResponse>
        where TRequest : ProjectRequestQuery<TData>, IRequest<TResponse>
        where TResponse : RequestResult<TData>, new()
    {
        protected ProjectDbContextFactory? ProjectNameDbContextFactory { get; init; }
        protected IProjectProvider? ProjectProvider { get; init; }
        protected ILogger Logger { get; init; }

        protected ProjectDbContext ProjectDbContext { get; set; }
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ProjectDbContextQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider? projectProvider, ILogger logger)
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            ProjectNameDbContextFactory = projectNameDbContextFactory ?? throw new ArgumentNullException(nameof(projectNameDbContextFactory));
            ProjectProvider = projectProvider ?? throw new ArgumentNullException(nameof(projectProvider));
            Logger = logger;
        }

        protected abstract Task<TResponse> GetDataAsync(TRequest request, CancellationToken cancellationToken);
       
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                ProjectDbContext = await ProjectNameDbContextFactory!.GetDatabaseContext(ProjectProvider?.CurrentProject?.ProjectName ?? string.Empty).ConfigureAwait(false);
                return await GetDataAsync(request, cancellationToken);
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


}
