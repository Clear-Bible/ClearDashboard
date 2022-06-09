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
using ClearDashboard.DataAccessLayer.Wpf;
using Microsoft.Extensions.DependencyInjection;
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
            string path = Path.Combine(Environment.CurrentDirectory, @"Resources\XML\");
            var dashboardProjectManager = ServiceProvider.GetService<DashboardProjectManager>();
            dashboardProjectManager.CreateDashboardProject();
            dashboardProjectManager.CurrentDashboardProject.DirectoryPath = path;
            var result = await ExecuteAndTestRequest<GetSpellingStatusQuery, RequestResult<SpellingStatus>, SpellingStatus>(new GetSpellingStatusQuery());

            Output.WriteLine($"Returned {result.Data.Status.Count} records.");
        }
    }
}
