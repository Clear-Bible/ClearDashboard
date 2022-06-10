using ClearDashboard.DAL.CQRS;
using System.Net.Http;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.ParatextPlugin.CQRS.Features.UnifiedScripture;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests;

public class GetUsxQueryHandlerTests : TestBase
{
    public GetUsxQueryHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetUsxTest()
    {
        var client = CreateHttpClient();

        var response = await client.PostAsJsonAsync<GetUsxQuery>("usx", new GetUsxQuery(1));

        Assert.True(response.IsSuccessStatusCode);
        var result = await response.Content.ReadAsAsync<RequestResult<UsxObject>>();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        Output.WriteLine(result.Data.USX);

    }
}