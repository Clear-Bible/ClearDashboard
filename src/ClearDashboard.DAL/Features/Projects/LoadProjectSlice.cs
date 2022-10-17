using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autofac;
using Autofac.Core.Lifetime;
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
    public record LoadProjectQuery(string projectName) :  IRequest<RequestResult<Models.Project>>;

    public class LoadProjectQueryHandler : IRequestHandler<LoadProjectQuery,RequestResult<Models.Project>>
    {
        private readonly IMediator _mediator;
        private readonly ILifetimeScope _lifetimeScope;
        public LoadProjectQueryHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider, ILogger<LoadProjectQueryHandler> logger, ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
            _mediator = mediator;
        }

        public async Task<RequestResult<Models.Project>> Handle(LoadProjectQuery request, CancellationToken cancellationToken)
        {
            using var requestScope = _lifetimeScope.BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            var projectDbContextFactory = _lifetimeScope.Resolve<ProjectDbContextFactory>();
            var projectDbContext = await projectDbContextFactory.GetDatabaseContext(
                request.projectName,
                false,
                requestScope);

            var currentProject = projectDbContext.Projects.First();

            return new RequestResult<Models.Project>(currentProject);
        }
    }
}
