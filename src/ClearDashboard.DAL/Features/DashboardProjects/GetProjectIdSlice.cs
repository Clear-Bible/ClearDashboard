using System;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.DashboardProjects
{
    public class GetProjectIdSlice
    {


        public record GetProjectIdQuery(string DatabasePath) : IRequest<RequestResult<string>>;

        public class
            GetProjectIdQueryHandler : SqliteDatabaseRequestHandler<GetProjectIdQuery, RequestResult<string>,
                string>
        {
            public GetProjectIdQueryHandler(ILogger<GetProjectIdQueryHandler> logger) : base(logger)
            {
                //no-op
            }


            protected override string ResourceName { get; set; }

            public override Task<RequestResult<string>> Handle(GetProjectIdQuery request,
                CancellationToken cancellationToken)
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
                            $"SELECT Id FROM PROJECT LIMIT 1");
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
                var appVersion = Guid.Empty.ToString();
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
    }
}
