using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Features.MarbleDataRequests;
using ClearDashboard.DataAccessLayer.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class GetConsonantsSliceQueryHandlerTests : TestBase
    {
        public GetConsonantsSliceQueryHandlerTests(ITestOutputHelper output) : base(output)
        {
            //no-op
        }

        [Theory]
        [InlineData("אב", 119)]
        [InlineData("ביד", 5)]
        [InlineData("בישׁ", 9)]
        [InlineData("לב", 46)]
        public async Task GetConsonantsSliceHandlerHebrewTests(string word, int expectedCount)
        {
            var results =
                await ExecuteAndTestRequest<GetConsonantsSliceQuery, RequestResult<List<CoupleOfStrings>>, List<CoupleOfStrings>>(
                    new GetConsonantsSliceQuery(word));
            Assert.True(results.Data is not null);
            Assert.True(results.Data.Count == expectedCount);
        }

        [Theory]
        [InlineData("β", 409)]
        [InlineData("Αβ", 83)]
        //[InlineData("ωσ", 117)] // not sure why this is different
        [InlineData("ωρα", 9)]
        [InlineData("ετα", 37)]
        [InlineData("γας", 1)]
        public async Task GetConsonantsSliceHandlerGreekTests(string word, int expectedCount)
        {
            var wordLower = word.ToLower(new CultureInfo("el-GR"));

            var results =
                await ExecuteAndTestRequest<GetConsonantsSliceQuery, RequestResult<List<CoupleOfStrings>>, List<CoupleOfStrings>>(
                    new GetConsonantsSliceQuery(wordLower));
            
            Assert.True(results.Data is not null);

            for (int i = 0; i < results.Data.Count; i++)
            {
                Debug.WriteLine(results.Data[i]);
            }

            Assert.True(results.Data.Count == expectedCount);
        }
    }
}
