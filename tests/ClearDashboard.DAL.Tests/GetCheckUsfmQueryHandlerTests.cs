using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using ClearDashboard.ParatextPlugin.CQRS.Features.ReferenceUsfm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class GetCheckUsfmQueryHandlerTests : TestBase
    {
        public GetCheckUsfmQueryHandlerTests(ITestOutputHelper output) : base(output)
        {
            //no-op
        }

        [Fact]
        public async Task GetReferenceUsfmTest()
        {
            // get the HEB/GRK sourcelanguage project type
            var result =
                await ExecuteParatextAndTestRequest<GetCheckUsfmQuery, RequestResult<List<UsfmError>>, List<UsfmError>>(
                    new GetCheckUsfmQuery("3f0f2b0426e1457e8e496834aaa30fce00000002abcdefff"));

            Assert.True(result.HasData);
            Assert.NotNull(result.Data);
            //Assert.NotEmpty(result?.Data?.UsfmDirectoryPath);
            //Assert.Equal("Biblia Hebraica Stuttgartensia", result?.Data?.LongName);
        }
    }
}
