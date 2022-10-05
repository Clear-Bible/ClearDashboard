using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Core;
using Autofac;
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
                var project = ProjectDbContext.Projects.FirstOrDefault();

                //projectAssets.ProjectDbContext!.Database.EnsureDeleted();
                await ProjectDbContext!.Database.EnsureDeletedAsync();
                await ProjectDbContext!.SaveChangesAsync(cancellationToken);

                if (ProjectDbContext.OptionsBuilder.GetType() == typeof(SqliteProjectDbContextOptionsBuilder))
                {
                    var databaseDirectory = (ProjectDbContext.OptionsBuilder as SqliteProjectDbContextOptionsBuilder)!.DatabaseDirectory;
                    if (Directory.Exists(databaseDirectory))
                    {
                        Directory.Delete(databaseDirectory, true);
                    }
                }

                return new RequestResult<Models.Project>(project);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected exception occurred while getting the database context for the project named '{request.projectName}'");
                throw;
            }
        }
    }
}
