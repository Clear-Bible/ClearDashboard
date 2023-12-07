using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using ClearDashboard.ParatextPlugin.CQRS.Features.Users;

namespace ClearDashboard.DAL.Tests
{
    public class GetAllEditableProjectUsersQueryTest : TestBase
    {
        public GetAllEditableProjectUsersQueryTest(ITestOutputHelper output) : base(output)
        {
            //no-op
        }

        [Fact]
        public async Task GetAllEditableProjectUsersTest()
        {
            var result =
                await ExecuteAndTestRequest<GetAllEditableProjectUsersQuery, RequestResult<List<string>>, List<string>>(
                    new GetAllEditableProjectUsersQuery("2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f"));

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);
            Assert.NotEqual(0, result.Data.Count);
        }

    }
}
