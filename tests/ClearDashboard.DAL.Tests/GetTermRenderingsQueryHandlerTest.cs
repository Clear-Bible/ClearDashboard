using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Features.PINS;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class GetTermRenderingsQueryHandlerTest : TestBase
    {
        #nullable disable


        public GetTermRenderingsQueryHandlerTest([NotNull] ITestOutputHelper output) : base(output)
        {
            // no-op
        }

        [Fact]
        private async Task GetTermRenderingTest()
        {
            string path = Path.Combine(Environment.CurrentDirectory, @"Resources\XML\");
            var dashboardProjectManager = ServiceProvider.GetService<DashboardProjectManager>();
            dashboardProjectManager.CreateDashboardProject();

            dashboardProjectManager.CurrentParatextProject = new ParatextProject();
            dashboardProjectManager.CurrentParatextProject.DirectoryPath = path;

            var result =
            await ExecuteAndTestRequest<GetTermRenderingsQuery, RequestResult<TermRenderingsList>,
                TermRenderingsList>(new GetTermRenderingsQuery());

            Output.WriteLine($"Returned {result.Data.TermRendering.Count} records.");
        }

    }
}
