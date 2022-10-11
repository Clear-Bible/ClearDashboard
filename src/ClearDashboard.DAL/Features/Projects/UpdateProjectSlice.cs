using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Lifetime;

namespace ClearDashboard.DataAccessLayer.Features.Projects
{
    public record UpdateProjectCommand(Models.Project project) : ProjectRequestCommand<Models.Project>;

    public class UpdateProjectCommandHandler : ProjectDbContextCommandHandler<UpdateProjectCommand, RequestResult<Models.Project>, Models.Project>
    {
        protected ILifetimeScope? LifetimeScope { get; set; }
        public UpdateProjectCommandHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<UpdateProjectCommandHandler> logger, ILifetimeScope lifetimeScope)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            ProjectNameDbContextFactory = projectNameDbContextFactory ?? throw new ArgumentNullException(nameof(projectNameDbContextFactory));
            Logger = logger;
            LifetimeScope = lifetimeScope;
        }

        protected override async Task<RequestResult<Models.Project>> SaveDataAsync(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            await using var requestScope = LifetimeScope.BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            var projectDbContextFactory = LifetimeScope.Resolve<ProjectDbContextFactory>();
            var projectDbContext = await projectDbContextFactory.GetDatabaseContext(request.project.ProjectName, false, requestScope);

            Logger.LogInformation($"Saving the design surface layout for project '{request.project.ProjectName}'");
            var updatedProject = projectDbContext.Update(request.project);

            await projectDbContext.SaveChangesAsync();

            Logger.LogInformation($"Saved the design surface layout for project '{request.project.ProjectName}'");

            return new RequestResult<Models.Project>(updatedProject.Entity);
        }
    }
}
