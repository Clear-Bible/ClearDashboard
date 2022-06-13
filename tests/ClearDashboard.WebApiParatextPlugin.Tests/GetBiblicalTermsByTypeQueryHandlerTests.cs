using ClearDashboard.DAL.CQRS;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class GetBiblicalTermsByTypeQueryHandlerTests : TestBase
    {
        public GetBiblicalTermsByTypeQueryHandlerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetAllBiblicalTermsTest()
        {
            try
            {
                await StartParatext();
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/api/");

                var response = await client.PostAsJsonAsync<GetBiblicalTermsByTypeQuery>("biblicalterms", new GetBiblicalTermsByTypeQuery(BiblicalTermsType.All));

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<List<BiblicalTermsData>>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data);

            }
            finally
            {
                await StopParatext();
            }

        }

        [Fact]
        public async Task GetProjectBiblicalTermsTest()
        {
            try
            {
                await StartParatext();
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/api/");

                var response = await client.PostAsJsonAsync<GetBiblicalTermsByTypeQuery>("biblicalterms", new GetBiblicalTermsByTypeQuery(BiblicalTermsType.Project));

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<List<BiblicalTermsData>>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data);
            }
            finally
            {
                await StopParatext();
            }

        }
    }
}
