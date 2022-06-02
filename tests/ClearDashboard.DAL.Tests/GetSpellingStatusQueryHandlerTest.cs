using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Features.PINS;
using ClearDashboard.DataAccessLayer.Models.Common;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    internal class GetSpellingStatusQueryHandlerTest
    {
    }

    public class GetSpellingStatusQueryHandler : TestBase
    {
        public GetSpellingStatusQueryHandler([NotNull] ITestOutputHelper output) : base(output)
        {
            // no-op
        }

        [Fact]
        private async Task GetSpellingStatusQueryTest()
        {
            string path = Path.Combine(Environment.CurrentDirectory, @"Resources\XML\SpellingStatus.xml");

            var result = await ExecuteAndTestRequest<GetSpellingStatusQuery, RequestResult<SpellingStatus>, SpellingStatus>(new GetSpellingStatusQuery(path));
        }
    }
}
