using System.Net.Http;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests;

public class GetCurrentVerseQueryHandlerTests : TestBase
{
    public GetCurrentVerseQueryHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetCurrentVerseTestAsync()
    {
        try
        {
            await StartParatext();

            var client = CreateHttpClient();

            var response = await client.PostAsJsonAsync<GetCurrentVerseQuery>("verse",
                new GetCurrentVerseQuery());

            Assert.True(response.IsSuccessStatusCode);
            var result = await response.Content.ReadAsAsync<RequestResult<string>>();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            Output.WriteLine(result.Data);
        }
        finally
        {
            await StopParatext();
        }
    }
}