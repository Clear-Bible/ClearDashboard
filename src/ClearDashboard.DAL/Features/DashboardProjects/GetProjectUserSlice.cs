using Autofac;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.DashboardProjects
{
    public class GetProjectUserSlice
    {
        public record GetProjectUserQuery(string projectName) : IRequest<RequestResult<List<Models.User>>>;

        public class GetProjectUserQueryHandler : IRequestHandler<GetProjectUserQuery, RequestResult<List<Models.User>>>
        {
            private readonly IMediator _mediator;
            private readonly ILifetimeScope _lifetimeScope;
            public GetProjectUserQueryHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
                IProjectProvider projectProvider, ILogger<GetProjectUserQueryHandler> logger, ILifetimeScope lifetimeScope)
            {
                _lifetimeScope = lifetimeScope;
                _mediator = mediator;
            }

            public async Task<RequestResult<List<Models.User>>> Handle(GetProjectUserQuery request, CancellationToken cancellationToken)
            {
                using var requestScope = _lifetimeScope.BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

                var projectDbContextFactory = _lifetimeScope.Resolve<ProjectDbContextFactory>();
                var projectDbContext = await projectDbContextFactory.GetDatabaseContext(request.projectName,true,requestScope);

                var projectUsers = projectDbContext.Users.ToList();

                return new RequestResult<List<Models.User>>(projectUsers);
            }
        }
    }
}
