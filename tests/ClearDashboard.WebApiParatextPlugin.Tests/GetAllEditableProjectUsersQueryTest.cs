using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Users;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class GetAllEditableProjectUsersQueryTest : TestBase
    {
        public GetAllEditableProjectUsersQueryTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetAllEditableProjectUsersQueryAsync()
        {
            try
            {
                await StartParatextAsync();
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/api/");

                var response = await client.PostAsJsonAsync<GetAllEditableProjectUsersQuery>("GetAllEditableProjectUsers", new GetAllEditableProjectUsersQuery("2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f"));

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<List<string>>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data);
                Assert.NotEqual(0, result.Data.Count);

            }
            finally
            {
                await StopParatextAsync();
            }

        }
    }
}
