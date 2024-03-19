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

        public record UpdateLabelGroupHyphenQuery(DashboardProject Project) : IRequest<RequestResult<string>>;

        public class UpdateLabelGroupHyphenQueryHandler : SqliteDatabaseRequestHandler<UpdateLabelGroupHyphenQuery, RequestResult<string>, string>
        {
            public UpdateLabelGroupHyphenQueryHandler(ILogger<UpdateLabelGroupHyphenQueryHandler> logger) : base(logger)
            {
                //no-op
            }

            protected override string ResourceName { get; set; }

            public override Task<RequestResult<string>> Handle(UpdateLabelGroupHyphenQuery request, CancellationToken cancellationToken)
            {
                FileInfo fi = new FileInfo(request.Project.FullFilePath);

                ResourceDirectory = fi.DirectoryName;
                ResourceName = fi.Name;

                var queryResult = ValidateResourcePath(string.Empty);
                if (queryResult.Success)
                {
                    try
                    {
                        queryResult.Data = ExecuteSqliteCommandAndProcessData($"UPDATE LABELGROUP SET NAME=REPLACE(NAME,'_','-')");
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
