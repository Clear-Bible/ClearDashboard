using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Interfaces;

namespace ClearDashboard.DataAccessLayer.Features.Projects
{
    public record CreateProjectCommand(string ProjectName, Models.User User) : ProjectRequestCommand<Models.Project>;

    public class CreateProjectCommandHandler : ProjectDbContextCommandHandler<CreateProjectCommand, RequestResult<Models.Project>, Models.Project>
    {
        public CreateProjectCommandHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<CreateProjectCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
            ProjectNameDbContextFactory = projectNameDbContextFactory ?? throw new ArgumentNullException(nameof(projectNameDbContextFactory));
            Logger = logger;
        }

        protected  override async Task<RequestResult<Models.Project>> SaveDataAsync(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var projectAssets = await ProjectNameDbContextFactory.Get(request.ProjectName);


                if (projectAssets.ProjectDbContext != null)
                {
                    var project = new Models.Project()
                    {
                        ProjectName = request.ProjectName
                    };

                    try
                    {
                        await projectAssets.ProjectDbContext.Users.AddAsync(request.User, cancellationToken);
                        await projectAssets.ProjectDbContext.Projects.AddAsync(project, cancellationToken);
                        await projectAssets.ProjectDbContext.SaveChangesAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "An unexpected error occurred while creating a new project. Attempting to save one last time.");
                       
                        await projectAssets.ProjectDbContext.Users.AddAsync(request.User, cancellationToken);
                        await projectAssets.ProjectDbContext.Projects.AddAsync(project, cancellationToken);
                        await projectAssets.ProjectDbContext.SaveChangesAsync(cancellationToken);
                    }

                    return new RequestResult<Models.Project>(project);
                }
                throw new NullReferenceException($"The 'ProjectDbContext' for the project {request.ProjectName} could not be created.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected exception occurred while getting the database context for the project named '{request.ProjectName}'");
                throw;
            }
        }

       
    }
}
