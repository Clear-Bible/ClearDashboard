using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Features.PINS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class GetTermRenderingsQueryHandlerTest : TestBase
    {
        public GetTermRenderingsQueryHandlerTest([NotNull] ITestOutputHelper output) : base(output)
        {
            // no-op
        }

        [Fact]
        private async Task GetTermRenderingTest()
        {
            string path = Path.Combine(Environment.CurrentDirectory, @"Resources\XML\TermRenderings.xml");

            var result = await ExecuteAndTestRequest<GetTermRenderingsQuery, RequestResult<TermRenderingsList>, TermRenderingsList>(new GetTermRenderingsQuery(path));
        }
    }
}
