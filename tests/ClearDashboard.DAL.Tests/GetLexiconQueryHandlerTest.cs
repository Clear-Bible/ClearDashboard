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


    public class GetLexiconQueryHandlerTest : TestBase
    {
        public GetLexiconQueryHandlerTest([NotNull] ITestOutputHelper output) : base(output)
        {
            // no-op
        }

        [Fact]
        private async Task GetLexiconQueryTest()
        {
            string path = Path.Combine(Environment.CurrentDirectory, @"Resources\XML\Lexicon.xml");

            var result = await ExecuteAndTestRequest<GetLexiconQuery, RequestResult<Lexicon>, Lexicon>(new GetLexiconQuery(path));
        }
    }
}
