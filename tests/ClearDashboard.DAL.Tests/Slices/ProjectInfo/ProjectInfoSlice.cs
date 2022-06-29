using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Features;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Tests.Slices.ProjectInfo
{
    public record GetProjectInfoQuery(string Project) : ProjectRequestQuery<List<DataAccessLayer.Models.ProjectInfo>>(Project);

    public class GetProjectInfoQueryHandler : AlignmentDbContextQueryHandler<GetProjectInfoQuery, RequestResult<List<DataAccessLayer.Models.ProjectInfo>>, List<DataAccessLayer.Models.ProjectInfo>>
    {
        public GetProjectInfoQueryHandler(ProjectNameDbContextFactory projectNameDbContextFactory, ILogger<GetProjectInfoQueryHandler> logger) : base(projectNameDbContextFactory, logger)
        {
            //no-op
        }

        protected override async Task<RequestResult<List<DataAccessLayer.Models.ProjectInfo>>> GetData(ProjectRequestQuery<List<DataAccessLayer.Models.ProjectInfo>> requestQuery)
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


    public record AddProjectInfoCommand(string Project, IEnumerable<DataAccessLayer.Models.ProjectInfo> ProjectInfos) : ProjectRequestCommand<IEnumerable<DataAccessLayer.Models.ProjectInfo>>(Project);

    public class AddProjectInfoCommandHandler : AlignmentDbContextCommandHandler<AddProjectInfoCommand, RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>, IEnumerable<DataAccessLayer.Models.ProjectInfo>>
    {
        public AddProjectInfoCommandHandler(ProjectNameDbContextFactory projectNameDbContextFactory, ILogger<GetProjectInfoQueryHandler> logger) : base(projectNameDbContextFactory, logger)
        {
            //no-op
        }

        protected override async Task<RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>> SaveData(AddProjectInfoCommand request, CancellationToken cancellationToken)
        {
            
            await AlignmentContext.ProjectInfos.AddRangeAsync(request.ProjectInfos, cancellationToken);
            await AlignmentContext.SaveChangesAsync(cancellationToken);
            return new RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>(request.ProjectInfos);
        }
    }

    public record UpdateProjectInfoCommand(string Project, IEnumerable<DataAccessLayer.Models.ProjectInfo> ProjectInfos) : ProjectRequestCommand<IEnumerable<DataAccessLayer.Models.ProjectInfo>>(Project);

    public class UpdateProjectInfoCommandHandler : AlignmentDbContextCommandHandler<UpdateProjectInfoCommand, RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>, IEnumerable<DataAccessLayer.Models.ProjectInfo>>
    {
        public UpdateProjectInfoCommandHandler(ProjectNameDbContextFactory projectNameDbContextFactory, ILogger<GetProjectInfoQueryHandler> logger) : base(projectNameDbContextFactory, logger)
        {
            //no-op
        }

        protected override async Task<RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>> SaveData(UpdateProjectInfoCommand request, CancellationToken cancellationToken)
        {

            AlignmentContext.ProjectInfos.UpdateRange(request.ProjectInfos);
            await AlignmentContext.SaveChangesAsync(cancellationToken);
            return new RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>(request.ProjectInfos);
        }
    }
}
