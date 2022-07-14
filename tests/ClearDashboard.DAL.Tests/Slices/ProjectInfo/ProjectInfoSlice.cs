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
    public record GetProjectInfoListQuery : ProjectRequestQuery<List<DataAccessLayer.Models.Project>>;

    public class GetProjectInfoListQueryHandler : ProjectDbContextQueryHandler<GetProjectInfoListQuery, RequestResult<List<DataAccessLayer.Models.Project>>, List<DataAccessLayer.Models.Project>>
    {
        public GetProjectInfoListQueryHandler(ProjectDbContextFactory projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetProjectInfoListQueryHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
            //no-op
        }

        protected override async Task<RequestResult<List<DataAccessLayer.Models.Project>>> GetDataAsync(GetProjectInfoListQuery requestQuery, CancellationToken cancellationToken)
        {
            var projects = ProjectDbContext.Projects.ToList();

            var result = new RequestResult<List<DataAccessLayer.Models.Project>>
            {
                Success = true,
                Data = projects
            };

            return await Task.FromResult(result);
        }
    }

    public record GetProjectInfoQuery(Guid ProjectInfoId) : ProjectRequestQuery<DataAccessLayer.Models.Project>;

    public class GetProjectInfoQueryHandler : ProjectDbContextQueryHandler<GetProjectInfoQuery, RequestResult<DataAccessLayer.Models.Project>, DataAccessLayer.Models.Project>
    {
        public GetProjectInfoQueryHandler(ProjectDbContextFactory projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetProjectInfoQueryHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
            //no-op
        }

        protected  override async Task<RequestResult<DataAccessLayer.Models.Project>> GetDataAsync(GetProjectInfoQuery request, CancellationToken cancellationToken)
        {
            var project = ProjectDbContext.Projects.FirstOrDefault(project => project.Id == request.ProjectInfoId);

            if (project == null)
            {
                var result = new RequestResult<DataAccessLayer.Models.Project>
                {
                    Message = $"Could not find a Project with Id '{request.ProjectInfoId}",
                    Success = false
                };
                return await Task.FromResult(result);
            }
            else
            {
                var result = new RequestResult<DataAccessLayer.Models.Project>
                {
                    Success = true,
                    Data = project
                };

                return await Task.FromResult(result);
            }

        }
    }
    


    public record AddProjectInfoCommand(IEnumerable<DataAccessLayer.Models.Project> ProjectInfos) : ProjectRequestCommand<IEnumerable<DataAccessLayer.Models.Project>>;

    public class AddProjectInfoCommandHandler : ProjectDbContextCommandHandler<AddProjectInfoCommand, RequestResult<IEnumerable<DataAccessLayer.Models.Project>>, IEnumerable<DataAccessLayer.Models.Project>>
    {
        public AddProjectInfoCommandHandler(ProjectDbContextFactory projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<AddProjectInfoCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
            //no-op
        }

        protected override async Task<RequestResult<IEnumerable<DataAccessLayer.Models.Project>>> SaveDataAsync(AddProjectInfoCommand request, CancellationToken cancellationToken)
        {
            
            await ProjectDbContext.Projects.AddRangeAsync(request.ProjectInfos, cancellationToken);
            await ProjectDbContext.SaveChangesAsync(cancellationToken);
            return new RequestResult<IEnumerable<DataAccessLayer.Models.Project>>(request.ProjectInfos);
        }
    }

    public record UpdateProjectInfoCommand(IEnumerable<DataAccessLayer.Models.Project> ProjectInfos) : ProjectRequestCommand<IEnumerable<DataAccessLayer.Models.Project>>;

    public class UpdateProjectInfoCommandHandler : ProjectDbContextCommandHandler<UpdateProjectInfoCommand, RequestResult<IEnumerable<DataAccessLayer.Models.Project>>, IEnumerable<DataAccessLayer.Models.Project>>
    {
        public UpdateProjectInfoCommandHandler(ProjectDbContextFactory projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<UpdateProjectInfoCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
            //no-op
        }

        protected override async Task<RequestResult<IEnumerable<DataAccessLayer.Models.Project>>> SaveDataAsync(UpdateProjectInfoCommand request, CancellationToken cancellationToken)
        {

            ProjectDbContext.Projects.UpdateRange(request.ProjectInfos);
            await ProjectDbContext.SaveChangesAsync(cancellationToken);
            return new RequestResult<IEnumerable<DataAccessLayer.Models.Project>>(request.ProjectInfos);
        }
    }
}
