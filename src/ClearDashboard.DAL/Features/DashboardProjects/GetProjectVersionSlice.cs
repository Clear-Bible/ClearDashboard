using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.DashboardProjects
{
    public class GetProjectVersionSlice
    {
        public record GetProjectVersionQuery(string DatabasePath) : IRequest<RequestResult<string>>;

        public class GetProjectVersionQueryHandler : SqliteDatabaseRequestHandler<GetProjectVersionQuery, RequestResult<string>, string>
        {
            public GetProjectVersionQueryHandler(ILogger<GetProjectVersionQueryHandler> logger) : base(logger)
            {
                //no-op
            }


            protected override string ResourceName { get; set; } 

            public override Task<RequestResult<string>> Handle(GetProjectVersionQuery request, CancellationToken cancellationToken)
            {
                FileInfo fi = new FileInfo(request.DatabasePath);

                ResourceDirectory = fi.DirectoryName;
                ResourceName = fi.Name;


                var queryResult = ValidateResourcePath(string.Empty);
                if (queryResult.Success)
                {
                    try
                    {
                        queryResult.Data = ExecuteSqliteCommandAndProcessData(
                            $"SELECT AppVersion FROM PROJECT LIMIT 1");
                    }
                    catch
                    {
                        queryResult.Success = false;
                    }
                }
                return Task.FromResult(queryResult);
            }

            protected override string ProcessData()
            {
                string appVersion = "unknown";
                while (DataReader != null && DataReader.Read())
                {
                    if (!DataReader.IsDBNull(0))
                    {
                        appVersion = DataReader.GetString(0);
                    }
                }
                return appVersion;
            }
        }




        //public record GetProjectVersionQuery(string projectName) : ProjectRequestQuery<RequestResult<IEnumerable<string>>>;

        //public class GetProjectVersionQueryHandler : ProjectDbContextQueryHandler<GetProjectVersionQuery,
        //    RequestResult<IEnumerable<string>>, IEnumerable<string>>
        //{
        //    private readonly IMediator _mediator;
        //    public GetProjectVersionQueryHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
        //        IProjectProvider projectProvider, ILogger<GetProjectVersionQueryHandler> logger)
        //        : base(projectNameDbContextFactory, projectProvider, logger)
        //    {
        //        _mediator = mediator;
        //    }

        //    protected override async Task<RequestResult<IEnumerable<string>>> GetDataAsync(
        //        GetProjectVersionQuery request, CancellationToken cancellationToken)
        //    {
        //        // need an await to get the compiler to be 'quiet'
        //        await Task.CompletedTask;

        //        using (var requestScope = ProjectNameDbContextFactory!.ServiceScope.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
        //        {
        //            //var requestScope = ProjectNameDbContextFactory!.ServiceScope.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
        //            //using (var ProjectDbContext =
        //            //       ProjectNameDbContextFactory!.GetDatabaseContext(DatabasePath ?? string.Empty,
        //            //           false, requestScope))
        //            //{

        //            //    try
        //            //    {
        //            //        version = ProjectDbContext.Result.Projects.FirstOrDefault().AppVersion;
        //            //    }
        //            //    catch (Exception ex)
        //            //    {
        //            //        Logger.LogError(ex, "Unable to obtain project version number.");
        //            //    }
        //            //}
        //        }

        //        return new RequestResult<IEnumerable<string>>();
        //    }
        //}
    }
}
