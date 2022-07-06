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

    public class GetProjectInfoListQueryHandler : ProjectDbContextQueryHandler<GetProjectInfoListQuery, RequestResult<List<DataAccessLayer.Models.ProjectInfo>>, List<DataAccessLayer.Models.ProjectInfo>>
    {
        public GetProjectInfoListQueryHandler(ProjectDbContextFactory projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetProjectInfoListQueryHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
            //no-op
        }

        protected override async Task<RequestResult<List<DataAccessLayer.Models.ProjectInfo>>> GetDataAsync(GetProjectInfoListQuery requestQuery, CancellationToken cancellationToken)
        {
            var projects = ProjectDbContext.ProjectInfos.ToList();

            var result = new RequestResult<List<DataAccessLayer.Models.ProjectInfo>>
            {
                Success = true,
                Data = projects
            };

            return await Task.FromResult(result);
        }
    }

    public record GetProjectInfoQuery(Guid ProjectInfoId) : ProjectRequestQuery<DataAccessLayer.Models.ProjectInfo>;

    public class GetProjectInfoQueryHandler : ProjectDbContextQueryHandler<GetProjectInfoQuery, RequestResult<DataAccessLayer.Models.ProjectInfo>, DataAccessLayer.Models.ProjectInfo>
    {
        public GetProjectInfoQueryHandler(ProjectDbContextFactory projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetProjectInfoQueryHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
            //no-op
        }

        protected  override async Task<RequestResult<DataAccessLayer.Models.ProjectInfo>> GetDataAsync(GetProjectInfoQuery request, CancellationToken cancellationToken)
        {
            var project = ProjectDbContext.ProjectInfos.FirstOrDefault(project => project.Id == request.ProjectInfoId);

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

    public class AddProjectInfoCommandHandler : ProjectDbContextCommandHandler<AddProjectInfoCommand, RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>, IEnumerable<DataAccessLayer.Models.ProjectInfo>>
    {
        public AddProjectInfoCommandHandler(ProjectDbContextFactory projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<AddProjectInfoCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
            //no-op
        }

        protected override async Task<RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>> SaveDataAsync(AddProjectInfoCommand request, CancellationToken cancellationToken)
        {
            
            await ProjectDbContext.ProjectInfos.AddRangeAsync(request.ProjectInfos, cancellationToken);
            await ProjectDbContext.SaveChangesAsync(cancellationToken);
            return new RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>(request.ProjectInfos);
        }
    }

    public record UpdateProjectInfoCommand(IEnumerable<DataAccessLayer.Models.ProjectInfo> ProjectInfos) : ProjectRequestCommand<IEnumerable<DataAccessLayer.Models.ProjectInfo>>;

    public class UpdateProjectInfoCommandHandler : ProjectDbContextCommandHandler<UpdateProjectInfoCommand, RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>, IEnumerable<DataAccessLayer.Models.ProjectInfo>>
    {
        public UpdateProjectInfoCommandHandler(ProjectDbContextFactory projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<UpdateProjectInfoCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
            //no-op
        }

        protected override async Task<RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>> SaveDataAsync(UpdateProjectInfoCommand request, CancellationToken cancellationToken)
        {

            ProjectDbContext.ProjectInfos.UpdateRange(request.ProjectInfos);
            await ProjectDbContext.SaveChangesAsync(cancellationToken);
            return new RequestResult<IEnumerable<DataAccessLayer.Models.ProjectInfo>>(request.ProjectInfos);
        }
    }
}
