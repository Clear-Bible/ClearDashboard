using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Features.PINS;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Paratext;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

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
            string path = Path.Combine(Environment.CurrentDirectory, @"Resources\XML\");
            var dashboardProjectManager = ServiceProvider.GetService<DashboardProjectManager>();
            dashboardProjectManager.CreateDashboardProject();
            dashboardProjectManager.CurrentDashboardProject.DirectoryPath = path;

            var result =
            await ExecuteAndTestRequest<GetTermRenderingsQuery, RequestResult<TermRenderingsList>,
                TermRenderingsList>(new GetTermRenderingsQuery());

            Output.WriteLine($"Returned {result.Data.TermRendering.Count} records.");
        }

    }
}
