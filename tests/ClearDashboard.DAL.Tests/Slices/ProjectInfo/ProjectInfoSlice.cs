using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Features;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Tests.Slices.ProjectInfo
{
    public record GetProjectInfoQuery(string Project) : ProjectRequestQuery<List<DataAccessLayer.Models.ProjectInfo>>(Project);

    public class GetProjectInfoQueryHandler : AlignmentDbContextRequestHandler<GetProjectInfoQuery, RequestResult<List<DataAccessLayer.Models.ProjectInfo>>, List<DataAccessLayer.Models.ProjectInfo>>
    {
        public GetProjectInfoQueryHandler(ProjectNameDbContextFactory projectNameDbContextFactory, ILogger<GetProjectInfoQueryHandler> logger) : base(projectNameDbContextFactory, logger)
        {
            //no-op
        }

        protected override async Task<RequestResult<List<DataAccessLayer.Models.ProjectInfo>>> GetData(ProjectRequestQuery<List<DataAccessLayer.Models.ProjectInfo>> request)
        {
            var projects = AlignmentContext.ProjectInfos.ToList();

            var result = new RequestResult<List<DataAccessLayer.Models.ProjectInfo>>
            {
                Success = true,
                Data = projects
            };

            return await Task.FromResult(result);
        }
    }
}
