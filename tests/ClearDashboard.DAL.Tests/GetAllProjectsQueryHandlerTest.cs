using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects;
using Paratext.PluginInterfaces;
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
            var results = await ExecuteParatextAndTestRequest<GetAllProjectsQuery, RequestResult<List<IProject>>, List<IProject>>(new GetAllProjectsQuery());
            Assert.NotNull(results);
        }
    }
}
