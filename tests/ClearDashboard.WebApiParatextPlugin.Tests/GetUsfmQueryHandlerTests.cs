﻿using System.Net.Http;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.UnifiedScripture;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests;

public class GetUsfmQueryHandlerTests : TestBase
{
    public GetUsfmQueryHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetUsfmTestAsync()
    {
        try
        {
            await StartParatextAsync();
            var client = CreateHttpClient();

            var response = await client.PostAsJsonAsync<GetUsfmQuery>("usfm", new GetUsfmQuery(1));

            Assert.True(response.IsSuccessStatusCode);
            var result = await response.Content.ReadAsAsync<RequestResult<string>>();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            Output.WriteLine(result.Data);
        }
        finally
        {
            await StopParatextAsync();
        }

    }
}