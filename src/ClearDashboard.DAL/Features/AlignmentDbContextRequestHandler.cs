using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features
{
    public abstract record ProjectRequestQuery<TData>(string Project) : IRequest<RequestResult<TData>>;
    
    public abstract class AlignmentDbContextRequestHandler<TRequest, TResponse, TData> : IRequestHandler<TRequest, TResponse>
        where TRequest : ProjectRequestQuery<TData>, IRequest<TResponse>
        where TResponse : RequestResult<TData>
    {
        protected ProjectNameDbContextFactory ProjectNameDbContextFactory { get; init; }
        protected ILogger Logger { get; init; }

        protected AlignmentContext AlignmentContext { get; set; }
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected AlignmentDbContextRequestHandler(ProjectNameDbContextFactory projectNameDbContextFactory, ILogger logger)
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            ProjectNameDbContextFactory = projectNameDbContextFactory;
            Logger = logger;
        }

        protected abstract Task<TResponse> GetData(ProjectRequestQuery<TData> request);
       
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var typedRequest = (ProjectRequestQuery<TData>)request;
            AlignmentContext = await ProjectNameDbContextFactory.GetDatabaseContext(request.Project).ConfigureAwait(false);
            return await GetData(request);
        }
    }
}
