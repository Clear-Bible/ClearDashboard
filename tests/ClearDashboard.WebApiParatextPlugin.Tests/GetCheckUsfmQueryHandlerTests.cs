using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class GetCheckUsfmQueryHandlerTests : TestBase
    {
        public GetCheckUsfmQueryHandlerTests(ITestOutputHelper output) : base(output)
        {
            //no-op
        }

        [Fact]
        public async Task GetCheckUsfmTestAsync()
        {
            try
            {
                await StartParatextAsync();
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/api/");

                var response = await client.PostAsJsonAsync<GetCheckUsfmQuery>("checkusfm",
                    new GetCheckUsfmQuery("3f0f2b0426e1457e8e496834aaa30fce00000002abcdefff"));

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<UsfmHelper>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data);

                Assert.NotEmpty(result?.Data?.Path);
            }
            finally
            {
                await StopParatextAsync();
            }

        }
    }
}
