using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm;
using MediatR;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClearDashboard.DAL.Tests
{
    public class GetUsfmBookByParatextIdBookIdQueryHandlerTest : TestBase
    {
        public GetUsfmBookByParatextIdBookIdQueryHandlerTest(ITestOutputHelper output) : base(output)
        {
            //no-op
        }

        [Fact]
        public async Task GetUsfmBookByParatextIdBookIdTest()
        {
            var results =
                await ExecuteParatextAndTestRequest<GetBookUsfmByParatextIdBookIdQuery,
                    RequestResult<List<UsfmVerse>>, List<UsfmVerse>>(
                    new GetBookUsfmByParatextIdBookIdQuery("3f0f2b0426e1457e8e496834aaa30fce00000002abcdefff", "GEN"));

            Assert.True(results.Success);
            Assert.NotNull(results.Data);
        }

        [Fact]
        public async Task GetUsfmBookByParatextIdBookId_zzSUR_Test()
        {
            var results =
                await ExecuteParatextAndTestRequest<GetBookUsfmByParatextIdBookIdQuery,
                    RequestResult<List<UsfmVerse>>, List<UsfmVerse>>(
                    new GetBookUsfmByParatextIdBookIdQuery("2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f", "GEN"));

            Assert.True(results.Success);
            Assert.NotNull(results.Data);
        }
    }
}
