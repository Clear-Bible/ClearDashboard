using ClearDashboard.DAL.CQRS;
using System.Net.Http;
using System.Threading.Tasks;
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
        try
        {
            await StartParatext();

            var client = CreateHttpClient();

            var response = await client.PostAsJsonAsync<GetUsxQuery>("usx", new GetUsxQuery());

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

    [Fact]
    public async Task GetUsxBook43Test()
    {
        try
        {
            await StartParatext();
            var client = CreateHttpClient();

            var response = await client.PostAsJsonAsync<GetUsxQuery>("usx", new GetUsxQuery(43));

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