using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.DashboardProjects
{
    public record UpdateDatabaseHyphenQuery(DashboardProject Project) : IRequest<RequestResult<string>>;

    public class UpdateDatabaseHyphenQueryHandler : SqliteDatabaseRequestHandler<UpdateDatabaseHyphenQuery, RequestResult<string>, string>
    {
        public UpdateDatabaseHyphenQueryHandler(ILogger<UpdateDatabaseHyphenQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        protected override string ResourceName { get; set; }

        public override Task<RequestResult<string>> Handle(UpdateDatabaseHyphenQuery request, CancellationToken cancellationToken)
        {
            var fi = new FileInfo(request.Project.FullFilePath);

            ResourceDirectory = fi.DirectoryName!;
            ResourceName = fi.Name;

            var queryResult = ValidateResourcePath(string.Empty);
            if (queryResult.Success)
            {
                try
                {
                    // fix the hyphens in the LABELGROUP table
                    queryResult.Data = ExecuteSqliteCommandAndProcessData($"UPDATE LABELGROUP SET NAME=REPLACE(NAME,'_','-')");
                }
                catch
                {
                    queryResult.Success = false;
                }

                try
                {
                    // fix the hyphens in the LABEL table
                    queryResult.Data = ExecuteSqliteCommandAndProcessData($"UPDATE LABEL SET TEXT=REPLACE(TEXT,'_','-')");
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
            return "";
        }
    }

}
