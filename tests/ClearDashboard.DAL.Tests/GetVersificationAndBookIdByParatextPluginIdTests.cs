using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{

    public class GetVersificationAndBookIdByParatextPluginIdTests : TestBase
    {
        public GetVersificationAndBookIdByParatextPluginIdTests(ITestOutputHelper output) : base(output)
        {
            //no-op
        }

        [Fact]
        public async Task GetVersificationAndBookIdByParatextPluginIdTest()
        {
            var results =
                await ExecuteParatextAndTestRequest<GetVersificationAndBookIdByParatextPluginIdQuery,
                    RequestResult<VersificationBookIds>, VersificationBookIds>(
                    new GetVersificationAndBookIdByParatextPluginIdQuery("3f0f2b0426e1457e8e496834aaa30fce00000002abcdefff"));

            Assert.True(results.Success);
            Assert.NotNull(results.Data);
            Assert.NotNull(results.Data.BookAbbreviations);
            Assert.True(results.Data.Versification.Type == SIL.Scripture.ScrVersType.Original);
        }

        [Fact]
        public async Task GetVersificationAndBookIdByParatextPluginId_zzSUR_Test()
        {
            var results =
                await ExecuteParatextAndTestRequest<GetVersificationAndBookIdByParatextPluginIdQuery,
                    RequestResult<VersificationBookIds>, VersificationBookIds>(
                    new GetVersificationAndBookIdByParatextPluginIdQuery("2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f"));

            Assert.True(results.Success);
            Assert.NotNull(results.Data);
            Assert.NotNull(results.Data.BookAbbreviations);
            Assert.True(results.Data.Versification.Type == SIL.Scripture.ScrVersType.English);
        }
    }
}
