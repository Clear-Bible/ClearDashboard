using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using ClearDashboard.ParatextPlugin.CQRS.Features.ReferenceUsfm;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class GerReferenceUsfmQueryHandlerTests : TestBase
    {
        public GerReferenceUsfmQueryHandlerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetReferenceUsfmTestAsync()
        {
            try
            {
                await StartParatextAsync();
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/api/");

                var response = await client.PostAsJsonAsync<GetReferenceUsfmQuery>("referenceusfm",
                    new GetReferenceUsfmQuery("3f0f2b0426e1457e8e496834aaa30fce00000002abcdefff"));

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<ReferenceUsfm>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data);

                Assert.NotEmpty(result?.Data?.UsfmDirectoryPath);
                Assert.Equal("Biblia Hebraica Stuttgartensia", result?.Data?.LongName);

            }
            finally
            {
                await StopParatextAsync();
            }

        }

    }
}
