using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Features.ManuscriptVerses;
using ClearDashboard.DataAccessLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.ParatextPlugin.CQRS.Features.TextCollections;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class GetTextCollectionsQueryHandlerTest : TestBase
    {
#nullable disable
        public GetTextCollectionsQueryHandlerTest(ITestOutputHelper output) : base(output)
        {
            //no-op
        }


        [Fact]
        public async Task GetTextCollections()
        {
            var results = await ExecuteParatextAndTestRequest<GetTextCollectionsQuery, RequestResult<List<TextCollection>>, List<TextCollection>>(new GetTextCollectionsQuery());
            Assert.True(results.Data.Count == 48);
        }
    }
}
