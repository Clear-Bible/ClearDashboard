using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.CQRS.Features.Features
{
    public abstract record ProjectRequestQuery<TData>(string ProjectName) : IRequest<RequestResult<TData>>;
    
    public abstract class AlignmentDbContextQueryHandler<TRequest, TResponse, TData> : IRequestHandler<TRequest, TResponse>
        where TRequest : ProjectRequestQuery<TData>, IRequest<TResponse>
        where TResponse : RequestResult<TData>, new()
    {
        protected ProjectNameDbContextFactory? ProjectNameDbContextFactory { get; init; }
        protected ILogger Logger { get; init; }

        protected AlignmentContext AlignmentContext { get; set; }
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected AlignmentDbContextQueryHandler(ProjectNameDbContextFactory? projectNameDbContextFactory, ILogger logger)
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            ProjectNameDbContextFactory = projectNameDbContextFactory ?? throw new ArgumentNullException(nameof(projectNameDbContextFactory));
            Logger = logger;
        }

        protected abstract Task<TResponse> GetData(TRequest request, CancellationToken cancellationToken);
       
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                AlignmentContext = await ProjectNameDbContextFactory!.GetDatabaseContext(request.ProjectName).ConfigureAwait(false);
                return await GetData(request, cancellationToken);
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
