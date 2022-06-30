using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Features.PINS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
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
            string path = Path.Combine(Environment.CurrentDirectory, @"Resources\XML\");
            var dashboardProjectManager = ServiceProvider.GetService<DashboardProjectManager>();
            dashboardProjectManager.CreateDashboardProject();
            dashboardProjectManager.CurrentDashboardProject.DirectoryPath = path;

            var result = await ExecuteAndTestRequest<GetLexiconQuery, RequestResult<Lexicon>, Lexicon>(new GetLexiconQuery());

            Output.WriteLine($"Returned {result.Data.Entries.Item.Count} records.");
        }
    }
}
