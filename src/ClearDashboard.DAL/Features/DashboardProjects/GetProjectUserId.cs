using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.DashboardProjects
{
    public record GetProjectUserIdQuery(string DatabasePath) : IRequest<RequestResult<string>>;

    public class GetProjectUserIdQueryHandler : SqliteDatabaseRequestHandler<GetProjectUserIdQuery, RequestResult<string>, string>
    {
        public GetProjectUserIdQueryHandler(ILogger<LastMergedCommitShaQueryHandler> logger) : base(logger)
        {
            //no-op
        }


        protected override string ResourceName { get; set; }

        public override Task<RequestResult<string>> Handle(GetProjectUserIdQuery request, CancellationToken cancellationToken)
        {
            FileInfo fi = new FileInfo(request.DatabasePath);

            ResourceDirectory = fi.DirectoryName;
            ResourceName = fi.Name;


            var queryResult = ValidateResourcePath(string.Empty);
            if (queryResult.Success)
            {
                try
                {
                    queryResult.Data = ExecuteSqliteCommandAndProcessData($"SELECT UserId FROM PROJECT LIMIT 1");
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
            string sha = "";
            while (DataReader != null && DataReader.Read())
            {
                if (!DataReader.IsDBNull(0))
                {
                    sha = DataReader.GetString(0);
                }
            }
            return sha;
        }
    }

}
