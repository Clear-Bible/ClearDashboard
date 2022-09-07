
using ClearDashboard.DAL.CQRS;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class GetCurrentProjectQueryHandlerTests : TestBase
    {


        public GetCurrentProjectQueryHandlerTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override void SetupDependencyInjection()
        {
            base.SetupDependencyInjection();
        }

        [Fact]
        public async Task TestQueryAsync()
        {
            try
            {
                await StartParatextAsync();
                var client = CreateHttpClient();

                var response = await client.PostAsJsonAsync<GetCurrentProjectQuery>("project", new GetCurrentProjectQuery());

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<ParatextProject>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data);
            }
            finally
            {
                await StopParatextAsync();
            }

        }
    }
}
