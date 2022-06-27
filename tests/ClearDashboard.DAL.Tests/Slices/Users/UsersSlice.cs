using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Features;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Tests.Slices.Users
{
    public record GetUsersQuery(string Project, string TestDummy) : ProjectRequestQuery<List<User>>(Project);

    public class GetUsersQueryHandler : AlignmentDbContextRequestHandler<GetUsersQuery, RequestResult<List<User>>, List<User> >
    {
        public GetUsersQueryHandler(ProjectNameDbContextFactory projectNameDbContextFactory, ILogger<GetUsersQueryHandler> logger) : base(projectNameDbContextFactory, logger)
        {
           //no-op
        }

        protected override async Task<RequestResult<List<User>>> GetData(ProjectRequestQuery<List<User>> request)
        {
            var users = AlignmentContext.Users.ToList();

            var result = new RequestResult<List<User>>
            {
                Success = true,
                Data = users
            };

            return await Task.FromResult(result);
        }
    }
}
