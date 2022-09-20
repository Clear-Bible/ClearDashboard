using System;
using System.Collections.Generic;
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
using SIL.Extensions;

namespace ClearDashboard.DataAccessLayer.Features.Projects
{
    public record GetAllProjectsFromDatabaseQuery() : LoadProjectQueryBase<IEnumerable<Models.Project>>;

    public class GetAllProjectsFromDatabaseQueryHandler : LoadProjectQueryBaseHandler<GetAllProjectsFromDatabaseQuery,
        RequestResult<IEnumerable<Models.Project>>, IEnumerable<Models.Project>>
    {
        private readonly IMediator _mediator;
        public GetAllProjectsFromDatabaseQueryHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider, ILogger<GetAllProjectsFromDatabaseQueryHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IEnumerable<Models.Project>>> GetDataAsync(GetAllProjectsFromDatabaseQuery request, CancellationToken cancellationToken)
        {
            var projects = ProjectNameDbContextFactory.ProjectAssets.ProjectDbContext.Projects;
            return (RequestResult<IEnumerable<Models.Project>>)projects.ToEnumerable(); 
        }
    }
}
