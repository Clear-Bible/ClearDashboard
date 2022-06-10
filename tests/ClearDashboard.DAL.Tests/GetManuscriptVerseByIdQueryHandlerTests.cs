using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Features.ManuscriptVerses;
using ClearDashboard.DataAccessLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class GetManuscriptVerseByIdQueryHandlerTests : TestBase
    {
        public GetManuscriptVerseByIdQueryHandlerTests(ITestOutputHelper output) : base(output)
        {
            //no-op
        }

      
        [Fact]
        public async Task GetManuscriptVerseByIdHandlerTest()
        {
            var results = await ExecuteAndTestRequest<GetManuscriptVerseByIdQuery, RequestResult<List<CoupleOfStrings>>, List<CoupleOfStrings>>(new GetManuscriptVerseByIdQuery("040005015"));
            Assert.True(results.Data.Count == 48);
        }
    }
}
