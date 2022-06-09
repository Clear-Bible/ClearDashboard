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
using System.Threading;
using System.Threading.Tasks;
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
            // TODO currently broken while we need a mocked up ProjectManager

            //var result =
            //    await ExecuteAndTestRequest<GetTermRenderingsQuery, RequestResult<TermRenderingsList>,
            //        TermRenderingsList>(new GetTermRenderingsQuery());
        }

    }
}
