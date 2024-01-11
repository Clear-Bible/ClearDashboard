using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.CQRS;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.DashboardProjects
{
    public record ResetProjectGitLabShaQuery(string DatabasePath) : IRequest<RequestResult<string>>;

    public class ResetProjectShaQueryHandler : SqliteDatabaseRequestHandler<ResetProjectGitLabShaQuery, RequestResult<string>, string>
    {
        public ResetProjectShaQueryHandler(ILogger<ResetProjectShaQueryHandler> logger) : base(logger)
        {
            //no-op
        }


        protected override string ResourceName { get; set; }

        public override Task<RequestResult<string>> Handle(ResetProjectGitLabShaQuery request, CancellationToken cancellationToken)
        {
            FileInfo fi = new FileInfo(request.DatabasePath);

            ResourceDirectory = fi.DirectoryName;
            ResourceName = fi.Name;


            var queryResult = ValidateResourcePath(string.Empty);
            if (queryResult.Success)
            {
                try
                {
                    // project is no longer a collab project so remove the sha
                    queryResult.Data = ExecuteSqliteCommandAndProcessData($"UPDATE PROJECT SET LastMergedCommitSha=''");
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
