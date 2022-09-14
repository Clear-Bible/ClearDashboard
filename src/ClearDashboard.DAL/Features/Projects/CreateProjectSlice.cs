using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.Projects
{
    public record CreateProjectCommand(string projectName) : ProjectRequestCommand<Models.Project>;

    public class CreateProjectCommandHandler : ProjectDbContextCommandHandler<CreateProjectCommand,
        RequestResult<Models.Project>, Models.Project>
    {
        private readonly IMediator _mediator;
        public CreateProjectCommandHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider, ILogger<CreateProjectCommandHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Models.Project>> SaveDataAsync(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var projectAssets = await ProjectNameDbContextFactory.Get(request.projectName);


                if (projectAssets.ProjectDbContext != null)
                {
                    var project = new Models.Project()
                    {
                        ProjectName = request.projectName
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
                throw new NullReferenceException($"The 'ProjectDbContext' for the project {request.projectName} could not be created.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected exception occurred while getting the database context for the project named '{request.projectName}'");
                throw;
            }
        }
    }
}
