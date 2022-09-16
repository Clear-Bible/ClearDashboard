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
    public record CreateProjectCommand(string ProjectName) : ProjectRequestCommand<Models.Project>;

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
                        await projectAssets.ProjectDbContext.Projects.AddAsync(project);
                        await projectAssets.ProjectDbContext.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        //var projects = projectAssets.ProjectDbContext.Projects.ToList() ?? throw new ArgumentNullException("projectAssets.ProjectDbContext.Projects.ToList()");
                        //projects.Add(project);
                        await projectAssets.ProjectDbContext.Projects.AddAsync(project);
                        await projectAssets.ProjectDbContext.SaveChangesAsync();
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
