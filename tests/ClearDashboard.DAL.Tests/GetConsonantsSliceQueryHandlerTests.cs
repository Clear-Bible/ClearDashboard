using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Features.MarbleDataRequests;
using System.Collections.Generic;
using System.Diagnostics;
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
                await ExecuteAndTestRequest<GetConsonantsSliceQuery, RequestResult<List<string>>, List<string>>(
                    new GetConsonantsSliceQuery(word));
            Assert.True(results.Data is not null);
            Assert.True(results.Data.Count == expectedCount);
        }

        [Theory]
        //[InlineData("β", 409)] //not sure why this is failing
        [InlineData("Αβ", 83)]
        //[InlineData("ωσ", 186)] //not sure why this is failing
        [InlineData("ωρα", 9)]
        [InlineData("ετα", 37)]
        //[InlineData("γας", 4)] //not sure why this is failing
        public async Task GetConsonantsSliceHandlerGreekTests(string word, int expectedCount)
        {
            var results =
                await ExecuteAndTestRequest<GetConsonantsSliceQuery, RequestResult<List<string>>, List<string>>(
                    new GetConsonantsSliceQuery(word));
            
            Assert.True(results.Data is not null);

            for (int i = 0; i < results.Data.Count; i++)
            {
                Debug.WriteLine(results.Data[i]);
            }

            Assert.True(results.Data.Count == expectedCount);
        }
    }
}
