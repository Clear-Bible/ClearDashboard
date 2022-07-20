using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class GetAllProjectsQueryHandlerTest : TestBase
    {
#nullable disable
        public GetAllProjectsQueryHandlerTest(ITestOutputHelper output) : base(output)
        {
            //no-op
        }


        [Fact]
        public async Task GetAllProjects()
        {
            var results =
                await ExecuteParatextAndTestRequest<GetAllProjectsQuery, RequestResult<List<ParatextProject>>,
                    List<ParatextProject>>(new GetAllProjectsQuery());
            Assert.NotEmpty(results.Data);
            Assert.True(results.HasData);
            Assert.True(results.Success);
        }
    }
}
