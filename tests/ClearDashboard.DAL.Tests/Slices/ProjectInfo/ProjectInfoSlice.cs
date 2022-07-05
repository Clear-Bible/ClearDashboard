using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Interfaces;

namespace ClearDashboard.DAL.Tests.Slices.ProjectInfo
{
    public record GetProjectInfoListQuery : ProjectRequestQuery<List<DataAccessLayer.Models.ProjectInfo>>;

    public class GetProjectInfoListQueryHandler : AlignmentDbContextQueryHandler<GetProjectInfoListQuery, RequestResult<List<DataAccessLayer.Models.ProjectInfo>>, List<DataAccessLayer.Models.ProjectInfo>>
    {
        public GetProjectInfoListQueryHandler(ProjectNameDbContextFactory projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetProjectInfoListQueryHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
            //no-op
        }

        protected override async Task<RequestResult<List<DataAccessLayer.Models.ProjectInfo>>> GetData(GetProjectInfoListQuery requestQuery, CancellationToken cancellationToken)
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

    public record GetProjectInfoQuery(Guid ProjectInfoId) : ProjectRequestQuery<DataAccessLayer.Models.ProjectInfo>;

    public class GetProjectInfoQueryHandler : AlignmentDbContextQueryHandler<GetProjectInfoQuery, RequestResult<DataAccessLayer.Models.ProjectInfo>, DataAccessLayer.Models.ProjectInfo>
    {
        public GetProjectInfoQueryHandler(ProjectNameDbContextFactory projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetProjectInfoQueryHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
            //no-op
        }

        protected  override async Task<RequestResult<DataAccessLayer.Models.ProjectInfo>> GetData(GetProjectInfoQuery request, CancellationToken cancellationToken)
        {
            var project = AlignmentContext.ProjectInfos.FirstOrDefault(project => project.Id == request.ProjectInfoId);

            if (project == null)
            {
                var result = new RequestResult<DataAccessLayer.Models.ProjectInfo>
                {
                    Message = $"Could not find a ProjectInfo with Id '{request.ProjectInfoId}",
                    Success = false
                };
                return await Task.FromResult(result);
            }
            else
            {
                var result = new RequestResult<DataAccessLayer.Models.ProjectInfo>
                {
                    Success = true,
                    Data = project
                };

                return await Task.FromResult(result);
            }

        }
    }
    


    public record AddProjectInfoCommand(IEnumerable<DataAccessLayer.Models.ProjectInfo> ProjectInfos) : ProjectRequestCommand<IEnumerable<DataAccessLayer.Models.ProjectInfo>>;

    public class AddProjectInfoCommandHandler : AlignmentDbContextCommandHandler<AddProjectInfoCommand, RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>, IEnumerable<DataAccessLayer.Models.ProjectInfo>>
    {
        public AddProjectInfoCommandHandler(ProjectNameDbContextFactory projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<AddProjectInfoCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
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

    public record UpdateProjectInfoCommand(IEnumerable<DataAccessLayer.Models.ProjectInfo> ProjectInfos) : ProjectRequestCommand<IEnumerable<DataAccessLayer.Models.ProjectInfo>>;

    public class UpdateProjectInfoCommandHandler : AlignmentDbContextCommandHandler<UpdateProjectInfoCommand, RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>, IEnumerable<DataAccessLayer.Models.ProjectInfo>>
    {
        public UpdateProjectInfoCommandHandler(ProjectNameDbContextFactory projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<UpdateProjectInfoCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
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
