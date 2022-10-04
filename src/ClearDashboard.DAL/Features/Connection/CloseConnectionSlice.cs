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
using ClearDashboard.DataAccessLayer.Features.Projects;
using Microsoft.EntityFrameworkCore;

namespace ClearDashboard.DataAccessLayer.Features.Connection
{
    public record CloseConnectionCommand(string ProjectName) : ProjectRequestCommand<bool>;

    public class CloseConnectionCommandHandler : ProjectDbContextCommandHandler<CloseConnectionCommand, RequestResult<bool>, bool>
    {
        public CloseConnectionCommandHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<CloseConnectionCommandHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            ProjectNameDbContextFactory = projectNameDbContextFactory ?? throw new ArgumentNullException(nameof(projectNameDbContextFactory));
            Logger = logger;
        }

        protected override async Task<RequestResult<bool>> SaveDataAsync(CloseConnectionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var projectAssets = await ProjectNameDbContextFactory.Get(request.ProjectName, true);


                if (projectAssets.ProjectDbContext != null)
                {
                    projectAssets.ProjectDbContext.Database.CloseConnectionAsync();

                }

                return new RequestResult<bool>(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected exception occurred while getting the database context for the project named '{request.ProjectName}'");
                throw;
            }
        }


    }
}
