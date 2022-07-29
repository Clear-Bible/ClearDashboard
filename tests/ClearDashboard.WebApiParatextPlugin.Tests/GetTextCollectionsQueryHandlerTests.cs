using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.ParatextPlugin.CQRS.Features.TextCollections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class GetTextCollectionsQueryHandlerTests : TestBase
    {
        public GetTextCollectionsQueryHandlerTests(ITestOutputHelper output) : base(output)
        {
        }


        [Fact] 
        public async Task GetTextCollectionsTestAsync()
        {
            try
            {
                await StartParatextAsync();
                var client = CreateHttpClient();

                var response = await client.PostAsJsonAsync<GetTextCollectionsQuery>("textcollections", new GetTextCollectionsQuery());

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<List<TextCollection>>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data);

                if (result != null)
                {
                    Output.WriteLine($"TextCollections Returned: {result?.Data?.Count.ToString()}");
                }
            }
            finally
            {
                await StopParatextAsync();
            }

        }
    }
}
