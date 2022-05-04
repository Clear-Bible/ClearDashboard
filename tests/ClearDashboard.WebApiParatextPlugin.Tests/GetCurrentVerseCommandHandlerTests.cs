using System.Net.Http;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests;

public class GetCurrentVerseCommandHandlerTests : TestBase
{
    public GetCurrentVerseCommandHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetCurrentVerseTest()
    {
        var client = CreateHttpClient();

        var response = await client.PostAsJsonAsync<GetCurrentVerseCommand>("verse",
            new GetCurrentVerseCommand());

        Assert.True(response.IsSuccessStatusCode);
        var result = await response.Content.ReadAsAsync<QueryResult<string>>();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        Output.WriteLine(result.Data);

    }
}