using System.Net.Http;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.User;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class GetCurrentParatextUserQueryHandlerTests : TestBase
    {
        #nullable disable

        public GetCurrentParatextUserQueryHandlerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetCurrentParatextUserTestAsync()
        {
            try
            {
                await StartParatextAsync();
                var client = CreateHttpClient();

                var response = await client.PostAsJsonAsync<GetCurrentParatextUserQuery>("user", new GetCurrentParatextUserQuery());

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<AssignedUser>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data);

                Output.WriteLine(result.Data.Name);
            }
            finally
            {
                await StopParatextAsync();
            }

        }
    }


}
