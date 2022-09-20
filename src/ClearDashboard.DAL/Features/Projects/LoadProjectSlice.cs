using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Data.Models;

namespace ClearDashboard.DataAccessLayer.Features.Projects
{
    public record LoadProjectQuery(string projectName) : LoadProjectQueryBase<IEnumerable<Models.Project>>;

    public class LoadProjectQueryHandler : LoadProjectQueryBaseHandler<LoadProjectQuery,
        RequestResult<IEnumerable<Models.Project>>, IEnumerable<Models.Project>>
    {
        private readonly IMediator _mediator;
        private ProjectAssets _projectAssets;
        private Microsoft.EntityFrameworkCore.DbSet<Models.Project> _projects;
        public LoadProjectQueryHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider, ILogger<LoadProjectQueryHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IEnumerable<Models.Project>>> GetDataAsync(LoadProjectQuery request, CancellationToken cancellationToken)
        {
            var projectAssets = await ProjectNameDbContextFactory.Get(request.projectName);
            return new RequestResult<IEnumerable<Models.Project>>(projectAssets.ProjectDbContext.Projects);
        }
    }
}
