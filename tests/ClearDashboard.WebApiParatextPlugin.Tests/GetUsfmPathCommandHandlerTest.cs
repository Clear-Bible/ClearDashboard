using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using ClearDashboard.ParatextPlugin.CQRS.Features.UsfmFilePath;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class GetUsfmPathCommandHandlerTest : TestBase
    {
        public GetUsfmPathCommandHandlerTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetAllBiblicalTermsTestAsync()
        {
            try
            {
                await StartParatextAsync();
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/api/");

                var paratextId = "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f";
                var response = await client.PostAsJsonAsync<GetUsfmFilePathQuery>("usfmfilepath", new GetUsfmFilePathQuery(paratextId));

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<List<string>>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data);

            }
            finally
            {
                await StopParatextAsync();
            }

        }
         
    }
}
