
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Features.DashboardProjects;
using ClearDashboard.DataAccessLayer.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class GetDashboardProjectsQueryHandlerTest : TestBase
    {
        public GetDashboardProjectsQueryHandlerTest([NotNull] ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetDashboardProjectsTest()
        {
            var result = await ExecuteAndTestRequest<GetDashboardProjectQuery, RequestResult<ObservableCollection<DashboardProject>>, ObservableCollection<DashboardProject>>(new GetDashboardProjectQuery());
        }
    }
}
