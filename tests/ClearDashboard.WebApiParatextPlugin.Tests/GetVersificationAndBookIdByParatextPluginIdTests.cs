using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class GetVersificationAndBookIdByParatextPluginIdTests: TestBase
    {
        public GetVersificationAndBookIdByParatextPluginIdTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetVersificationAndBookIdByParatextPluginId_zzSUR()
        {
            //zzSUR paratext id:"2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f"  
            try
            {
                await StartParatextAsync();
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/api/");

                var response = await client.PostAsJsonAsync<GetVersificationAndBookIdByDalParatextProjectIdQuery>(
                    "versificationbooksbyparatextid",
                    new GetVersificationAndBookIdByDalParatextProjectIdQuery("2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f"));

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<VersificationBookIds>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data.Versification);
                Assert.NotNull(result.Data.BookAbbreviations);
            }
            finally
            {
                await StopParatextAsync();
            }
        }

    }
}
