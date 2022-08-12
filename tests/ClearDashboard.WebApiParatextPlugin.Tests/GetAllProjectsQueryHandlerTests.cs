using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects;
using Paratext.PluginInterfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class GetAllProjectsQueryHandlerTests : TestBase
    {
        public GetAllProjectsQueryHandlerTests(ITestOutputHelper output) : base(output)
        {
        }


        [Fact]
        public async Task GetAllProjectsTestAsync()
        {
            try
            {
                await StartParatextAsync();
                var client = CreateHttpClient();

                var response =
                    await client.PostAsJsonAsync<GetAllProjectsQuery>("allprojects",
                        new GetAllProjectsQuery());

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<List<IProject>>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data);

                if (result != null)
                {
                    Output.WriteLine($"All Projects Returned: {result?.Data?.Count.ToString()}");
                }
            }
            finally
            {
                await StopParatextAsync();
            }

        }
    }
}