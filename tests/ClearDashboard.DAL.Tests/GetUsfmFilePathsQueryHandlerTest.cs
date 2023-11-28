using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Features.PINS;
using ClearDashboard.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Annotations;
using Xunit.Abstractions;
using Xunit;
using ClearDashboard.ParatextPlugin.CQRS.Features.UsfmFilePath;

namespace ClearDashboard.DAL.Tests
{
    public class GetUsfmFilePathsQueryHandlerTest : TestBase
    {
#nullable disable

        public GetUsfmFilePathsQueryHandlerTest([NotNull] ITestOutputHelper output) : base(output)
        {
            // no-op
        }

        [Fact]
        private async Task GetUsfmFilePathsTest()
        {
            var paratextId = "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f";

            var result =
                await ExecuteAndTestRequest<GetUsfmFilePathQuery, RequestResult<List<string>>, List<string>>(
                    new GetUsfmFilePathQuery(paratextId));

            Assert.True(result.HasData);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result?.Data);

        }


    }
}
