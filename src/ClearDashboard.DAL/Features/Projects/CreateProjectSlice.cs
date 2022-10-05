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
        public CreateProjectCommandHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<CreateProjectCommandHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            ProjectNameDbContextFactory = projectNameDbContextFactory ?? throw new ArgumentNullException(nameof(projectNameDbContextFactory));
            Logger = logger;
        }

        protected  override async Task<RequestResult<Models.Project>> SaveDataAsync(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var project = new Models.Project()
                {
                    ProjectName = ProjectDbContext.DatabaseName
                };

                await ProjectDbContext.Migrate();

                await ProjectDbContext.Users.AddAsync(request.User, cancellationToken);
                await ProjectDbContext.Projects.AddAsync(project, cancellationToken);
                await ProjectDbContext.SaveChangesAsync(cancellationToken);

                return new RequestResult<Models.Project>(project);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected exception occurred while getting the database context for the project named '{request.ProjectName}'");
                throw;
            }
        }

       
    }
}
