using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Interfaces;

namespace ClearDashboard.DAL.Tests.Slices.Users
{
    public record GetUsersQuery(string TestDummy) : ProjectRequestQuery<List<User>>;

    public class GetUsersQueryHandler : ProjectDbContextQueryHandler<GetUsersQuery, RequestResult<List<User>>, List<User> >
    {
        public GetUsersQueryHandler(ProjectDbContextFactory projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetUsersQueryHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
           //no-op
        }

        protected override async Task<RequestResult<List<User>>> GetDataAsync(GetUsersQuery requestQuery, CancellationToken cancellationToken)
        {
            var users = ProjectDbContext.Users.ToList();

            var result = new RequestResult<List<User>>
            {
                Success = true,
                Data = users
            };

            return await Task.FromResult(result);
        }
    }
}
