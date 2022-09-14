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
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.Projects
{
    public record DeleteProjectCommand(string projectName) : ProjectRequestCommand<Models.Project>;

    public class DeleteProjectCommandHandler : ProjectDbContextCommandHandler<DeleteProjectCommand,
        RequestResult<Models.Project>, Models.Project>
    {
        private readonly IMediator _mediator;
        public DeleteProjectCommandHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider, ILogger<DeleteProjectCommandHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Models.Project>> SaveDataAsync(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var projectAssets = await ProjectNameDbContextFactory.Get(request.projectName);

                if (projectAssets.ProjectDbContext != null)
                {
                    var project = projectAssets.ProjectDbContext.Projects.FirstOrDefault();

                    //projectAssets.ProjectDbContext!.Database.EnsureDeleted();
                    await projectAssets.ProjectDbContext!.Database.EnsureDeletedAsync();
                    await projectAssets.ProjectDbContext!.SaveChangesAsync();


                    if (Directory.Exists(projectAssets.ProjectDirectory))
                    {
                        Directory.Delete(projectAssets.ProjectDirectory, true);
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
